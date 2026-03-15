using UnityEngine;

public class InvestigateBehaviour : SteeringBehaviour
{
    [SerializeField] private float arriveThreshold = 0.4f;

    public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData aiData)
    {
        if (!aiData.hasLastSeenPosition)
            return (danger, interest);

        Vector2 directionToTarget = aiData.lastSeenPosition - (Vector2)transform.position;

        if (directionToTarget.magnitude <= arriveThreshold)
        {
            aiData.hasLastSeenPosition = false;
            return (danger, interest);
        }

        Vector2 dir = directionToTarget.normalized;

        for (int i = 0; i < interest.Length; i++)
        {
            float result = Vector2.Dot(dir, Directions.eightDirections[i]);

            if (result > 0 && result > interest[i])
            {
                interest[i] = result;
            }
        }

        return (danger, interest);
    }
}
