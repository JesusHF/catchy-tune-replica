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
        GetNextState();
        GameManager.OnKeynotePressedSuccessfully += DecreaseNumber;
    }

    void Update()
    {
        switch (tutorialState)
        {
            case TutorialStates.SpawnLoop:
                SpawnOrangeLoop();
                break;
            case TutorialStates.End:
                // object should be disabled
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
            nextBeatToSpawnItem += 16;
            GameManager.instance.CreateKeynote();
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
                // end tutorial, start game
                EndTutorial();
                break;
        }
    }

    private void EndTutorial()
    {
        GameManager.OnKeynotePressedSuccessfully -= DecreaseNumber;
        moreTimesText.gameObject.SetActive(false);
        numberText.gameObject.SetActive(false);
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

}
