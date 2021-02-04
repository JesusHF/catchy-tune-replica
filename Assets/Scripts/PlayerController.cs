﻿using UnityEngine;

public class PlayerStates
{
    // animation states
    public const string Loop = "loop";
    public const string GrabOrange = "player_grab_orange";
    public const string GrabPineapple = "player_grab_pineapple";
    public const string GrabFail = "player_grab_fail";
    public const string GrabPassed = "player_grab_passed";
    public const string GrabPassed2 = "player_grab_passed_2";
}

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private KeyCode actionKey = KeyCode.Space;
    [SerializeField]
    private StairsSide playerSide;
    private Animator animator;

    private bool isGrabbing;
    private string currentState;
    private float lockInput;
    const float durationInBeats = 1f;

    void Start()
    {
        currentState = PlayerStates.Loop;
        animator = GetComponent<Animator>();
        animator.speed = 0;
        animator.Play(currentState);

        isGrabbing = false;
        GameManager.OnKeynoteNotPressed += SetGrabPassedAnimation;
    }

    void Update()
    {
        if (lockInput <= 0f)
        {
            if (Input.GetKeyDown(actionKey))
            {
                isGrabbing = true;
            }
        }
        else
        {
            lockInput -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (isGrabbing)
        {
            FruitType fruit = GameManager.instance.CheckCurrentBeatHasAnyNoteInSide(playerSide);
            if (fruit != FruitType.None)
            {
                string animation = fruit == FruitType.Orange ?
                    PlayerStates.GrabOrange : PlayerStates.GrabPineapple;
                ChangeAnimationState(animation);
                AudioManager.instance.PlaySfx("grab_success");
            }
            else
            {
                ChangeAnimationState(PlayerStates.GrabFail);
                AudioManager.instance.PlaySfx("grab_fail");
            }

            isGrabbing = false;
            ScheduleLoopAnimation();
        }
        else if (currentState == PlayerStates.Loop)
        {
            float analogPosition = Conductor.instance.loopPositionInAnalog * (8f / durationInBeats);
            int loopHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
            animator.Play(loopHash, -1, analogPosition);
        }
    }


    public void ChangeAnimationState(string newState, float speed = 1f)
    {
        if (currentState == newState)
        {
            // restart the animation
            animator.Play(currentState, -1, 0f);
        }
        else
        {
            // play the animation
            animator.Play(newState);
            animator.speed = speed;
        }

        // reassign the current state
        currentState = newState;
    }

    void ScheduleLoopAnimation()
    {
        float delayedTime = Mathf.Max(Conductor.instance.GetTimeToNextBeat(), 0.35f);
        CancelInvoke("SetLoopAnimation");
        Invoke("SetLoopAnimation", delayedTime);
    }

    void SetLoopAnimation()
    {
        ChangeAnimationState(PlayerStates.Loop, 0f);
    }

    void SetGrabPassedAnimation(StairsSide side, FruitType fruitType)
    {
        if (side == playerSide)
        {
            string grabAnimation = "";
            if (fruitType == FruitType.Orange)
            {
                grabAnimation = PlayerStates.GrabPassed;
            }
            else
            {
                grabAnimation = PlayerStates.GrabPassed2;
            }
            ChangeAnimationState(grabAnimation);
            lockInput = 0.5f;
            AudioManager.instance.PlaySfx("grab_passed");
            ScheduleLoopAnimation();
        }
    }
}
