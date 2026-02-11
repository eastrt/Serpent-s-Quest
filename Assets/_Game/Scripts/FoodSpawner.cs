using System.Collections.Generic;
using UnityEngine;
using SerpentsQuest.Food;
using Unity.VisualScripting; // FruitType, Ripeness (FoodIdentity에 사용)

public class FoodSpawner : MonoBehaviour
{
    [Header("Spawn Area")]
    [SerializeField] private BoxCollider spawnArea;

    [Header("Prefabs")]
    [SerializeField] private GameObject applePrefab;
    [SerializeField] private GameObject bananaPrefab;      // 일반 Banana (부스트 없음)
    [SerializeField] private GameObject dummyBanaPrefab;   // DummyBana (필드 최대 1)

    [Header("Counts (Stage)")]
    [SerializeField] private int totalCount = 10;          // 스테이지별 변경
    [SerializeField] private int bananaFamilyCount = 3;    // 10개 중 3개 유지(요구사항)

    [Header("Dummy Rule")]
    [SerializeField] private int bananasPerDummy = 10;     // Banana 10개 먹으면 Dummy 등장
    [SerializeField] private int maxActiveDummy = 1;       // Dummy는 항상 1개만

    [Header("Placement")]
    [SerializeField] private float foodRadius = 0.35f;
    [SerializeField] private float spawnHeightRayStart = 50f;
    [SerializeField] private float spawnHeightOffset = 0.05f;
    [SerializeField] private int maxTryPerSpawn = 40;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask blockerLayers;
    [SerializeField] private LayerMask snakeLayers;

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 0.05f;

    private readonly List<GameObject> aliveFoods = new();

    // 현재 필드 재고
    private int activeApple;
    private int activeBanana;
    private int activeDummy;

    // Banana 10개 먹으면 Dummy를 “예약”
    private int bananaEatCounter = 0;
    private bool dummyPending = false;  // true면 "바나나 자리 하나"를 Dummy로 바꿈

    private void Reset()
    {
        spawnArea = GetComponent<BoxCollider>();
        if (spawnArea != null) spawnArea.isTrigger = true;
    }

    private void Awake()
    {
        if (spawnArea == null) spawnArea = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        RecountFromScene();
        EnsureFoodCount();
    }

    // 스테이지 바뀔 때 호출(원하면 외부에서)
    public void SetStageCounts(int stageTotal, int stageBananaFamily)
    {
        totalCount = Mathf.Max(0, stageTotal);
        bananaFamilyCount = Mathf.Clamp(stageBananaFamily, 0, totalCount);
        EnsureFoodCount();
    }

    public void NotifyFoodRemoved(FruitType fruit)
    {
        // 재고 감소 + Banana 누적 카운트
        switch (fruit)
        {
            case FruitType.Apple:
                activeApple = Mathf.Max(0, activeApple - 1);
                break;

            case FruitType.Banana:
                activeBanana = Mathf.Max(0, activeBanana - 1);

                bananaEatCounter++;
                if (bananaEatCounter >= bananasPerDummy)
                {
                    bananaEatCounter -= bananasPerDummy;
                    dummyPending = true; // ✅ 다음 “바나나 자리”를 Dummy로 치환
                }
                break;

            case FruitType.DummyBana:
                activeDummy = Mathf.Max(0, activeDummy - 1);
                // Dummy를 먹으면 다시 기본 구성(바나나 3개)으로 돌아가게:
                dummyPending = false;
                break;
        }

        CleanupNulls();
        CancelInvoke(nameof(EnsureFoodCount));
        Invoke(nameof(EnsureFoodCount), respawnDelay);
    }

