using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum StairsSide
{
    Left,
    Right
}

public enum FruitType
{
    None,
    Orange,
    PineApple
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
        StartCoroutine(FadeBlackSquare(2f, 0f, () =>
        {
            tutorialManager.ShowUI();
            AudioManager.instance.PlaySongWithCallback(songs[0].presong_clip, StartTutorial);
        }));
    }

    public void CreateKeynoteNow(Instrument instrument = Instrument.orangeR)
    {
        float currentBeat = Mathf.Floor(Conductor.instance.songPositionInBeats);
        if (instrument == Instrument.orangeL || instrument == Instrument.orangeR)
        {
            keynoteHolder.ScheduleSoundEffect(currentBeat + 1, "bounce", 1f);
            keynoteHolder.ScheduleSoundEffect(currentBeat + 2, "bounce", 1f);
            keynoteHolder.ScheduleSoundEffect(currentBeat + 3, "bounce", 1f);
            keynoteHolder.QueueNoteInBeat(4f, instrument);
        }
        else
        {
            // todo: change provisional bounce sfx
            keynoteHolder.ScheduleSoundEffect(currentBeat + 1, "bounce", 1f);
            keynoteHolder.ScheduleSoundEffect(currentBeat + 3, "bounce", 1f);
            keynoteHolder.ScheduleSoundEffect(currentBeat + 5, "bounce", 1f);
            keynoteHolder.QueueNoteInBeat(7f, instrument);
        }
        fruitSpawner.SpawnFruit(instrument);
    }

    public void CreateTwoKeynotesNow(Instrument instrument1, Instrument instrument2)
    {
        float currentBeat = Mathf.Floor(Conductor.instance.songPositionInBeats);
        if (instrument1 == Instrument.orangeL || instrument1 == Instrument.orangeR)
        {
            keynoteHolder.ScheduleSoundEffect(currentBeat + 1, "bounce", 1.3f);
            keynoteHolder.ScheduleSoundEffect(currentBeat + 2, "bounce", 1.3f);
            keynoteHolder.ScheduleSoundEffect(currentBeat + 3, "bounce", 1.3f);
            keynoteHolder.QueueNoteInBeat(4f, instrument1);
            keynoteHolder.QueueNoteInBeat(4f, instrument2);
        }
        else
        {
            // todo: change provisional bounce sfx
            keynoteHolder.ScheduleSoundEffect(currentBeat + 1, "bounce", 1.3f);
            keynoteHolder.ScheduleSoundEffect(currentBeat + 3, "bounce", 1.3f);
            keynoteHolder.ScheduleSoundEffect(currentBeat + 5, "bounce", 1.3f);
            keynoteHolder.QueueNoteInBeat(7f, instrument1);
            keynoteHolder.QueueNoteInBeat(7f, instrument2);
        }
        fruitSpawner.SpawnFruit(instrument1);
        fruitSpawner.SpawnFruit(instrument2);
    }

    public FruitType CheckCurrentBeatHasAnyNoteInSide(StairsSide side)
    {
        FruitType fruit = keynoteHolder.CheckCurrentBeatHasAnyFruitInSide(side);
        if (fruit != FruitType.None)
        {
            OnKeynotePressedSuccessfully?.Invoke();
        }
        return fruit;
    }

    public void NotifyNotePassedInSide(StairsSide side)
    {
        OnKeynoteNotPressed?.Invoke(side);
        // todo: track fails for score
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
        StartCoroutine(FadeBlackSquare(3f, 0f, () =>
        {
            AudioManager.instance.PlaySongWithCallback(songs[1].presong_clip, StartGame);
        }));
    }

    private void StartGame()
    {
        Conductor.instance.StartSong(songs[1]);
        keynoteHolder.PreprocessSongNotes(songs[1].keynotes);
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
}
