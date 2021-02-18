using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class Panel
{
    public bool isDone { get; private set; }
    private Queue<string> sentences;

    public Panel()
    {
        this.isDone = false;
        this.sentences = new Queue<string>();
    }

    public string GetNextSentence()
    {
        if (sentences.Count > 0)
        {
            return sentences.Dequeue();
        }
        else
        {
            isDone = true;
            return "";
        }
    }

    public void QueueSentence(string sentence)
    {
        sentences.Enqueue(sentence);
        isDone = false;
    }
}

public class TutorialManager : MonoBehaviour
{
    [SerializeField]
    private SongData song;
    [SerializeField]
    private TextMeshProUGUI moreTimesText;
    [SerializeField]
    private TextMeshProUGUI numberText;
    [SerializeField]
    private TextMeshProUGUI pressEscapeText;
    [SerializeField]
    private GameObject panelObject;
    [SerializeField]
    private TextMeshProUGUI panelText;
    [SerializeField]
    private GameObject bannerObject;
    [SerializeField]
    private TextMeshProUGUI bannerText;
    [SerializeField]
    private int nextBeatToSpawnItem = 12;
    private int itemsLeft;
    private Instrument nextInstrument;
    private TutorialStates tutorialState;
    private Panel panel = new Panel();

    private enum TutorialStates
    {
        None,
        PreLoopRight,
        WaitForSong,
        LoopOrangesRight,
        PreLoopLeft,
        LoopOrangesLeft,
        PreLoopBoth,
        LoopPineApples,
        PreEnd,
        End
    }

    void Start()
    {
        ShowUI(false);
        panelObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void StartTutorial()
    {
        tutorialState = TutorialStates.None;
        GetNextState();
    }

    public void StartTutorialLoop()
    {
        Conductor.instance.StartSong(song);
        GameManager.OnKeynotePressedSuccessfully += DecreaseNumber;
        GetNextState();
    }

    void Update()
    {
        if (tutorialState != TutorialStates.None)
        {
            if (panel.isDone)
            {
                UpdateTutorial();
            }
            else
            {
                UpdatePanelText();
            }
        }
    }

    void UpdateTutorial()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && tutorialState != TutorialStates.End)
        {
            tutorialState = TutorialStates.End;
            EndTutorial();
        }
        switch (tutorialState)
        {
            case TutorialStates.LoopOrangesRight:
                SpawnInstrumentLoop(Instrument.orangeR);
                break;
            case TutorialStates.LoopOrangesLeft:
                SpawnInstrumentLoop(Instrument.orangeL);
                break;
            case TutorialStates.LoopPineApples:
                SpawnPineApplesLoop();
                break;
            case TutorialStates.End:
                // component should be disabled
                return;
            default:
                break;
        }
    }

    private void UpdatePanelText()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetNextPanelSentence();
            AudioManager.instance.PlaySfx("ui_click2");
        }
    }

    private void SpawnInstrumentLoop(Instrument instrument)
    {
        if (Conductor.instance.songPositionInBeats >= nextBeatToSpawnItem)
        {
            nextBeatToSpawnItem += 8;
            GameManager.instance.CreateKeynoteNow(instrument);
        }
    }
    private void SpawnPineApplesLoop()
    {
        if (Conductor.instance.songPositionInBeats >= nextBeatToSpawnItem)
        {
            if (nextInstrument == Instrument.pineAppleR)
            {
                nextInstrument = Instrument.pineAppleL;
            }
            else
            {
                nextInstrument = Instrument.pineAppleR;
            }
            nextBeatToSpawnItem += 8;
            GameManager.instance.CreateKeynoteNow(nextInstrument);
        }
    }

    private void GetNextState()
    {
        tutorialState++;
        switch (tutorialState)
        {
            case TutorialStates.PreLoopRight:
                QueueSentence("Fruit just bounces down these stairs. Let's catch some of it!");
                QueueSentence("First it will come down on the right (press \"J\" to catch).");
                break;
            case TutorialStates.WaitForSong:
                ShowInBanner("Right side: press \"J\" to catch");
                ShowUI(true);
                AudioManager.instance.PlaySongWithCallback(song.presong_clip, StartTutorialLoop);
                break;
            case TutorialStates.PreLoopLeft:
                ShowUI(false);
                QueueSentence("But that's not all!");
                QueueSentence("Next, it will down on the left (press \"F\" to catch).");
                break;
            case TutorialStates.PreLoopBoth:
                ShowUI(false);
                QueueSentence("Yes! That's it!");
                QueueSentence("Incoming pineapple!");
                break;
            case TutorialStates.PreEnd:
                ShowUI(false);
                QueueSentence("You really dodged a seed pod there!");
                QueueSentence("Let's do it for real now!");
                break;
            case TutorialStates.LoopOrangesRight:
                ShowUI(true);
                ResetMoreTimesText();
                break;
            case TutorialStates.LoopOrangesLeft:
                ShowUI(true);
                ShowInBanner("Left side: press \"F\" to catch");
                ResetMoreTimesText();
                nextBeatToSpawnItem = (int)Conductor.instance.songPositionInBeats + 4;
                nextBeatToSpawnItem += 4 - (nextBeatToSpawnItem % 4);
                break;
            case TutorialStates.LoopPineApples:
                ShowUI(true);
                ResetMoreTimesText();
                MoveTextsTop();
                nextBeatToSpawnItem = (int)Conductor.instance.songPositionInBeats;
                nextBeatToSpawnItem += 8 - (nextBeatToSpawnItem % 8);
                break;
            case TutorialStates.End:
                EndTutorial();
                break;
        }
    }

    private void EndTutorial()
    {
        ShowUI(false);
        panelObject.SetActive(false);
        GameManager.OnKeynotePressedSuccessfully -= DecreaseNumber;
        GameManager.instance.EndTutorial();
        this.enabled = false;
    }

    private void DecreaseNumber(StairsSide side, FruitType type)
    {
        if (itemsLeft == 1)
        {
            GetNextState();
        }
        else
        {
            itemsLeft--;
            numberText.text = itemsLeft.ToString();
            if (itemsLeft == 1)
            {
                moreTimesText.text = "More time!";
            }
        }
    }

    public void ShowUI(bool show = true)
    {
        moreTimesText.gameObject.SetActive(show);
        numberText.gameObject.SetActive(show);
        pressEscapeText.gameObject.SetActive(show);
        bannerObject.SetActive(show);
    }

    private void ResetMoreTimesText()
    {
        itemsLeft = 3;
        numberText.text = itemsLeft.ToString();
        moreTimesText.text = "More times!";
    }

    private void MoveTextsTop()
    {
        bannerObject.SetActive(false);
        numberText.rectTransform.anchoredPosition = new Vector2(
            numberText.rectTransform.anchoredPosition.x, 0f);
        moreTimesText.rectTransform.anchoredPosition = Vector2.zero;
        pressEscapeText.rectTransform.anchoredPosition = Vector2.zero;
    }

    private void ShowInBanner(string text)
    {
        bannerObject.SetActive(true);
        bannerText.text = text;
    }

    private void QueueSentence(string sentence)
    {
        panel.QueueSentence(sentence);
        if (!panelObject.activeInHierarchy)
        {
            panelObject.SetActive(true);
            GetNextPanelSentence();
        }
    }

    private void GetNextPanelSentence()
    {
        string sentence = panel.GetNextSentence();
        if (sentence != "")
        {
            panelText.text = sentence;
        }
        else
        {
            panelObject.SetActive(false);
            GetNextState();
        }
    }
}
