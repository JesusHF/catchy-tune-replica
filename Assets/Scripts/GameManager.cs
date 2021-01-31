using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum StairsSide
{
    Left,
    Right
}

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
    public TutorialManager tutorialManager;
    public Image blackSquareImage;
    public SongData[] songs;
    public static event Action OnKeynotePressedSuccessfully;
    public static event Action<StairsSide> OnKeynoteNotPressed;

    void Start()
    {
        StartCoroutine(FadeBlackSquare(2f, 0f, () => {
            tutorialManager.ShowUI();
            AudioManager.instance.PlaySongWithCallback(songs[0].presong_clip, StartTutorial); 
        }));
    }

    public void CreateKeynoteNow()
    {
        float currentBeat = Conductor.instance.songPositionInBeats;
        StartCoroutine(ScheduleSoundEffect("bounce", currentBeat + 1));
        StartCoroutine(ScheduleSoundEffect("bounce", currentBeat + 2));
        StartCoroutine(ScheduleSoundEffect("bounce", currentBeat + 3));
        keynoteHolder.QueueNoteInBeat(4f);
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

    public void NotifyNotePassed(Instrument instrument = Instrument.orangeR)
    {
        StairsSide side = GetSideOfInstrument(instrument);
        OnKeynoteNotPressed?.Invoke(side);
        // track fails
    }

    private void StartTutorial()
    {
        Conductor.instance.StartSong(songs[0]);
        tutorialManager.StartTutorial();
    }

    public void EndTutorial()
    {
        tutorialManager.enabled = false;
        AudioManager.instance.FadeCurrentSong(3f);
        StartCoroutine(FadeBlackSquare(3f, 1f, TransitionToGame));
    }

    private void TransitionToGame()
    {
        Conductor.instance.StopSong();
        StartCoroutine(FadeBlackSquare(3f, 0f, ()=> { 
            AudioManager.instance.PlaySongWithCallback(songs[1].presong_clip, StartGame);
        }));
    }
    
    private void StartGame()
    {
        Conductor.instance.StartSong(songs[1]);
        keynoteHolder.PreprocessSongNotes(songs[1].keynotes);
    }


    private IEnumerator ScheduleSoundEffect(string sfxName, float beat)
    {
        while (Conductor.instance.songPositionInBeats < beat + 0.1f)
        {
            yield return null;
        }
        AudioManager.instance.PlaySfx(sfxName);
    }

    private IEnumerator FadeBlackSquare(float duration, float finalAlpha = 0f, Action onFinished = null)
    {
        float currentTime = 0;
        float startAlpha = 1 - finalAlpha;
        Color c = blackSquareImage.color;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, finalAlpha, currentTime / duration);
            blackSquareImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        onFinished?.Invoke();
        yield break;
    }

    private StairsSide GetSideOfInstrument(Instrument instrument)
    {
        if (instrument == Instrument.orangeL || instrument == Instrument.pineAppleL)
        {
            return StairsSide.Left;
        }
        else
        {
            return StairsSide.Right;
        }
    }
}
