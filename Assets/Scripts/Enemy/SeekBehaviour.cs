using System.Linq;
using UnityEngine;

public class SeekBehaviour : SteeringBehaviour
{
    [SerializeField]
    private float targetRechedThreshold = 0.5f;

    [SerializeField]
    private bool showGizmo = true;

    bool reachedLastTarget = true;

    private Vector2 targetPositionCached;
    private float[] interestsTemp;

    public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData aiData)
    {
        if (aiData.targets == null || aiData.targets.Count <= 0)
        {
            aiData.currentTarget = null;
            reachedLastTarget = true;
            return (danger, interest);
        }

        if (reachedLastTarget)
        {
            reachedLastTarget = false;
            aiData.currentTarget = aiData.targets
                .OrderBy(target => Vector2.Distance(target.position, transform.position))
                .FirstOrDefault();
        }

        if (aiData.currentTarget != null && aiData.targets.Contains(aiData.currentTarget))
        {
            targetPositionCached = aiData.currentTarget.position;
        }

        if (Vector2.Distance(transform.position, targetPositionCached) < targetRechedThreshold)
        {
            reachedLastTarget = true;
            return (danger, interest);
        }

        Vector2 directionToTarget = targetPositionCached - (Vector2)transform.position;

        for (int i = 0; i < interest.Length; i++)
        {
            float result = Vector2.Dot(directionToTarget.normalized, Directions.eightDirections[i]);

            if (result > 0 && result > interest[i])
            {
                interest[i] = result;
            }
        }

        interestsTemp = interest;
        return (danger, interest);
    }

    private void OnDrawGizmos()
    {
        if (showGizmo == false)
            return;

        Gizmos.DrawSphere(targetPositionCached, 0.2f);

        if (Application.isPlaying && interestsTemp != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < interestsTemp.Length; i++)
            {
                Gizmos.DrawRay(transform.position, Directions.eightDirections[i] * interestsTemp[i] * 2);
            }

            if (reachedLastTarget == false)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(targetPositionCached, 0.1f);
            }
        }
    }

}
