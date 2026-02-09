using UnityEngine;

namespace SerpentsQuest.Food
{
    /// <summary>
    /// FoodVisual(자식)에 붙이면 예쁘게 부유/회전.
    /// </summary>
    public sealed class FoodBobAndSpin : MonoBehaviour
    {
        [Header("Bob")]
        public float amplitude = 0.15f;
        public float frequency = 1.5f;

        [Header("Spin")]
        public float spinSpeed = 60f;

        private Vector3 _startLocalPos;

        private void Awake()
        {
            _startLocalPos = transform.localPosition;
        }

        private void Update()
        {
            float y = Mathf.Sin(Time.time * frequency) * amplitude;
            transform.localPosition = _startLocalPos + new Vector3(0f, y, 0f);
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.Self);
        }
    }
}
