using System;
using System.Collections;
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

    public static event Action OnKeynotePressedSuccessfully;
    public FruitSpawner fruitSpawner;
    public KeynoteHolder keynoteHolder;
    public SongData[] songs;

    void Start()
    {
        StartTutorial();
    }

    public void CreateKeynote()
    {
        float currentBeat = Conductor.instance.songPositionInBeats;
        StartCoroutine(ScheduleSoundEffect("bounce", currentBeat + (1)));
        StartCoroutine(ScheduleSoundEffect("bounce", currentBeat + (2)));
        StartCoroutine(ScheduleSoundEffect("bounce", currentBeat + (3)));
        keynoteHolder.CreateKeynote(Conductor.instance.songPosition + (4 * Conductor.instance.secPerBeat));
        fruitSpawner.SpawnOrange();
    }

    public bool CheckCurrentBeatHasAnyNote()
    {
        if (keynoteHolder.CheckCurrentBeatHasAnyNote())
        {
            OnKeynotePressedSuccessfully?.Invoke();
            return true;
        }
        return false;
    }

    public void NotifyNotePassed()
    {
        Debug.Log("Note passed");
        // player animate fail
        // play fail sfx
        // track fails
    }

    private void StartTutorial()
    {
        Conductor.instance.StartSong(songs[0]);
    }

    public void EndTutorial()
    {
        // transition to game
        AudioManager.instance.FadeCurrentSong(3f);
        Invoke("StartGame", 4f);
    }

    public void StartGame()
    {
        Conductor.instance.StartSong(songs[1]);
    }


    private IEnumerator ScheduleSoundEffect(string sfxName, float beat)
    {
        while (Conductor.instance.songPositionInBeats < beat)
        {
            yield return null;
        }
        AudioManager.instance.PlaySfx(sfxName);
    }


}
