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

    //    private readonly List<Vector3> points = new();
    //  [수정 1] 위치만 저장하지 말고 pos+rot 같이 저장
    private struct TrailPoint
    {
        public Vector3 pos;
        public Quaternion rot;
        public TrailPoint(Vector3 p, Quaternion r) { pos = p; rot = r; }
    }
    private readonly List<Transform> segments = new();
    //  [수정 2] List<Vector3> -> List<TrailPoint>
    private readonly List<TrailPoint> points = new();

    void Start()
    {
        if (head != null)
            //points.Add(head.position);
            points.Add(new TrailPoint(head.position, head.rotation));
        for (int i = 0; i < 3; i++) AddSegment();
    }

    void Update()
    {
        if (head == null) return;

        // 1) 머리 경로 포인트 쌓기
        if (points.Count == 0 || Vector3.Distance(points[0].pos, head.position) >= pointSpacing)
        {
            // points.Insert(0, head.position);
            points.Insert(0, new TrailPoint(head.position, head.rotation));
        }

        // 2) 세그먼트별 목표 위치 계산 후 배치
        for (int i = 0; i < segments.Count; i++)
        {
            float dist = (i + 1) * segmentSpacing;
            // segments[i].position = GetPointAtDistance(dist);
            TrailPoint tp = GetTrailPointAtDistance(dist); // ✅ [수정 6] 반환 타입 변경

            segments[i].position = tp.pos;
            segments[i].rotation = tp.rot;                 // ✅ [핵심] 회전도 따라가게!
        }

        // 3) 필요 이상 오래된 포인트 정리(성능)
        float maxNeeded = (segments.Count + 2) * segmentSpacing;
        TrimPoints(maxNeeded + 2f);
    }

    /*
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
    */
    // ✅ [수정 7] Vector3 -> TrailPoint 반환
    TrailPoint GetTrailPointAtDistance(float distance)
    {
        if (points.Count == 1) return points[0];

        float d = 0f;
        for (int i = 0; i < points.Count - 1; i++)
        {
            float seg = Vector3.Distance(points[i].pos, points[i + 1].pos);
            if (d + seg >= distance)
            {
                float t = (distance - d) / seg;

                // 위치 보간
                Vector3 pos = Vector3.Lerp(points[i].pos, points[i + 1].pos, t);

                // ✅ 회전 보간 (곡선에서 기차처럼 자연스러움)
                Quaternion rot = Quaternion.Slerp(points[i].rot, points[i + 1].rot, t);

                return new TrailPoint(pos, rot);
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
            d += Vector3.Distance(points[i].pos, points[i + 1].pos);
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

        Vector3 spawnPos = (segments.Count == 0) ? GetTrailPointAtDistance(segmentSpacing).pos : segments[^1].position;
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
