using UnityEngine;

namespace SerpentsQuest.Food
{
    public enum FruitType
    {
        Apple,
        Banana,
        DummyBana,
        // 앞으로 추가: Orange, Grape ...
    }

    public enum Ripeness
    {
        Unripe,     // 덜익은
        Ripe,       // 익은
        Overripe,   // 잘익은
        Rotten      // 썩은
    }

    /// <summary>
    /// "이 음식이 무엇인지"에 대한 정체성만 담당.
    /// 로직 분기는 절대 Prefab 이름으로 하지 말고 이 값으로.
    /// </summary>
    public sealed class FoodIdentity : MonoBehaviour
    {
        [Header("Identity")]
        public FruitType fruit = FruitType.Apple;
        public Ripeness ripeness = Ripeness.Ripe;

        [Header("Optional")]
        [Tooltip("나중에 ScriptableObject로 버프/효과 프로필 연결할 때 사용")]
        public ScriptableObject effectProfile;
    }
}



