
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;
    public Vector3 center;
    public Vector3 size = new Vector3(20, 0, 20);

    [Header("Spawn Check")]
    public LayerMask blockMask;       // 벽/장애물 레이어 넣기
    public float checkRadius = 0.4f;  // 음식 반지름 정도
    public float groundY = 0f;        // 바닥 높이

    GameObject currentFood;

    void Start()
    {
        Spawn();
    }

    public void Spawn()
    {
        if (foodPrefab == null) return;

        // 기존 음식 제거
        if (currentFood != null) Destroy(currentFood);

        for (int i = 0; i < 50; i++) // 최대 50번 시도
        {
            float x = Random.Range(center.x - size.x * 0.5f, center.x + size.x * 0.5f);
            float z = Random.Range(center.z - size.z * 0.5f, center.z + size.z * 0.5f);

            Vector3 pos = new Vector3(x, groundY, z);

            // 장애물/벽 위 피하기
            bool blocked = Physics.CheckSphere(pos, checkRadius, blockMask);
            if (blocked) continue;

            currentFood = Instantiate(foodPrefab, pos, Quaternion.identity);
            currentFood.name = "Food";
            return;
        }

        Debug.LogWarning("FoodSpawner: Failed to find a spawn position.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(center + new Vector3(0, groundY, 0), new Vector3(size.x, 0.1f, size.z));
    }
}
