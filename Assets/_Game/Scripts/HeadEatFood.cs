using UnityEngine;

public class HeadEatFood : MonoBehaviour
{
    //public SnakeTrailFollower trail;   // 몸통 관리
    //public FoodSpawner spawner;        // 다음 먹이 생성
    [SerializeField] SnakeTrailFollower trail;
    [SerializeField] FoodSpawner spawner;
    public int score;



    void Awake()
    {
        // 혹시 Inspector 연결을 잊었을 때만 백업
        if (trail == null)
            trail = FindAnyObjectByType<SnakeTrailFollower>();

        if (spawner == null)
            spawner = FindAnyObjectByType<FoodSpawner>();
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

        // 먹기
        Destroy(other.gameObject);

        score += 1;
        if (trail != null) trail.AddSegment();
        if (spawner != null) spawner.Spawn();

        Debug.Log($"Score: {score}");
    }
}
