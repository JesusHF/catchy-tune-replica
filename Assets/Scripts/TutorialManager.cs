using UnityEngine;
using TMPro;
using System;

public class TutorialManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI numberText;
    [SerializeField]
    private FruitSpawner fruitSpawner;
    private int itemsLeft;
    private int nextBeatToSpawnItem;
    private TutorialStates tutorialState;

    // tutorial states
    private enum TutorialStates
    {
        SpawnLoop,
        End
    }

    void Start()
    {
        tutorialState = TutorialStates.SpawnLoop;
        itemsLeft = 3;
        numberText.text = itemsLeft.ToString();
        nextBeatToSpawnItem = 12;
    }

    void Update()
    {
        switch (tutorialState)
        {
            case TutorialStates.SpawnLoop:
                SpawnOrangeLoop();
                break;
            case TutorialStates.End:
                // end tutorial, start game
                break;
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
            fruitSpawner.SpawnOrange();
        }
    }

    void DecreaseNumber()
    {
        if (itemsLeft == 1)
        {
            tutorialState++;
        }
        else
        {
            itemsLeft--;
            numberText.text = itemsLeft.ToString();
        }
    }

}
