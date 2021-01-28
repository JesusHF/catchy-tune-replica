using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager instance { get; private set; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    #endregion

    public FruitSpawner fruitSpawner;
    public KeynoteHolder keynoteHolder;

    void Start()
    {
        Conductor.instance.StartSong();
    }

    public void CreateKeynote()
    {
        float currentTime = Conductor.instance.songPosition + (4 * Conductor.instance.secPerBeat);
        keynoteHolder.CreateKeynote(currentTime);
        fruitSpawner.SpawnOrange();
    }

    public bool CheckCurrentBeatHasAnyNote()
    {
        return keynoteHolder.CheckCurrentBeatHasAnyNote();
    }

    public void NotifyNotePassed()
    {
        Debug.Log("Note passed");
        // player animate fail
        // play fail sfx
        // track fails
    }

}
