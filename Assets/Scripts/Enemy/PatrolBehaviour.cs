using UnityEngine;

public class PatrolBehaviour : SteeringBehaviour
{
    [SerializeField] private float patrolPointReachedThreshold = 0.5f;

    public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData aiData)
    {
        if (aiData == null)
            return (danger, interest);

        if (aiData.patrolPoints == null || aiData.patrolPoints.Count == 0)
        {
            aiData.currentPatrolTarget = null;
            return (danger, interest);
        }

        if (aiData.currentPatrolIndex < 0 || aiData.currentPatrolIndex >= aiData.patrolPoints.Count)
        {
            aiData.currentPatrolIndex = 0;
        }

        Transform patrolTarget = aiData.patrolPoints[aiData.currentPatrolIndex];

        if (patrolTarget == null)
        {
            aiData.currentPatrolTarget = null;
            return (danger, interest);
        }

        float distance = Vector2.Distance(transform.position, patrolTarget.position);

        if (distance <= patrolPointReachedThreshold)
        {
            aiData.currentPatrolIndex = (aiData.currentPatrolIndex + 1) % aiData.patrolPoints.Count;

            patrolTarget = aiData.patrolPoints[aiData.currentPatrolIndex];

            if (patrolTarget == null)
            {
                aiData.currentPatrolTarget = null;
                return (danger, interest);
            }
        }

        aiData.currentPatrolTarget = patrolTarget;

        Vector2 directionToTarget =
            ((Vector2)patrolTarget.position - (Vector2)transform.position).normalized;

        for (int i = 0; i < Directions.eightDirections.Count; i++)
        {
            float result = Vector2.Dot(directionToTarget, Directions.eightDirections[i]);

            if (result > 0 && result > interest[i])
            {
                interest[i] = result;
            }
        }

        return (danger, interest);
    }

}
