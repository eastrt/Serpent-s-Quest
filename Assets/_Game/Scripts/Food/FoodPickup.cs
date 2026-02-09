using UnityEngine;

namespace SerpentsQuest.Food
{
    /// <summary>
    /// 트리거에 들어온 대상(뱀 머리)이 음식을 먹도록 이벤트를 전달.
    /// 지금은 버프 미구현이므로 '먹었다' 이벤트만 쏘고 제거.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class FoodPickup : MonoBehaviour
    {
        [Header("Pickup")]
        [Tooltip("한 번만 먹히도록 잠금")]
        [SerializeField] private bool _consumed;

        [Tooltip("머리가 가진 컴포넌트(예: HeadEatFood)가 이 인터페이스를 구현하면 자동 연동됨")]
        [SerializeField] private bool _destroyOnConsume = true;

        private FoodIdentity _identity;

        private void Awake()
        {
            _identity = GetComponent<FoodIdentity>();

            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_consumed) return;

            // 1) HeadEatFood 같은 구체 클래스에 의존하지 않게 인터페이스로 연결
            if (other.TryGetComponent<IFoodConsumer>(out var consumer))
            {
                _consumed = true;
                consumer.Consume(_identity, transform);

                if (_destroyOnConsume)
                    Destroy(gameObject);
                return;
            }

            // 2) 태그 기반(옵션): 프로젝트에 Head 태그 쓰고 싶으면 여기서 확장
            // if (other.CompareTag("Player")) { ... }
        }
    }

    /// <summary>
    /// 머리(Head)가 구현해야 하는 최소 계약.
    /// (HeadEatFood가 이걸 구현하면 Food_Base는 머리 코드 변경 없이 동작)
    /// </summary>
    public interface IFoodConsumer
    {
        void Consume(FoodIdentity identity, Transform foodTransform);
    }
}
