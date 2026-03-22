using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TargetDetector : Detector
{
    [SerializeField]
    private float targetDetectionRange = 5;

    [SerializeField]
    private LayerMask obstaclesLayerMask, playerLayerMask;

    [SerializeField]
    private bool showGizmos = false;

    //gizmo parameters
    private List<Transform> colliders;

    //public override void Detect(AIData aiData)
    //{
    //    //Find out if player is near
    //    Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, targetDetectionRange, playerLayerMask);
    //    Debug.Log($"OverlapCircle result: {playerCollider}");
    //    if (playerCollider != null)
    //    {
    //        //Check if you see the player
    //        Vector2 direction = (playerCollider.transform.position - transform.position).normalized;
    //        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, targetDetectionRange, obstaclesLayerMask);

    //        //Make sure that the collider we see is on the "Player" layer
    //        if (hit.collider != null && (playerLayerMask & (1 << hit.collider.gameObject.layer)) != 0)
    //        {
    //            Debug.DrawRay(transform.position, direction * targetDetectionRange, Color.magenta);
    //            colliders = new List<Transform>() { playerCollider.transform };
    //        }
    //        else
    //        {
    //            colliders = null;
    //        }
    //    }
    //    else
    //    {
    //        //Enemy doesn't see the player
    //        colliders = null;
    //    }
    //    aiData.targets = colliders;

    //}

    public override void Detect(AIData aiData)
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, targetDetectionRange, playerLayerMask);

        Debug.Log($"OverlapCircle: {(playerCollider != null ? playerCollider.name : "NONE")}");

        if (playerCollider == null)
        {
            aiData.targets = null;
            return;
        }

        Vector2 direction = (playerCollider.transform.position - transform.position).normalized;

        int combinedMask = obstaclesLayerMask | playerLayerMask;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, targetDetectionRange, combinedMask);

        if (hit.collider != null)
        {
            Debug.Log($"Raycast hit: {hit.collider.name} | Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
        }
        else
        {
            Debug.Log("Raycast hit: NONE");
        }

        if (hit.collider != null && ((1 << hit.collider.gameObject.layer) & playerLayerMask) != 0)
        {
            Debug.Log("Player VISIBLE");
            aiData.targets = new List<Transform> { playerCollider.transform };
        }
        else
        {
            Debug.Log("Player BLOCKED");
            aiData.targets = null;
        }


    }

    private void OnDrawGizmosSelected()
    {
        if (showGizmos == false)
            return;

        Gizmos.DrawWireSphere(transform.position, targetDetectionRange);

        if (colliders == null)
            return;
        Gizmos.color = Color.magenta;
        foreach (var item in colliders)
        {
            Gizmos.DrawSphere(item.position, 0.3f);
        }
    }
}
