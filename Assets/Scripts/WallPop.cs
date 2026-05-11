using UnityEngine;

public class WallPop : MonoBehaviour
{
    public Transform player;
    public SpriteRenderer sr;

    public float nearDistance = 2f;
    public float maxScale = 1.1f;
    public float minScale = 1f;

    void Update()
    {
        float d = Vector2.Distance(player.position, transform.position);

        float t = Mathf.InverseLerp(nearDistance, 0f, d);

        float scale = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = new Vector3(scale, scale, 1f);

        Color c = sr.color;
        c.a = Mathf.Lerp(0.6f, 1f, t);
        sr.color = c;
    }
}