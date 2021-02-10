using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
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
    private int nextBeatToSpawnItem = 11;
    private int itemsLeft;
    private Instrument nextInstrument;
    private TutorialStates tutorialState;

    private enum TutorialStates
    {
        None,
        SpawnLoopOrangesRight,
        SpawnLoopOrangesLeft,
        SpawnLoopPineApples,
        End
    }

    void Start()
    {
        tutorialState = TutorialStates.None;
        ShowUI(false);
    }

    public void StartTutorial()
    {
        tutorialState = TutorialStates.None;
        ShowUI(true);
        GetNextState();
        GameManager.OnKeynotePressedSuccessfully += DecreaseNumber;
    }

    void Update()
    {
        if (tutorialState != TutorialStates.None)
        {
            UpdateTutorial();
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
            case TutorialStates.SpawnLoopOrangesRight:
                SpawnInstrumentLoop(Instrument.orangeR);
                break;
            case TutorialStates.SpawnLoopOrangesLeft:
                SpawnInstrumentLoop(Instrument.orangeL);
                break;
            case TutorialStates.SpawnLoopPineApples:
                SpawnPineApplesLoop();
                break;
            case TutorialStates.End:
                // component should be disabled
                return;
            default:
                break;
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
            case TutorialStates.SpawnLoopOrangesRight:
                panelObject.SetActive(false);
                ShowInBanner("Right side: press \"J\" to catch");
                ResetMoreTimesText();
                break;
            case TutorialStates.SpawnLoopOrangesLeft:
                panelObject.SetActive(false);
                ShowInBanner("Left side: press \"F\" to catch");
                ResetMoreTimesText();
                break;
            case TutorialStates.SpawnLoopPineApples:
                panelObject.SetActive(false);
                bannerObject.SetActive(false);
                ResetMoreTimesText();
                MoveTextsTop();
                nextBeatToSpawnItem += nextBeatToSpawnItem % 8;
                break;
            case TutorialStates.End:
                EndTutorial();
                break;
        }
    }

    private void EndTutorial()
    {
        ShowUI(false);
        GameManager.OnKeynotePressedSuccessfully -= DecreaseNumber;
        GameManager.instance.EndTutorial();
        this.enabled = false;
    }

    void DecreaseNumber()
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
        panelObject.SetActive(show);
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
        numberText.rectTransform.anchoredPosition = Vector2.zero;
        moreTimesText.rectTransform.anchoredPosition = Vector2.zero;
        pressEscapeText.rectTransform.anchoredPosition = Vector2.zero;
    }

    private void ShowInPanel(string text)
    {
        panelObject.SetActive(true);
        panelText.text = text;
    }

    private void ShowInBanner(string text)
    {
        bannerObject.SetActive(true);
        bannerText.text = text;
    }

}
