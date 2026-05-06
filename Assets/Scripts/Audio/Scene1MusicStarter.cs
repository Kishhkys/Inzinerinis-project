using UnityEngine;
using System.Collections;

public class Scene1MusicStarter : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return null;
        AmbientSoundManager.PlayMusic("Ambience", 1f);
    }
}