using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    [SerializeField] private Transform destination;
    [SerializeField] private Door requiredDoor;
    [SerializeField] private float cooldown = 0.2f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (destination == null) return;
        if (requiredDoor == null || !requiredDoor.IsOpened) return;

        ITeleportable teleportable = collision.GetComponentInParent<ITeleportable>();
        if (teleportable == null) return;
        if (!teleportable.CanTeleport()) return;

        teleportable.Teleport(destination.position, cooldown);
    }
}
