using System.Collections.Generic;
using UnityEngine;

public class PatrolBehaviour : SteeringBehaviour
{
    //[SerializeField] private float patrolPointReachedThreshold = 0.5f;
    //[SerializeField] private PathFinder pathFinder;

    //private List<PathNode> currentPath;
    //private int currentPathIndex;

    //public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData aiData)
    //{
    //    if (aiData == null) return (danger, interest);
    //    if (aiData.patrolPoints == null || aiData.patrolPoints.Count == 0)
    //    {
    //        aiData.currentPatrolTarget = null;
    //        return (danger, interest);
    //    }

    //    if (aiData.currentPatrolIndex < 0 || aiData.currentPatrolIndex >= aiData.patrolPoints.Count)
    //        aiData.currentPatrolIndex = 0;

    //    Transform patrolTarget = aiData.patrolPoints[aiData.currentPatrolIndex];
    //    if (patrolTarget == null)
    //    {
    //        aiData.currentPatrolTarget = null;
    //        return (danger, interest);
    //    }


    //    if (currentPath == null || currentPath.Count == 0)
    //    {
    //        currentPath = pathFinder.FindPath(transform.position, patrolTarget.position);
    //        currentPathIndex = 0;
    //    }


    //    float distToFinal = Vector2.Distance(transform.position, patrolTarget.position);
    //    if (distToFinal <= patrolPointReachedThreshold)
    //    {
    //        aiData.currentPatrolIndex = (aiData.currentPatrolIndex + 1) % aiData.patrolPoints.Count;
    //        currentPath = null;
    //        return (danger, interest);
    //    }


    //    if (currentPath != null && currentPathIndex < currentPath.Count)
    //    {
    //        Vector2 nextNode = currentPath[currentPathIndex].transform.position;
    //        float distToNode = Vector2.Distance(transform.position, nextNode);

    //        if (distToNode <= patrolPointReachedThreshold)
    //        {
    //            currentPathIndex++;
    //            if (currentPathIndex >= currentPath.Count)
    //            {
    //                currentPath = null;
    //                return (danger, interest);
    //            }
    //            nextNode = currentPath[currentPathIndex].transform.position;
    //        }

    //        aiData.currentPatrolTarget = currentPath[currentPathIndex].transform;

    //        Vector2 directionToTarget = (nextNode - (Vector2)transform.position).normalized;

    //        for (int i = 0; i < Directions.eightDirections.Count; i++)
    //        {
    //            float result = Vector2.Dot(directionToTarget, Directions.eightDirections[i]);
    //            if (result > 0 && result > interest[i])
    //                interest[i] = result;
    //        }
    //    }

    //    return (danger, interest);
    //}
    [SerializeField] private float patrolPointReachedThreshold = 0.5f;
    [SerializeField] private PathFinder pathFinder;

    [Header("Stuck recovery")]
    [SerializeField] private float nodeStuckTimeout = 1.5f;
    [SerializeField] private float repathCooldown = 0.5f;

    private List<PathNode> currentPath;
    private int currentPathIndex;

    private float timeOnCurrentNode;
    private float lastDistToCurrentNode;
    private float lastRepathTime = -999f;

    /// <summary>
    /// Iskvieciama is isores (EnemyAI) kad priverstu perskaiciuoti path.
    /// </summary>
    public void ForceRepath()
    {
        currentPath = null;
        currentPathIndex = 0;
        timeOnCurrentNode = 0f;
        // Atstatom cooldown, kad iskart galetume bandyti is naujo
        lastRepathTime = -999f;
    }

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

        // Apskaiciuojam path, jei jo nera
        if (currentPath == null || currentPath.Count == 0)
        {
            // Cooldown — kad per daznai nebandytume rasti kelio
            if (Time.time < lastRepathTime + repathCooldown)
            {
                aiData.currentPatrolTarget = null;
                return (danger, interest);
            }

            currentPath = pathFinder.FindPath(transform.position, patrolTarget.position);
            currentPathIndex = 0;
            timeOnCurrentNode = 0f;
            lastRepathTime = Time.time;

            if (currentPath == null || currentPath.Count == 0)
            {
                aiData.currentPatrolTarget = null;
                return (danger, interest);
            }

            // Jei pirmas node yra ten pat kur jau esam — praleidziam
            while (currentPathIndex < currentPath.Count - 1)
            {
                if (currentPath[currentPathIndex] == null) break;
                float distToFirst = Vector2.Distance(transform.position, currentPath[currentPathIndex].transform.position);
                if (distToFirst <= patrolPointReachedThreshold)
                    currentPathIndex++;
                else
                    break;
            }
        }

        // Patikrinam ar pasiekem galutini patrol target tiesiogiai
        float distToFinal = Vector2.Distance(transform.position, patrolTarget.position);
        if (distToFinal <= patrolPointReachedThreshold)
        {
            aiData.currentPatrolIndex = (aiData.currentPatrolIndex + 1) % aiData.patrolPoints.Count;
            currentPath = null;
            timeOnCurrentNode = 0f;
            return (danger, interest);
        }

        if (currentPath != null && currentPathIndex < currentPath.Count)
        {
            if (currentPath[currentPathIndex] == null)
            {
                ForceRepath();
                return (danger, interest);
            }

            Vector2 nextNode = currentPath[currentPathIndex].transform.position;
            float distToNode = Vector2.Distance(transform.position, nextNode);

            // Jei pasiekem dabartini node — pereinam i sekanti
            if (distToNode <= patrolPointReachedThreshold)
            {
                currentPathIndex++;
                timeOnCurrentNode = 0f;
                if (currentPathIndex >= currentPath.Count)
                {
                    currentPath = null;
                    return (danger, interest);
                }
                nextNode = currentPath[currentPathIndex].transform.position;
                distToNode = Vector2.Distance(transform.position, nextNode);
            }

            // Stuck detection per node — jei per ilgai negalim pasiekti, perskaiciuojam path
            if (Mathf.Abs(distToNode - lastDistToCurrentNode) < 0.02f)
            {
                timeOnCurrentNode += Time.deltaTime;
            }
            else
            {
                timeOnCurrentNode = 0f;
            }
            lastDistToCurrentNode = distToNode;

            if (timeOnCurrentNode > nodeStuckTimeout && Time.time > lastRepathTime + repathCooldown)
            {
                ForceRepath();
                return (danger, interest);
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
