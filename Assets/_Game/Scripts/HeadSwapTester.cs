
using UnityEngine;
using UnityEngine.InputSystem;

public class HeadSwapTester : MonoBehaviour
{
    public HeadVisualFitter fitter;
    public GameObject[] headModelPrefabs; // 모델 프리팹들
    private int idx;

    void Start()
    {
        if (fitter == null)
            fitter = GetComponent<HeadVisualFitter>();

        if (headModelPrefabs != null && headModelPrefabs.Length > 0)
        {
            idx = 0;
            fitter.SetModel(headModelPrefabs[idx]);
        }
    }

    // PlayerInput (Behavior: Send Messages)에서 "SwapHead" 액션이 호출하면
    // 자동으로 OnSwapHead가 호출됨 (이름 중요!)
    public void OnSwapHead(InputValue value)
    {
        if (!value.isPressed) return;
        Debug.Log("SwapHead pressed");
        Next();
    }

    public void Next()
    {
        if (fitter == null) return;
        if (headModelPrefabs == null || headModelPrefabs.Length == 0) return;

        idx = (idx + 1) % headModelPrefabs.Length;
        fitter.SetModel(headModelPrefabs[idx]);
    }
}
