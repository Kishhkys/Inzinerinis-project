using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ContextSolver : MonoBehaviour
{
    private readonly bool showGizmos = true;

    float[] interestGizmo = new float[0];
    Vector2 resultDirection = Vector2.zero;
    private readonly float rayLength = 2;
    private const float MinSqrMagnitude = 0.01f;

    private void Start()
    {
        interestGizmo = new float[8];
    }

    public Vector2 GetDirectionToMove(List<SteeringBehaviour> behaviours, AIData aiData)
    {
        float[] danger = new float[8];
        float[] interest = new float[8];

        foreach (SteeringBehaviour behaviour in behaviours)
        {
            (danger, interest) = behaviour.GetSteering(danger, interest, aiData);
        }

        for (int i = 0; i < 8; i++)
        {
            interest[i] = Mathf.Clamp01(interest[i] - danger[i]);
        }

        interestGizmo = interest;

        Vector2 outputDirection = Vector2.zero;
        for (int i = 0; i < 8; i++)
        {
            outputDirection += Directions.eightDirections[i] * interest[i];
        }

        if (outputDirection.sqrMagnitude > MinSqrMagnitude)
        {
            outputDirection.Normalize();
            resultDirection = outputDirection;
        }
        else
        {
            resultDirection = Vector2.zero;
        }

        return resultDirection;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && showGizmos)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, resultDirection * rayLength);
        }
    }


}
