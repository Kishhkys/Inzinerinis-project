using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    public List<PathNode> neighbours = new List<PathNode>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.15f);

        Gizmos.color = Color.white;
        foreach (var neighbour in neighbours)
        {
            if (neighbour != null)
            {
                Gizmos.DrawLine(transform.position, neighbour.transform.position);
            }
        }
    }
}
