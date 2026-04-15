using System.Collections.Generic;
using UnityEngine;

public class AIData : MonoBehaviour
{
    public List<Transform> targets = null;
    public Collider2D[] obstacles = null;

    public Transform currentTarget;
    public Transform currentPatrolTarget;

    [Header("Patrol")]
    public List<Transform> patrolPoints = new List<Transform>();
    public int currentPatrolIndex = 0;

    [Header("Investigation")]
    public Vector2 lastSeenPosition;
    public bool hasLastSeenPosition = false;

    [Header("Pathfinding")]
    public List<PathNode> currentPath = new List<PathNode>();
    public int currentPathIndex = 0;


    public int GetTargetsCount() => targets == null ? 0 : targets.Count;

}