    private void EnsureFoodCount()
    {
        CleanupNulls();

        // 목표치 계산
        int targetTotal = Mathf.Max(0, totalCount);
        int targetBananaFamily = Mathf.Clamp(bananaFamilyCount, 0, targetTotal);
        int targetApple = targetTotal - targetBananaFamily;

        // Dummy가 등장해야 하는 상태면: 바나나 계열 = Banana 2 + Dummy 1 (단, Dummy는 최대 1)
        int targetDummy = 0;
        int targetBanana = targetBananaFamily;
        // pending일 때만 Dummy 목표를 1로 잡는다.
        // 단, 이미 Dummy가 있으면 새로 만들지 않는다.
        if (dummyPending && activeDummy == 0)
        {
            targetDummy = Mathf.Min(maxActiveDummy, 1); // 항상 1개만
            targetBanana = Mathf.Max(0, targetBananaFamily - targetDummy); // 3-1=2
        }

        // (안전) Dummy가 이미 있는데 또 만들지 않게
        if (activeDummy >= maxActiveDummy) targetDummy = 0;

        // 부족분만큼 채우기
        int safety = 500;
        while ((activeApple + activeBanana + activeDummy) < targetTotal && safety-- > 0)
        {
            // 1) Dummy 우선(필요할 때만)
            if (targetDummy > 0 && activeDummy < targetDummy)
            {
                SpawnOne(dummyBanaPrefab, FruitType.DummyBana);
                // ✅ Dummy를 “한 번” 스폰했으니 pending 해제
                dummyPending = false;
                continue;
            }

            // 2) Banana 채우기
            if (activeBanana < targetBanana)
            {
                SpawnOne(bananaPrefab, FruitType.Banana);
                continue;
            }

            // 3) Apple 채우기
            if (activeApple < targetApple)
            {
                SpawnOne(applePrefab, FruitType.Apple);
                continue;
            }

            // 혹시라도 계산이 딱 맞는데도 루프 도는 경우 방지
            break;
        }
    }

    private void SpawnOne(GameObject prefab, FruitType fruit)
    {
        if (prefab == null) return;

        for (int i = 0; i < maxTryPerSpawn; i++)
        {
            Vector3 randomPoint = GetRandomPointInBox(spawnArea);

            if (!TryProjectToGround(randomPoint, out Vector3 onGround)) continue;
            if (IsOverlapping(onGround)) continue;

            GameObject food = Instantiate(prefab, onGround, Quaternion.identity);

            // FoodIdentity 설정(없으면 붙임)
            var identity = food.GetComponent<FoodIdentity>();
            if (identity == null) identity = food.AddComponent<FoodIdentity>();
            identity.fruit = fruit;
            identity.ripeness = Ripeness.Ripe;

            // 파괴 감지 → 스포너에게 통지
            var notifier = food.GetComponent<FoodSpawnedNotifier>();
            if (notifier == null) notifier = food.AddComponent<FoodSpawnedNotifier>();
            notifier.Init(this, fruit);

            aliveFoods.Add(food);

            // 카운트 증가
            if (fruit == FruitType.Apple) activeApple++;
            else if (fruit == FruitType.Banana) activeBanana++;
            else if (fruit == FruitType.DummyBana) activeDummy++;

            return;
        }
    }

    private void RecountFromScene()
    {
        aliveFoods.Clear();
        activeApple = activeBanana = activeDummy = 0;

        var foods = FindObjectsByType<FoodSpawnedNotifier>(FindObjectsSortMode.None);
        foreach (var n in foods)
        {
            if (n == null) continue;
            aliveFoods.Add(n.gameObject);

            if (n.Fruit == FruitType.Apple) activeApple++;
            else if (n.Fruit == FruitType.Banana) activeBanana++;
            else if (n.Fruit == FruitType.DummyBana) activeDummy++;
        }
    }

    private void CleanupNulls()
    {
        for (int i = aliveFoods.Count - 1; i >= 0; i--)
            if (aliveFoods[i] == null) aliveFoods.RemoveAt(i);
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
        return Physics.CheckSphere(pos, foodRadius, mask, QueryTriggerInteraction.Ignore);
    }
}

/// <summary>
/// Food 파괴 시 스포너에 통지
/// </summary>
public class FoodSpawnedNotifier : MonoBehaviour
{
    private FoodSpawner spawner;
    public FruitType Fruit { get; private set; }

    public void Init(FoodSpawner spawner, FruitType fruit)
    {
        this.spawner = spawner;
        Fruit = fruit;
    }

    private void OnDestroy()
    {
        if (spawner != null)
            spawner.NotifyFoodRemoved(Fruit);
    }
}
