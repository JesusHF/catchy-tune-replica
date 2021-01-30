using UnityEngine;
using TMPro;
using System;

public class TutorialManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI moreTimesText;
    [SerializeField]
    private TextMeshProUGUI numberText;
    [SerializeField]
    private int nextBeatToSpawnItem = 11;
    private int itemsLeft;
    private TutorialStates tutorialState;

    // tutorial states
    private enum TutorialStates
    {
        None,
        SpawnLoop,
        End
    }

    void Start()
    {
        tutorialState = TutorialStates.None;
        ShowUI(false);
    }

    public void StartTutorial()
    {
        ShowUI(true);
        GetNextState();
        GameManager.OnKeynotePressedSuccessfully += DecreaseNumber;
    }

    void Update()
    {
        switch (tutorialState)
        {
            case TutorialStates.None:
                break;
            case TutorialStates.SpawnLoop:
                SpawnOrangeLoop();
                break;
            case TutorialStates.End:
                // component should be disabled
                return;
            default:
                break;
        }
    }

    private void SpawnOrangeLoop()
    {
        float currentBeat = Conductor.instance.songPositionInBeats;
        if (currentBeat >= nextBeatToSpawnItem)
        {
            nextBeatToSpawnItem += 8;
            GameManager.instance.CreateKeynoteNow();
        }
    }

    private void GetNextState()
    {
        tutorialState++;
        switch (tutorialState)
        {
            case TutorialStates.SpawnLoop:
                itemsLeft = 3;
                numberText.text = itemsLeft.ToString();
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
                moreTimesText.text = moreTimesText.text.Replace("times!", "time!");
            }
        }
    }

    private void ShowUI(bool show = true)
    {
        moreTimesText.gameObject.SetActive(show);
        numberText.gameObject.SetActive(show);
    }

}
