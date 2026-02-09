using UnityEngine;

namespace SerpentsQuest.Food
{
    [DisallowMultipleComponent]
    public sealed class FoodRipenessTint : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private FoodIdentity identity;
        [SerializeField] private Renderer[] renderers;

        [Header("Ripeness Color Control")]
        [Tooltip("원래 색에 곱해지는 밝기(1=그대로, 0=검정)")]
        [Range(0f, 2f)] public float unripeBrightness = 1.15f;
        [Range(0f, 2f)] public float ripeBrightness = 1.00f;
        [Range(0f, 2f)] public float overripeBrightness = 0.90f;
        [Range(0f, 2f)] public float rottenBrightness = 0.75f;

        [Tooltip("원래 색에 추가로 곱해지는 색 틴트(흰색=변화 없음)")]
        public Color unripeTint = new Color(0.48f, 0.9f, 0.15f);// Color.white;     // 예: 약간 초록빛 주고 싶으면 (0.9,1.05,0.9)
        public Color ripeTint = Color.white;
        public Color overripeTint = new Color(0.70f, 0.30f, 0.10f); //Color.white;   // 예: 살짝 갈색 기
        public Color rottenTint = new Color(0.65f, 0.65f, 0.65f);     // 어둡게만 할거면 white 그대로

        [Tooltip("스폰 시 1회 적용 권장")]
        public bool applyOnAwake = true;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor"); // URP Lit
        static readonly int ColorId = Shader.PropertyToID("_Color");         // Standard 계열

        MaterialPropertyBlock _mpb;

        private void Reset()
        {
            if (identity == null) identity = GetComponentInParent<FoodIdentity>();
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void Awake()
        {
            if (applyOnAwake) Apply();
        }

        private void OnEnable()
        {
            // 오브젝트 풀링(재사용)까지 고려하면 OnEnable에서 한 번 더 적용하는 게 안전
            if (applyOnAwake == false) return;
            Apply();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying && applyOnAwake)
                Apply();
        }

        public void Apply()
        {
            if (identity == null) identity = GetComponentInParent<FoodIdentity>();
            if (identity == null) return;

            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>(true);

            if (_mpb == null) _mpb = new MaterialPropertyBlock();

            // 숙성도별 파라미터 선택
            float brightness;
            Color tint;
            switch (identity.ripeness)
            {
                case Ripeness.Unripe:
                    brightness = unripeBrightness;
                    tint = unripeTint;
                    break;
                case Ripeness.Ripe:
                    brightness = ripeBrightness;
                    tint = ripeTint;
                    break;
                case Ripeness.Overripe:
                    brightness = overripeBrightness;
                    tint = overripeTint;
                    break;
                case Ripeness.Rotten:
                    brightness = rottenBrightness;
                    tint = rottenTint; // rotten은 same material + 어둡게만이면 white로 두면 됨
                    break;
                default:
                    brightness = 1f;
                    tint = Color.white;
                    break;
            }

            foreach (var r in renderers)
            {
                if (r == null) continue;

                // sharedMaterial에서 "원래 색" 가져오기 (메쉬마다 달라도 OK)
                var mat = r.sharedMaterial;
                Color baseCol = Color.white;

                if (mat != null)
                {
                    if (mat.HasProperty(BaseColorId)) baseCol = mat.GetColor(BaseColorId);
                    else if (mat.HasProperty(ColorId)) baseCol = mat.GetColor(ColorId);
                }

                // 최종 색 = 원래색 * 밝기 * 틴트
                Color final = baseCol * brightness;
                final.r *= tint.r; final.g *= tint.g; final.b *= tint.b;
                final.a = baseCol.a;

                _mpb.Clear();
                if (mat != null && mat.HasProperty(BaseColorId)) _mpb.SetColor(BaseColorId, final);
                else _mpb.SetColor(ColorId, final);

                r.SetPropertyBlock(_mpb);
            }
        }
    }
}
