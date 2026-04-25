using UnityEngine;

public interface ITeleportable
{
    void Teleport(Vector3 newPosition, float blockDuration = 0.2f);
    bool CanTeleport();
}
