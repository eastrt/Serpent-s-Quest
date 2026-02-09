using UnityEngine;

public enum FoodKind
{
    Apple,
    Banana,
    Unknown
}

public class FoodType : MonoBehaviour
{
    public FoodKind kind = FoodKind.Unknown;

    [Tooltip("비활성화하면 Inspector에서 직접 kind를 지정")]
    public bool autoDetectFromName = true;

    void Awake()
    {
        if (autoDetectFromName)
            Detect();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (autoDetectFromName)
            Detect();
    }
#endif

    void Detect()
    {
        string n = gameObject.name.ToLowerInvariant();

        // (Clone) 붙어도 상관없게 문자열 포함 체크
        if (n.Contains("apple")) kind = FoodKind.Apple;
        else if (n.Contains("banana")) kind = FoodKind.Banana;
        else kind = FoodKind.Unknown;
    }
}
