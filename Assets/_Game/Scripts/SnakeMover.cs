using UnityEngine;

public class SnakeMover : MonoBehaviour
{
    [Header("Refs")]
    public SnakeInputHandler input;
    public Transform headRoot; // HeadSocket 아래 HeadPrefab(또는 HeadSocket) 지정

    [Header("Move")]
    public float moveSpeed = 6f;
    public float boostMul = 1.6f;           // 손으로 누르는 부스트 배수
    public float turnSpeed = 240f;          // degrees/sec

    [Header("Turn Limit")]
    [Range(0f, 180f)]
    public float maxTurnAngle = 75f;        // 진행방향 기준 최대 회전 허용 각도
    public bool allowReverse = false;       // 후진 허용 여부

    [Header("Buff (from foods)")]
    [SerializeField] private float buffSpeedMul = 1f;   // 현재 버프 배수(기본 1)
    private float buffExpireAt = -1f;                   // Time.time 기준 만료 시각

    // 진행방향(수평) 캐시: "뒤로 못가게/회전제한" 판단 기준
    private Vector3 moveForward = Vector3.forward;

    private void Reset()
    {
        input = GetComponent<SnakeInputHandler>();
    }

    private void Awake()
    {
        if (input == null) input = GetComponent<SnakeInputHandler>();
        if (headRoot == null) headRoot = transform;
    }

    private void Update()
    {
        if (input == null || headRoot == null) return;

        // 1) 버프 만료 처리
        UpdateBuff();

        // 2) 입력
        float forward = Mathf.Clamp(input.move.y, -1f, 1f);
        if (!allowReverse) forward = Mathf.Max(0f, forward);

        float turn = Mathf.Clamp(input.move.x, -1f, 1f);

        // 3) 현재 속도 배수 계산: base * (held boost) * (food buff)
        float heldMul = input.boostHeld ? boostMul : 1f;
        float spd = moveSpeed * heldMul * buffSpeedMul;

        // 4) 이번 프레임에 원하는 방향(desired)을 구성
        //    (현재 headRoot.forward에 입력 회전량을 가상 적용한 방향)
        float yawDelta = turn * turnSpeed * Time.deltaTime;
        Vector3 desiredDir = Quaternion.Euler(0f, yawDelta, 0f) * headRoot.forward;
        desiredDir.y = 0f;

        if (desiredDir.sqrMagnitude < 0.0001f)
            desiredDir = headRoot.forward;

        desiredDir.Normalize();

        // 5) 진행방향 기준 회전 제한
        float angle = Vector3.Angle(moveForward, desiredDir);
        if (angle > maxTurnAngle)
        {
            desiredDir = Vector3.RotateTowards(
                moveForward,
                desiredDir,
                Mathf.Deg2Rad * maxTurnAngle,
                0f
            );
            desiredDir.y = 0f;
            desiredDir.Normalize();
        }

        // 6) 방향으로 회전 (부드럽게)
        Quaternion targetRot = Quaternion.LookRotation(desiredDir, Vector3.up);
        headRoot.rotation = Quaternion.RotateTowards(
            headRoot.rotation,
            targetRot,
            turnSpeed * Time.deltaTime
        );

        // 7) 이동
        if (forward > 0f)
        {
            Vector3 dir = headRoot.forward;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.0001f)
                dir.Normalize();
            else
                dir = moveForward;

            headRoot.position += dir * (spd * forward) * Time.deltaTime;

            // 이동 중일 때만 진행방향 갱신
            moveForward = dir;
        }
    }

    private void UpdateBuff()
    {
        if (buffExpireAt > 0f && Time.time >= buffExpireAt)
        {
            buffExpireAt = -1f;
            buffSpeedMul = 1f;
        }
    }

    /// <summary>
    /// 시간제 속도 버프 적용.
    /// - 배수는 "더 큰 값" 우선(스택 폭주 방지)
    /// - 시간은 "연장" (기존 만료시각 vs now+duration 중 큰 값)
    /// </summary>
    public void ApplySpeedBoost(float mul, float duration)
    {
        if (mul <= 0f) mul = 1f;
        if (duration <= 0f) duration = 0.01f;

        buffSpeedMul = Mathf.Max(buffSpeedMul, mul);
        float newExpire = Time.time + duration;
        buffExpireAt = Mathf.Max(buffExpireAt, newExpire);
    }

    /// <summary>디버그/ UI용: 현재 버프 남은 시간(초)</summary>
    public float GetBuffRemaining()
    {
        if (buffExpireAt < 0f) return 0f;
        return Mathf.Max(0f, buffExpireAt - Time.time);
    }

    /// <summary>디버그/ UI용: 현재 버프 배수</summary>
    public float GetBuffMul() => buffSpeedMul;
}
