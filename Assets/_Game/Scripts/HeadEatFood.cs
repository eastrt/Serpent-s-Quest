using UnityEngine;
using SerpentsQuest.Food; // FoodIdentity, FruitType, Ripeness, IFoodConsumer

public class HeadEatFood : MonoBehaviour, IFoodConsumer
{
    [Header("Refs (drag in Inspector)")]
    [SerializeField] private SnakeTrailFollower trail;   // 몸통관리
    [SerializeField] private FoodSpawner spawner;        // 다음 먹이 생성
    [SerializeField] private SnakeMover mover;           // SnakeRoot에 붙은 SnakeMover

    [Header("Base Effects (for now)")]
    public int appleScore = 1;
    public int appleAddSegments = 1;

    public float bananaSpeedMul = 1.5f;         //버프 배수
    public float bananaBoostDuration = 2.0f;    //버프 지속 시간

    [Header("Runtime")]
    public int score;

    private void Awake()
    {
        // 혹시 Inspector 연결을 잊었을 때만 백업
        if (trail == null) trail = FindAnyObjectByType<SnakeTrailFollower>();
        if (spawner == null) spawner = FindAnyObjectByType<FoodSpawner>();
        if (mover == null) mover = FindAnyObjectByType<SnakeMover>();
    }

    /// <summary>
    /// Food_Base(FoodPickup)가 호출하는 공식 먹기 진입점
    /// </summary>
    public void Consume(FoodIdentity identity, Transform foodTransform)
    {
        if (identity == null)
        {
            // 안전장치: 정체성 없으면 기본 처리
            score += appleScore;
            trail?.AddSegment();
            spawner?.Spawn();
            Debug.Log($"Food=Unknown (no identity), Score: {score}");
            return;
        }

        // 지금은 숙성도별 버프는 미구현이므로 FruitType 기준만 적용
        switch (identity.fruit)
        {
            case FruitType.Apple:
                score += appleScore;
                for (int i = 0; i < appleAddSegments; i++)
                    trail?.AddSegment();
                break;

            case FruitType.Banana:
                mover?.ApplySpeedBoost(bananaSpeedMul, bananaBoostDuration);
                break;

            default:
                // 다른 과일 추가되기 전까지는 기본 사과처럼 처리(원하면 제거 가능)
                score += appleScore;
                trail?.AddSegment();
                break;
        }

        // ✅ 먹었으면 항상 다음 먹이 스폰 + 로그
        spawner?.Spawn();
        Debug.Log($"Eat: {identity.fruit}/{identity.ripeness}, Score: {score}");
    }

    /// <summary>
    /// (선택) 기존 방식과의 호환을 위한 백업 트리거.
    /// Food_Base를 쓰면 사실상 필요 없음.
    /// </summary>
    /*
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Food")) return;

        // Food_Base 구조가 아닌 예전 Food 프리팹 대비:
        var identity = other.GetComponent<FoodIdentity>();

        // 예전 음식은 여기서 제거(단, Food_Base의 FoodPickup이 Destroy를 이미 했다면 중복 파괴 주의)
        if (other.gameObject != null)
            Destroy(other.gameObject);

        Consume(identity, other.transform);
    }
    */
}
