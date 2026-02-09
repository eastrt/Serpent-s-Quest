using UnityEngine;

namespace SerpentsQuest.Food
{
    [DisallowMultipleComponent]
    public sealed class FoodTintByRipeness : MonoBehaviour
    {
        [SerializeField] private FoodIdentity identity;
        [SerializeField] private Renderer[] renderers;

        [Header("Darken Settings")]
        [Range(0f, 1f)] public float rottenBrightness = 0.45f; // 0.45면 꽤 어둡게
        public bool applyOnAwake = true;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        MaterialPropertyBlock _mpb;

        private void Reset()
        {
            if (identity == null) identity = GetComponentInParent<FoodIdentity>();
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void Awake()
        {
            if (!applyOnAwake) return;
            Apply();
        }

        private void OnValidate()
        {
            if (!applyOnAwake) return;
            if (!Application.isPlaying) Apply();
        }

        public void Apply()
        {
            if (identity == null) identity = GetComponentInParent<FoodIdentity>();
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>(true);

            if (_mpb == null) _mpb = new MaterialPropertyBlock();

            bool isRotten = identity != null && identity.ripeness == Ripeness.Rotten;

            foreach (var r in renderers)
            {
                if (r == null) continue;

                r.GetPropertyBlock(_mpb);

                // 현재 머티리얼의 컬러를 기준으로 어둡게 만들기
                // (URP Lit는 보통 _BaseColor, Standard는 _Color)
                Color src = Color.white;

                var mat = r.sharedMaterial;
                if (mat != null)
                {
                    if (mat.HasProperty(BaseColorId)) src = mat.GetColor(BaseColorId);
                    else if (mat.HasProperty(ColorId)) src = mat.GetColor(ColorId);
                }

                if (isRotten)
                {
                    Color dst = src * rottenBrightness; // 곱해서 어둡게
                    dst.a = src.a;

                    if (mat != null && mat.HasProperty(BaseColorId)) _mpb.SetColor(BaseColorId, dst);
                    else _mpb.SetColor(ColorId, dst);
                }
                else
                {
                    // Rotten이 아니면 PropertyBlock 컬러 제거(원래 머티리얼 그대로)
                    _mpb.Clear();
                }

                r.SetPropertyBlock(_mpb);
            }
        }
    }
}
