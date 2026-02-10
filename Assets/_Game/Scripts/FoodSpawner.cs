using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Spawn Area (BoxCollider)")]
    [SerializeField] private BoxCollider spawnArea;

    [Header("Food Prefabs")]
    [SerializeField] private GameObject[] foodPrefabs;

    [Header("Counts")]
    [SerializeField] private int targetFoodCount = 1;

    [Header("Placement")]
    [SerializeField] private float foodRadius = 0.35f;                 // 충돌/겹침 체크 반경
    [SerializeField] private float spawnHeightRayStart = 50f;          // 레이 시작 높이
    [SerializeField] private float spawnHeightOffset = 0.05f;          // 바닥에서 살짝 띄우기
    [SerializeField] private int maxTryPerSpawn = 40;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask blockerLayers;                 // 벽/장애물
    [SerializeField] private LayerMask snakeLayers;                   // 뱀(머리/몸통)

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 0.15f;

    private readonly List<GameObject> aliveFoods = new();

    private void Reset()
    {
        spawnArea = GetComponent<BoxCollider>();
        if (spawnArea != null) spawnArea.isTrigger = true;
    }

    private void Awake()
    {
        if (spawnArea == null)
            spawnArea = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        EnsureFoodCount();
    }

    private void EnsureFoodCount()
    {
        CleanupNulls();


        //while (aliveFoods.Count < targetFoodCount)
        for (int i = 0; i < targetFoodCount - aliveFoods.Count; i++)
        {
            SpawnOne();
        }
    }

    private void CleanupNulls()
    {
        for (int i = aliveFoods.Count - 1; i >= 0; i--)
        {
            if (aliveFoods[i] == null) aliveFoods.RemoveAt(i);
        }
    }

    private void SpawnOne()
    {
        if (foodPrefabs == null || foodPrefabs.Length == 0) return;

        for (int i = 0; i < maxTryPerSpawn; i++)
        {
            Vector3 randomPoint = GetRandomPointInBox(spawnArea);

            // Debug 
            GameObject prefabDebug = PickFoodPrefab();  // only debug
            Debug.Log(
                $"FoodSpawner Spawn Try #{i} | Pos={randomPoint:F2} | Prefab={prefabDebug.name}"
            );


            // 바닥 투영 체크
            if (!TryProjectToGround(randomPoint, out Vector3 onGround))
            {
                Debug.Log($"FoodSpawner : 바닥 투영으로 생성 실패 {onGround.ToString("F2")}");
                continue;
            }
            //Debug.Log($"FoodSpawner : {onGround.ToString("F2")}");
            // 겹침 체크(벽/장애물 + 뱀)
            if (IsOverlapping(onGround))
            {
                Debug.Log($"FoodSpawner : 겹침 체크로 생성 실패 {onGround.ToString("F2")}");
                continue;
            }


            GameObject prefab = PickFoodPrefab();
            GameObject food = Instantiate(prefab, onGround, Quaternion.identity);

            // 먹히면 스포너에게 알려주기(아래 FoodSpawnedNotifier 참고)
            var notifier = food.GetComponent<FoodSpawnedNotifier>();
            if (notifier == null) notifier = food.AddComponent<FoodSpawnedNotifier>();
            notifier.Init(this);

            aliveFoods.Add(food);
            return;
        }

        // 실패했으면(너무 빽빽한 맵) 아무것도 안 하고 종료
        // 필요하면 여기서 경고 로그
        // Debug.LogWarning("FoodSpawner: Failed to find valid spawn position.");
    }

    private GameObject PickFoodPrefab()
    {
        // 지금 구조에선 프리팹 이름에 Apple/Banana가 들어가면 FoodType이 자동 분기하니까
        // 스포너는 그냥 랜덤 선택만 하면 됨.

        int idx = Random.Range(0, foodPrefabs.Length);
        return foodPrefabs[idx];
    }

    private Vector3 GetRandomPointInBox(BoxCollider box)
    {
        Vector3 center = box.transform.TransformPoint(box.center);
        Vector3 size = Vector3.Scale(box.size, box.transform.lossyScale);

        float x = Random.Range(-size.x * 0.5f, size.x * 0.5f);
        float y = Random.Range(-size.y * 0.5f, size.y * 0.5f);
        float z = Random.Range(-size.z * 0.5f, size.z * 0.5f);

        return center + box.transform.right * x + box.transform.up * y + box.transform.forward * z;
    }

    private bool TryProjectToGround(Vector3 point, out Vector3 onGround)
    {
        Vector3 rayOrigin = new Vector3(point.x, point.y + spawnHeightRayStart, point.z);

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, spawnHeightRayStart * 2f, groundLayer))
        {
            onGround = hit.point + Vector3.up * spawnHeightOffset;
            return true;
        }

        onGround = default;
        return false;
    }

    private bool IsOverlapping(Vector3 pos)
    {
        int mask = blockerLayers | snakeLayers;
        // sphere overlap 체크
        return Physics.CheckSphere(pos, foodRadius, mask, QueryTriggerInteraction.Ignore);
    }

    // Food가 먹히거나 파괴될 때 호출
    public void NotifyFoodRemoved(GameObject food)
    {
        aliveFoods.Remove(food);
        CancelInvoke(nameof(EnsureFoodCount));
        Invoke(nameof(EnsureFoodCount), respawnDelay);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (spawnArea == null) return;
        Gizmos.matrix = spawnArea.transform.localToWorldMatrix;
        Gizmos.DrawWireCube(spawnArea.center, spawnArea.size);
    }
#endif
}

/// <summary>
/// Food가 Destroy될 때 스포너에 알려주는 경량 컴포넌트.
/// (먹기 로직(HeadEatFood)이 food를 Destroy 해도 자동으로 리스폰됨)
/// </summary>
public class FoodSpawnedNotifier : MonoBehaviour
{
    private FoodSpawner spawner;

    public void Init(FoodSpawner spawner) => this.spawner = spawner;

    private void OnDestroy()
    {
        if (spawner != null)
            spawner.NotifyFoodRemoved(gameObject);
    }
}
