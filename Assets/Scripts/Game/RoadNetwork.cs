using System.Collections.Generic;
using UnityEngine;

public class RoadNetwork : MonoBehaviour
{
    public static RoadNetwork Instance { get; private set; }

    [SerializeField] float[] columnsX = { -120f, -60f, 0f, 60f, 120f };
    [SerializeField] float[] rowsZ = { -90f, -30f, 30f, 90f };
    [SerializeField] float laneOffset = 3.35f;

    Vector3[] nodes;
    readonly List<(int a, int b)> segments = new List<(int a, int b)>();
    float[,] distance;
    int[,] nextHop;

    void Awake()
    {
        Instance = this;
        BuildGraph();
    }

    public List<Vector3> Route(Vector3 from, Vector3 to)
    {
        var (fromSegment, fromPoint) = NearestOnRoad(from);
        var (toSegment, toPoint) = NearestOnRoad(to);

        var center = new List<Vector3> { fromPoint };
        if (fromSegment != toSegment)
        {
            int[] fromEnds = { segments[fromSegment].a, segments[fromSegment].b };
            int[] toEnds = { segments[toSegment].a, segments[toSegment].b };
            int bestFrom = fromEnds[0], bestTo = toEnds[0];
            float best = float.MaxValue;
            foreach (int s in fromEnds)
                foreach (int t in toEnds)
                {
                    float d = Vector3.Distance(fromPoint, nodes[s]) + distance[s, t] + Vector3.Distance(nodes[t], toPoint);
                    if (d < best) { best = d; bestFrom = s; bestTo = t; }
                }
            for (int node = bestFrom; ; node = nextHop[node, bestTo])
            {
                center.Add(nodes[node]);
                if (node == bestTo) break;
            }
        }
        center.Add(toPoint);
        return KeepRight(RemoveDuplicates(center));
    }

    public float Distance(Vector3 from, Vector3 to)
    {
        var route = Route(from, to);
        float total = 0f;
        for (int i = 1; i < route.Count; i++) total += Vector3.Distance(route[i - 1], route[i]);
        return total;
    }

    void BuildGraph()
    {
        int cols = columnsX.Length, rows = rowsZ.Length, n = cols * rows;
        nodes = new Vector3[n];
        for (int c = 0; c < cols; c++)
            for (int r = 0; r < rows; r++)
            {
                nodes[c * rows + r] = new Vector3(columnsX[c], 0f, rowsZ[r]);
                if (c + 1 < cols) segments.Add((c * rows + r, (c + 1) * rows + r));
                if (r + 1 < rows) segments.Add((c * rows + r, c * rows + r + 1));
            }

        distance = new float[n, n];
        nextHop = new int[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                distance[i, j] = i == j ? 0f : float.PositiveInfinity;
                nextHop[i, j] = i == j ? i : -1;
            }
        foreach (var (a, b) in segments)
        {
            distance[a, b] = distance[b, a] = Vector3.Distance(nodes[a], nodes[b]);
            nextHop[a, b] = b;
            nextHop[b, a] = a;
        }
        for (int k = 0; k < n; k++)
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    if (distance[i, k] + distance[k, j] < distance[i, j])
                    {
                        distance[i, j] = distance[i, k] + distance[k, j];
                        nextHop[i, j] = nextHop[i, k];
                    }
    }

    (int segment, Vector3 point) NearestOnRoad(Vector3 p)
    {
        p.y = 0f;
        int bestSegment = 0;
        Vector3 bestPoint = nodes[0];
        float bestDistance = float.MaxValue;
        for (int i = 0; i < segments.Count; i++)
        {
            Vector3 a = nodes[segments[i].a], ab = nodes[segments[i].b] - a;
            Vector3 q = a + ab * Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude);
            float d = (p - q).sqrMagnitude;
            if (d < bestDistance) { bestDistance = d; bestSegment = i; bestPoint = q; }
        }
        return (bestSegment, bestPoint);
    }

    static List<Vector3> RemoveDuplicates(List<Vector3> points)
    {
        var result = new List<Vector3>();
        foreach (var p in points)
            if (result.Count == 0 || (result[result.Count - 1] - p).sqrMagnitude > 0.01f) result.Add(p);
        return result;
    }

    List<Vector3> KeepRight(List<Vector3> points)
    {
        var result = new List<Vector3>(points.Count);
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 dirIn = i > 0 ? (points[i] - points[i - 1]).normalized : Vector3.zero;
            Vector3 dirOut = i < points.Count - 1 ? (points[i + 1] - points[i]).normalized : Vector3.zero;
            if (dirIn == Vector3.zero) dirIn = dirOut;
            if (dirOut == Vector3.zero) dirOut = dirIn;
            Vector3 rightIn = new Vector3(dirIn.z, 0f, -dirIn.x), rightOut = new Vector3(dirOut.z, 0f, -dirOut.x);
            float denominator = 1f + Vector3.Dot(rightIn, rightOut);
            Vector3 offset = denominator > 0.1f ? (rightIn + rightOut) / denominator : rightIn;
            result.Add(points[i] + offset * laneOffset);
        }
        return result;
    }

    void OnDrawGizmosSelected()
    {
        if (nodes == null) return;
        Gizmos.color = Color.cyan;
        foreach (var (a, b) in segments) Gizmos.DrawLine(nodes[a] + Vector3.up, nodes[b] + Vector3.up);
    }
}
