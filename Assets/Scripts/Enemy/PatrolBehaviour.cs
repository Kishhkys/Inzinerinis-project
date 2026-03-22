using System.Collections.Generic;
using UnityEngine;

public class PatrolBehaviour : SteeringBehaviour
{
    [SerializeField] private float patrolPointReachedThreshold = 0.5f;
    [SerializeField] private PathFinder pathFinder;

    private List<PathNode> currentPath;
    private int currentPathIndex;

    public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData aiData)
    {
        if (aiData == null) return (danger, interest);
        if (aiData.patrolPoints == null || aiData.patrolPoints.Count == 0)
        {
            aiData.currentPatrolTarget = null;
            return (danger, interest);
        }

        if (aiData.currentPatrolIndex < 0 || aiData.currentPatrolIndex >= aiData.patrolPoints.Count)
            aiData.currentPatrolIndex = 0;

        Transform patrolTarget = aiData.patrolPoints[aiData.currentPatrolIndex];
        if (patrolTarget == null)
        {
            aiData.currentPatrolTarget = null;
            return (danger, interest);
        }

      
        if (currentPath == null || currentPath.Count == 0)
        {
            currentPath = pathFinder.FindPath(transform.position, patrolTarget.position);
            currentPathIndex = 0;
        }

      
        float distToFinal = Vector2.Distance(transform.position, patrolTarget.position);
        if (distToFinal <= patrolPointReachedThreshold)
        {
            aiData.currentPatrolIndex = (aiData.currentPatrolIndex + 1) % aiData.patrolPoints.Count;
            currentPath = null;
            return (danger, interest);
        }

  
        if (currentPath != null && currentPathIndex < currentPath.Count)
        {
            Vector2 nextNode = currentPath[currentPathIndex].transform.position;
            float distToNode = Vector2.Distance(transform.position, nextNode);

            if (distToNode <= patrolPointReachedThreshold)
            {
                currentPathIndex++;
                if (currentPathIndex >= currentPath.Count)
                {
                    currentPath = null;
                    return (danger, interest);
                }
                nextNode = currentPath[currentPathIndex].transform.position;
            }

            aiData.currentPatrolTarget = currentPath[currentPathIndex].transform;

            Vector2 directionToTarget = (nextNode - (Vector2)transform.position).normalized;

            for (int i = 0; i < Directions.eightDirections.Count; i++)
            {
                float result = Vector2.Dot(directionToTarget, Directions.eightDirections[i]);
                if (result > 0 && result > interest[i])
                    interest[i] = result;
            }
        }

        return (danger, interest);
    }

}
