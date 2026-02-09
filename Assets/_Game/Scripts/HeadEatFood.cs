using UnityEngine;

public class HeadEatFood : MonoBehaviour
{

    [Header("Refs (drag in Inspector)")]
    [SerializeField] private SnakeTrailFollower trail;   // 몸통관리
    [SerializeField] private FoodSpawner spawner;        // 다음 먹이 생성
    [SerializeField] private SnakeMover mover;           // SnakeRoot에 붙은 SnakeMover

    [Header("Effects Table")]
    public int appleScore = 1;
    public int appleAddSegments = 1;

    public float bananaSpeedMul = 1.5f;         //버프 배수
    public float bananaBoostDuration = 2.0f;    //버프 지속 시간

    public int score;



    void Awake()
    {
        // 혹시 Inspector 연결을 잊었을 때만 백업
        if (trail == null)
            trail = FindAnyObjectByType<SnakeTrailFollower>();

        if (spawner == null)
            spawner = FindAnyObjectByType<FoodSpawner>();

        if (mover == null) mover = FindAnyObjectByType<SnakeMover>();
    }
    //void Reset()
    //{
    // FindAnyObjectByType<T>() : “아무거나 하나” (더 빠름, 순서 보장 없음) 대체가능
    //  trail = FindFirstObjectByType<SnakeTrailFollower>();
    // spawner = FindFirstObjectByType<FoodSpawner>();

    // }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Food")) return;

        //FoodType 가져오기

        FoodKind kind = FoodKind.Unknown;
        var ft = other.GetComponent<FoodType>();
        if (ft != null) kind = ft.kind;
        else
        {
            string n = other.name.ToLowerInvariant();
            if (n.Contains("apple")) kind = FoodKind.Apple;
            else if (n.Contains("banana")) kind = FoodKind.Banana;
        }

        // 먹기
        Destroy(other.gameObject);

        //score += 1;
        //if (trail != null) trail.AddSegment();
        switch (kind)
        {
            case FoodKind.Apple:
                score += appleScore;
                for (int i = 0; i < appleAddSegments; i++)
                    trail?.AddSegment();
                break;

            case FoodKind.Banana:
                // 바나나 = 속도/부스트
                mover?.ApplySpeedBoost(bananaSpeedMul, bananaBoostDuration);
                break;

            default:
                // 알 수 없으면 기본 사과처럼 처리(원하면 제거 가능)
                score += appleScore;
                trail?.AddSegment();
                break;

                spawner?.Spawn();

                Debug.Log($"Food={kind}, Score: {score}");
        }
    }
}
