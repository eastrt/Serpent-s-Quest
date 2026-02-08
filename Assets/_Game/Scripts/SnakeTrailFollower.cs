using System.Collections.Generic;
using UnityEngine;

public class SnakeTrailFollower : MonoBehaviour
{
    public Transform head;                 // 따라갈 대상(HeadSocket or HeadPrefab)
    public Transform bodyContainer;         // 세그먼트 부모
    public GameObject bodySegmentPrefab;

    [Header("Follow")]
    public float pointSpacing = 0.2f;      // 포인트 간격(작을수록 부드럽고 무거움)
    public float segmentSpacing = 0.6f;    // 세그먼트 간격

    private readonly List<Vector3> points = new();
    private readonly List<Transform> segments = new();

    void Start()
    {
        if (head != null)
            points.Add(head.position);
        for (int i = 0; i < 5; i++) AddSegment();
    }

    void Update()
    {
        if (head == null) return;

        // 1) 머리 경로 포인트 쌓기
        if (points.Count == 0 || Vector3.Distance(points[0], head.position) >= pointSpacing)
        {
            points.Insert(0, head.position);
        }

        // 2) 세그먼트별 목표 위치 계산 후 배치
        for (int i = 0; i < segments.Count; i++)
        {
            float dist = (i + 1) * segmentSpacing;
            segments[i].position = GetPointAtDistance(dist);
        }

        // 3) 필요 이상 오래된 포인트 정리(성능)
        float maxNeeded = (segments.Count + 2) * segmentSpacing;
        TrimPoints(maxNeeded + 2f);
    }

    Vector3 GetPointAtDistance(float distance)
    {
        if (points.Count == 1) return points[0];

        float d = 0f;
        for (int i = 0; i < points.Count - 1; i++)
        {
            float seg = Vector3.Distance(points[i], points[i + 1]);
            if (d + seg >= distance)
            {
                float t = (distance - d) / seg;
                return Vector3.Lerp(points[i], points[i + 1], t);
            }
            d += seg;
        }
        return points[points.Count - 1];
    }

    void TrimPoints(float keepDistance)
    {
        float d = 0f;
        for (int i = 0; i < points.Count - 1; i++)
        {
            d += Vector3.Distance(points[i], points[i + 1]);
            if (d > keepDistance)
            {
                points.RemoveRange(i + 1, points.Count - (i + 1));
                break;
            }
        }
    }

    public void AddSegment()
    {
        if (bodySegmentPrefab == null || bodyContainer == null) return;

        Vector3 spawnPos = (segments.Count == 0) ? GetPointAtDistance(segmentSpacing) : segments[^1].position;
        var go = Instantiate(bodySegmentPrefab, spawnPos, Quaternion.identity, bodyContainer);
        segments.Add(go.transform);
    }

    public void RemoveLastSegment()
    {
        if (segments.Count == 0) return;
        var last = segments[^1];
        segments.RemoveAt(segments.Count - 1);
        Destroy(last.gameObject);
    }
}
