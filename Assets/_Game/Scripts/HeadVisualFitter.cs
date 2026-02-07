using UnityEngine;

public class HeadVisualFitter : MonoBehaviour
{
    [Header("Refs")]
    public Transform visualRoot;
    public CapsuleCollider hitCollider;

    [Header("Normalize")]
    public float targetWidth = 1.0f;
    public bool autoScale = true;
    public bool autoCollider = true;

    public void SetModel(GameObject modelPrefab)
    {


        // 기존 모델 제거
        for (int i = visualRoot.childCount - 1; i >= 0; i--)
            Destroy(visualRoot.GetChild(i).gameObject);

        // 새 모델 생성
        var model = Instantiate(modelPrefab, visualRoot);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        // bounds 계산
        var bounds = CalculateBounds(model);
        if (bounds.size == Vector3.zero)
        {
            Debug.LogWarning("Model has no renderers. Bounds is zero.");
            return;
        }

        // 스케일 통일 (가로폭 기준)
        if (autoScale)
        {
            float width = bounds.size.x;
            if (width > 0.0001f)
            {
                float s = targetWidth / width;
                model.transform.localScale = Vector3.one * s;
                bounds = CalculateBounds(model);
            }
        }

        // 중심 정렬
        var localCenter = visualRoot.InverseTransformPoint(bounds.center);
        model.transform.localPosition = -localCenter;

        // 콜라이더 자동 맞춤
        if (autoCollider && hitCollider != null)
        {
            FitCapsuleToBounds(bounds, hitCollider, visualRoot);
        }
    }

    Bounds CalculateBounds(GameObject go)
    {
        // ✅ inactive 자식도 포함해서 Renderer 검색
        var renderers = go.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);
            return b;
        }

        // ✅ Renderer가 아예 없으면 MeshFilter로 한 번 더 시도 (fallback)
        var filters = go.GetComponentsInChildren<MeshFilter>(true);
        if (filters.Length > 0)
        {
            // MeshFilter.sharedMesh.bounds는 "로컬 bounds"라서
            // transform을 고려해 월드 bounds로 변환해서 합침
            bool hasAny = false;
            Bounds wb = new Bounds();

            foreach (var f in filters)
            {
                if (f.sharedMesh == null) continue;

                var lb = f.sharedMesh.bounds;
                // 로컬 bounds의 중심/크기를 월드로 근사 변환
                var center = f.transform.TransformPoint(lb.center);
                var size = Vector3.Scale(lb.size, f.transform.lossyScale);

                var b = new Bounds(center, size);
                if (!hasAny) { wb = b; hasAny = true; }
                else wb.Encapsulate(b);
            }

            if (hasAny) return wb;
        }

        return new Bounds(Vector3.zero, Vector3.zero);
    }


    void FitCapsuleToBounds(Bounds worldBounds, CapsuleCollider capsule, Transform reference)
    {
        Vector3 centerLocal = reference.InverseTransformPoint(worldBounds.center);
        Vector3 sizeLocal = reference.InverseTransformVector(worldBounds.size);

        capsule.direction = 1; // Y
        capsule.center = centerLocal;

        float radius = Mathf.Max(sizeLocal.x, sizeLocal.z) * 0.5f;
        float height = Mathf.Max(sizeLocal.y, radius * 2f);

        capsule.radius = radius;
        capsule.height = height;
    }
}
