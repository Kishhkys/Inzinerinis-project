using UnityEngine;

public class Scene1MusicStarter : MonoBehaviour
{
    private void Start()
    {
        AmbientSoundManager.FadeInMusic("Ambience", 1f, 1.3f);
    }
}