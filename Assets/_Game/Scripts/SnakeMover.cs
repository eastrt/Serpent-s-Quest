using UnityEngine;

public class SnakeMover : MonoBehaviour
{
    public SnakeInputHandler input;
    public Transform headRoot;       // HeadSocket 아래 HeadPrefab(또는 HeadSocket) 지정

    [Header("Move")]
    public float moveSpeed = 6f;
    public float boostMul = 1.6f;
    public float turnSpeed = 240f;   // degrees/sec

    [Header("Turn Limit")]
    [Range(0f, 180f)]
    public float maxTurnAngle = 115f;   // 진행방향 기준 최대 회전 허용 각도
    public bool allowReverse = false;   // 후진 허용 여부 (기본 false)
    // add field
    [Header("Buff")]
    public float buffSpeedMul = 1f;
    Coroutine buffCo;

    // 진행방향(수평) 캐시: "뒤로 못가게" 판단 기준
    Vector3 _moveForward = Vector3.forward;

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
        if (!allowReverse) forward = Mathf.Max(0f, forward); // ✅ 후진 차단

        // A/D: 좌우 회전(조향)
        float turn = Mathf.Clamp(input.move.x, -1f, 1f);

        // 회전 먼저
        //headRoot.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime);
        //headRoot.Rotate(0f, turn * turnSpeed * Time.deltaTime, 0f, Space.World);
        // 전진/후진
        //Vector3 dir = headRoot.forward;
        //dir.y = 0f;
        //headRoot.position += dir.normalized * (spd * forward) * Time.deltaTime;
        // ====== 1) 입력으로 만든 "원래 목표 방향" ======
        float yawDelta = turn * turnSpeed * Time.deltaTime;
        Vector3 desiredDir = Quaternion.Euler(0f, yawDelta, 0f) * headRoot.forward;
        desiredDir.y = 0f;

        // 안정성
        if (desiredDir.sqrMagnitude < 0.0001f)
            desiredDir = headRoot.forward;

        desiredDir.Normalize();

        // ====== 2) 현재 진행방향 기준으로 135도 제한 ======
        // (뒤쪽으로 꺾이는 방향이면 경계선에서 멈추도록 보정)
        float angle = Vector3.Angle(_moveForward, desiredDir);
        if (angle > maxTurnAngle)
        {
            desiredDir = Vector3.RotateTowards(
                _moveForward,
                desiredDir,
                Mathf.Deg2Rad * maxTurnAngle,
                0f
            );
            desiredDir.y = 0f;
            desiredDir.Normalize();
        }

        // ====== 3) 보정된 목표 방향으로 회전 적용 ======
        if (desiredDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(desiredDir, Vector3.up);
            headRoot.rotation = Quaternion.RotateTowards(
                headRoot.rotation,
                targetRot,
                turnSpeed * Time.deltaTime
            );
        }

        // ====== 4) 전진 이동 ======
        if (forward > 0f)
        {
            Vector3 dir = headRoot.forward;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f) dir.Normalize();

            headRoot.position += dir * (spd * forward) * Time.deltaTime;

            // 이동 중일 때만 진행방향 갱신 (정지 중엔 기준이 흔들리지 않게)
            _moveForward = dir;
        }
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
