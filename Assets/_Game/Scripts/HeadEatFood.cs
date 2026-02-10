using UnityEngine;
using SerpentsQuest.Food;
using Unity.VisualScripting; // FoodIdentity, FruitType, Ripeness, IFoodConsumer

public class HeadEatFood : MonoBehaviour, IFoodConsumer
{
    [Header("Refs (drag in Inspector)")]
    [SerializeField] private SnakeTrailFollower trail;   // 몸통관리
    [SerializeField] private SnakeMover mover;           // SnakeRoot에 붙은 SnakeMover

    [Header("Apple Effects")]
    public int appleScore = 1;
    public int appleAddSegments = 1;

    [Header("Banana Effects")]
    public float bananaSpeedMul = 1.5f;         // 버프 배수
    public float bananaBoostDuration = 2.0f;    // 버프 지속 시간

    [Header("Runtime")]
    public int score;

    private void Awake()
    {
        if (trail == null) trail = FindAnyObjectByType<SnakeTrailFollower>();
        if (mover == null) mover = FindAnyObjectByType<SnakeMover>();
    }

    /// <summary>
    /// Food_Base(FoodPickup)가 호출하는 공식 먹기 진입점
    /// </summary>
    public void Consume(FoodIdentity identity, Transform foodTransform)
    {
        // ⚠️ 파괴(Destroy)는 FoodPickup(Food_Base) 쪽에서 담당하는 걸 권장.
        // Consume에서 Destroy까지 해버리면 중복 파괴/중복 스폰 문제가 나기 쉬움.
        // (네가 "destroy consume 체크" 했던 상태면, 여기서는 절대 Destroy 하지 마.)

        if (identity == null)
        {
            score += appleScore;
            trail?.AddSegment();
            Debug.Log($"Eat: Unknown, Score: {score}");
            return;
        }

        switch (identity.fruit)
        {
            case FruitType.Apple:
                score += appleScore;
                for (int i = 0; i < appleAddSegments; i++)
                    trail?.AddSegment();
                break;

            case FruitType.Banana:
                // ✅ SnakeMover가 코루틴으로 버프를 관리함 (먹을 때마다 갱신)
                mover?.ApplySpeedBoost(bananaSpeedMul, bananaBoostDuration);
                break;

            default:
                // 기본 처리(원하면 제거)
                score += appleScore;
                trail?.AddSegment();
                break;
        }

        Debug.Log($"Eat: {identity.fruit}/{identity.ripeness}, Score: {score}");
    }
}
