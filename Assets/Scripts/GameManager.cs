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

public enum GameStates
{
    None,
    Tutorial,
    GameTransition,
    Game,
    EndGameTransition,
    EndGame
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
    public EndGameManager endGameManager;
    public Image blackSquareImage;
    public SongData gameSong;
    public GameObject objectsInGame;
    public static event Action OnGameEnded;
    public static event Action<StairsSide, FruitType> OnKeynotePressedSuccessfully;
    public static event Action<StairsSide, FruitType> OnKeynoteNotPressed;
    private GameStates currentState;
    private int numberOfFails;

    void Start()
    {
        currentState = GameStates.None;
        GetNextState();
    }

    private void GetNextState()
    {
        currentState++;
        switch (currentState)
        {
            case GameStates.Tutorial:
                if (tutorialManager != null)
                {
                    StartCoroutine(FadeBlackSquare(blackSquareImage, 2f, 0f, () =>
                    {
                        tutorialManager.gameObject.SetActive(true);
                        tutorialManager.StartTutorial();
                    }));
                }
                else
                {
                    currentState++;
                    GetNextState();
                }
                break;
            case GameStates.GameTransition:
                AudioManager.instance.FadeCurrentSong(3f);
                StartCoroutine(FadeBlackSquare(blackSquareImage, 3f, 1f, GetNextState));
                break;
            case GameStates.Game:
                Conductor.instance.StopSong();
<<<<<<< Updated upstream
                StartCoroutine(FadeBlackSquare(blackSquareImage, 3f, 0f, () =>
                {
                    AudioManager.instance.PlaySongWithCallback(gameSong.presong_clip, StartGame);
                }));
=======
                StartCoroutine(FadeBlackSquare(blackSquareImage, 3f, 0f, PlayPreSong));
>>>>>>> Stashed changes
                break;
            case GameStates.EndGameTransition:
                keynoteHolder.enabled = false;
                Conductor.instance.StopSong();
                AudioManager.instance.FadeCurrentSong(3f);
                StartCoroutine(FadeBlackSquare(blackSquareImage, 4f, 1f, GetNextState));
                break;
            case GameStates.EndGame:
                OnGameEnded?.Invoke();
                OnGameEnded = null;
                objectsInGame.SetActive(false);
                StartEndGame();
                break;
            default:
                break;
        }
    }

<<<<<<< Updated upstream
=======
    private void PlayPreSong()
    {
        if (gameSong.presong_clip == "")
        {
            StartGame();
            return;
        }
        AudioManager.instance.PlaySongWithCallback(gameSong.presong_clip, StartGame);
    }

>>>>>>> Stashed changes
    private void StartGame()
    {
        Conductor.instance.StartSong(gameSong);
        keynoteHolder.PreprocessSongNotes(gameSong.keynotes, gameSong.finish_beat);
        numberOfFails = 0;
    }

    private void StartEndGame()
    {
        endGameManager.gameObject.SetActive(true);
        float score = (1f - ((float)numberOfFails / gameSong.keynotes.Length)) * 100f;
        endGameManager.StartEndGame((int)score);
    }

    public void CreateKeynoteNow(Instrument instrument = Instrument.orangeR)
    {
        if (instrument == Instrument.orangeL || instrument == Instrument.orangeR)
        {
            keynoteHolder.QueueSoundEffectInBeats(1f, "bounce", 0.5f);
            keynoteHolder.QueueSoundEffectInBeats(2f, "bounce", 0.5f);
            keynoteHolder.QueueSoundEffectInBeats(3f, "bounce", 0.5f);
            keynoteHolder.QueueNoteInBeats(4f, instrument);
        }
        else
        {
            keynoteHolder.QueueSoundEffectInBeats(1f, "bounce2", 0.5f);
            keynoteHolder.QueueSoundEffectInBeats(3f, "bounce2", 0.5f);
            keynoteHolder.QueueSoundEffectInBeats(5f, "bounce2", 0.5f);
            keynoteHolder.QueueNoteInBeats(7f, instrument);
        }
        CreateFruitNow(instrument);
    }

    public void CreateTwoKeynotesNow(Instrument instrument1, Instrument instrument2)
    {
        if (instrument1 == Instrument.orangeL || instrument1 == Instrument.orangeR)
        {
            keynoteHolder.QueueSoundEffectInBeats(1f, "bounce", 1f);
            keynoteHolder.QueueSoundEffectInBeats(2f, "bounce", 1f);
            keynoteHolder.QueueSoundEffectInBeats(3f, "bounce", 1f);
            keynoteHolder.QueueNoteInBeats(4f, instrument1);
            keynoteHolder.QueueNoteInBeats(4f, instrument2);
        }
        else
        {
            keynoteHolder.QueueSoundEffectInBeats(1f, "bounce2", 1f);
            keynoteHolder.QueueSoundEffectInBeats(3f, "bounce2", 1f);
            keynoteHolder.QueueSoundEffectInBeats(5f, "bounce2", 1f);
            keynoteHolder.QueueNoteInBeats(7f, instrument1);
            keynoteHolder.QueueNoteInBeats(7f, instrument2);
        }
        CreateTwoFruitsNow(instrument1, instrument2);
    }

    public void CreateFruitNow(Instrument instrument)
    {
        fruitSpawner.SpawnFruit(instrument);
    }

    public void CreateTwoFruitsNow(Instrument instrument1, Instrument instrument2)
    {
        CreateFruitNow(instrument1);
        CreateFruitNow(instrument2);
    }

    public FruitType CheckCurrentBeatHasAnyNoteInSide(StairsSide side)
    {
        FruitType fruit = keynoteHolder.CheckCurrentBeatHasAnyFruitInSide(side);
        if (fruit != FruitType.None)
        {
            OnKeynotePressedSuccessfully?.Invoke(side, fruit);
        }
        return fruit;
    }

    public void NotifyNotePassedInSide(StairsSide side, FruitType fruitType)
    {
        OnKeynoteNotPressed?.Invoke(side, fruitType);
        numberOfFails++;
    }

    public void EndTutorial()
    {
        if (currentState == GameStates.Tutorial)
        {
            tutorialManager.gameObject.SetActive(false);
            GetNextState();
        }
    }

    public void EndGame()
    {
        if (currentState == GameStates.Game)
        {
            GetNextState();
        }
    }

    public static IEnumerator FadeBlackSquare(Image blackSquareImage, float duration, float finalAlpha = 0f, Action onFinished = null)
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
