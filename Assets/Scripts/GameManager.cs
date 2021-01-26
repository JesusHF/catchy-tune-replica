using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        Conductor.instance.StartSong();
    }

}
