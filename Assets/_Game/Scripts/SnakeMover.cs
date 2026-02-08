using UnityEngine;

public class SnakeMover : MonoBehaviour
{
    public SnakeInputHandler input;
    public Transform headRoot;       // HeadSocket 아래 HeadPrefab(또는 HeadSocket) 지정

    [Header("Move")]
    public float moveSpeed = 6f;
    public float boostMul = 1.6f;
    public float turnSpeed = 240f;   // degrees/sec

    void Reset()
    {
        input = GetComponent<SnakeInputHandler>();
    }

    void Update()
    {
        if (input == null || headRoot == null) return;

        float spd = moveSpeed * (input.boostHeld ? boostMul : 1f);

        // WASD를 "머리의 방향 기준"으로 이동
        Vector3 local = new Vector3(input.move.x, 0f, input.move.y);
        Vector3 world = headRoot.TransformDirection(local);
        world.y = 0f;

        if (world.sqrMagnitude > 0.0001f)
        {
            headRoot.position += world.normalized * spd * Time.deltaTime;

            // 이동 방향으로 부드럽게 회전(look 없이도 FPS 느낌)
            Quaternion target = Quaternion.LookRotation(world.normalized, Vector3.up);
            headRoot.rotation = Quaternion.RotateTowards(headRoot.rotation, target, turnSpeed * Time.deltaTime);
        }
    }
}
