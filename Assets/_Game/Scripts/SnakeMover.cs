using UnityEngine;

public class SnakeMover : MonoBehaviour
{
    public SnakeInputHandler input;
    public Transform headRoot;       // HeadSocket 아래 HeadPrefab(또는 HeadSocket) 지정

    [Header("Move")]
    public float moveSpeed = 6f;
    public float boostMul = 1.6f;
    public float turnSpeed = 240f;   // degrees/sec

    // add field
    [Header("Buff")]
    public float buffSpeedMul = 1f;
    Coroutine buffCo;
    void Reset()
    {
        input = GetComponent<SnakeInputHandler>();
    }

    void Update()
    {
        if (input == null || headRoot == null) return;

        // buff Speed Mul 적용
        float spd = moveSpeed * (input.boostHeld ? boostMul : 1f) * (buffSpeedMul);

        // W/S: 전/후 (선택: 후진 허용 여부)
        float forward = Mathf.Clamp(input.move.y, -1f, 1f);

        // A/D: 좌우 회전(조향)
        float turn = Mathf.Clamp(input.move.x, -1f, 1f);

        // 회전 먼저
        //headRoot.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime);
        headRoot.Rotate(0f, turn * turnSpeed * Time.deltaTime, 0f, Space.World);
        // 전진/후진
        Vector3 dir = headRoot.forward;
        dir.y = 0f;
        headRoot.position += dir.normalized * (spd * forward) * Time.deltaTime;
    }

    public void ApplySpeedBoost(float mul, float duration)
    {
        if (buffCo != null) StopCoroutine(buffCo);
        buffCo = StartCoroutine(CoSpeedBoost(mul, duration));
    }

    System.Collections.IEnumerator CoSpeedBoost(float mul, float duration)
    {
        buffSpeedMul = mul;
        yield return new WaitForSeconds(duration);
        buffSpeedMul = 1f;
        buffCo = null;
    }

}
