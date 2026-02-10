using UnityEngine;
using UnityEngine.InputSystem;

public class SnakeInputHandler : MonoBehaviour
{
    [Header("Read-only")]
    public Vector2 move;      // WASD/Stick
    public Vector2 look;      // Mouse delta / Left stick
    //public Vector2 turn;      // Mouse delta / Right stick
    public bool boostHeld;

    // PlayerInput(Send Messages)가 아래 함수들을 자동 호출함 (이름 중요)
    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();

        // 입력 안정화 (스틱 대각선 과속 방지)
        move = Vector2.ClampMagnitude(v, 1f);
    }

    // public void OnTurn(InputValue value)
    //{
    //    turn = value.Get<Vector2>();
    // }

    public void OnBoost(InputValue value)
    {
        boostHeld = value.isPressed;

    }

    public void OnSwapHead(InputValue value)
    {
        if (!value.isPressed) return;
        // TODO: 여기서 머리 교체 호출
        // 예: GetComponentInChildren<HeadSwapTester>()?.Next();

    }
}
