using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    public static event Action OnKeynoteNotPressed;
    public FruitSpawner fruitSpawner;
    public KeynoteHolder keynoteHolder;
    public TutorialManager tutorialManager;
    public Image blackSquareImage;
    public SongData[] songs;

    void Start()
    {
        StartCoroutine(FadeBlackSquare(2f, 0f, StartTutorial));
    }

    public void CreateKeynote()
    {
        float currentBeat = Conductor.instance.songPositionInBeats;
        StartCoroutine(ScheduleSoundEffect("bounce", currentBeat + 1));
        StartCoroutine(ScheduleSoundEffect("bounce", currentBeat + 2));
        StartCoroutine(ScheduleSoundEffect("bounce", currentBeat + 3));
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
        OnKeynoteNotPressed?.Invoke();
        // track fails
    }

    private void StartTutorial()
    {
        Conductor.instance.StartSong(songs[0]);
        tutorialManager.StartTutorial();
    }

    public void EndTutorial()
    {
        AudioManager.instance.FadeCurrentSong(3f);
        StartCoroutine(FadeBlackSquare(3f, 1f, TransitionToGame));
    }

    private void TransitionToGame()
    {
        Conductor.instance.StopSong();
        StartCoroutine(FadeBlackSquare(3f, 0f, StartGame));
    }
    
    private void StartGame()
    {
        Conductor.instance.StartSong(songs[1]);
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
}
