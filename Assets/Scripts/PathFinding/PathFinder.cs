using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    [SerializeField] private List<PathNode> allNodes = new();

    [Header("Line of Sight")]
    [SerializeField] private LayerMask obstacleMask;

    [SerializeField] private bool requireLineOfSightToStartNode = true;

    public List<PathNode> FindPath(Vector2 startPos, Vector2 targetPos)
    {
        PathNode startNode = requireLineOfSightToStartNode
            ? GetClosestReachableNode(startPos)
            : GetClosestNode(startPos);

        PathNode targetNode = GetClosestNode(targetPos);

        if (startNode == null || targetNode == null)
            return null;

        List<PathNode> openSet = new() { startNode };
        HashSet<PathNode> closedSet = new();

        Dictionary<PathNode, PathNode> cameFrom = new();
        Dictionary<PathNode, float> gScore = new();
        Dictionary<PathNode, float> fScore = new();

        foreach (var node in allNodes)
        {
            gScore[node] = float.PositiveInfinity;
            fScore[node] = float.PositiveInfinity;
        }

        gScore[startNode] = 0f;
        fScore[startNode] = Heuristic(startNode, targetNode);

        while (openSet.Count > 0)
        {
            PathNode current = openSet.OrderBy(n => fScore[n]).First();

            if (current == targetNode)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbour in current.neighbours)
            {
                if (neighbour == null || closedSet.Contains(neighbour))
                    continue;

                float tentativeG = gScore[current] + Vector2.Distance(current.transform.position, neighbour.transform.position);

                if (!openSet.Contains(neighbour))
                    openSet.Add(neighbour);
                else if (tentativeG >= gScore[neighbour])
                    continue;

                cameFrom[neighbour] = current;
                gScore[neighbour] = tentativeG;
                fScore[neighbour] = tentativeG + Heuristic(neighbour, targetNode);
            }
        }

        return null;
    }

    private float Heuristic(PathNode a, PathNode b)
    {
        return Vector2.Distance(a.transform.position, b.transform.position);
    }

    private List<PathNode> ReconstructPath(Dictionary<PathNode, PathNode> cameFrom, PathNode current)
    {
        List<PathNode> path = new() { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }

    private PathNode GetClosestNode(Vector2 position)
    {
        PathNode bestNode = null;
        float bestDistance = float.PositiveInfinity;

        foreach (var node in allNodes)
        {
            if (node == null) continue;

            float dist = Vector2.Distance(position, node.transform.position);
            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestNode = node;
            }
        }

        return bestNode;
    }


    private PathNode GetClosestReachableNode(Vector2 position)
    {
        var sortedNodes = allNodes
            .Where(n => n != null)
            .OrderBy(n => Vector2.Distance(position, n.transform.position))
            .ToList();

        foreach (var node in sortedNodes)
        {
            if (HasLineOfSight(position, node.transform.position))
            {
                return node;
            }
        }

        return sortedNodes.FirstOrDefault();
    }


    private const float MinRaycastDistance = 0.01f;

    private bool HasLineOfSight(Vector2 from, Vector2 to)
    {
        Vector2 direction = to - from;
        float distance = direction.magnitude;

        if (distance < MinRaycastDistance) return true;

        RaycastHit2D hit = Physics2D.Raycast(from, direction.normalized, distance, obstacleMask);
        return hit.collider == null;
    }
}
