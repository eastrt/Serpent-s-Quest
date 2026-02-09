using UnityEngine;

public class DebugLocalRot : MonoBehaviour
{
    void LateUpdate()
    {
        //Debug.Log($"[HeadPrefab F1] localY={transform.localEulerAngles.y:F1} worldY={transform.eulerAngles.y:F1}");
        //Debug.Log($"[HeadPrefab F2] localY={transform.localEulerAngles.y:F2} worldY={transform.eulerAngles.y:F2}");
        // 부모(HeadSocket)의 회전만 따르게 하고, 자신은 로컬 회전 고정
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}
