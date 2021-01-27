using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float durationInBeats = 1f;
    private Animator animator;

    private bool isGrabbing;
    private string currentState;

    // animation states
    const string PLAYER_LOOP = "loop";
    const string PLAYER_GRAB = "player_grab";
    const string PLAYER_GRAB_FAIL = "player_grab_fail";

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0;

        currentState = PLAYER_LOOP;
        isGrabbing = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isGrabbing = true;
        }
    }

    private void FixedUpdate()
    {
        if (isGrabbing)
        {
            if (true) // check if player hit the note right
            {
                ChangeAnimationState(PLAYER_GRAB_FAIL);
                // play grab fail sfx
            }
            else
            {
                ChangeAnimationState(PLAYER_GRAB);
                // play grab success sfx
            }

            isGrabbing = false;
            ScheduleLoopAnimation();
        }
        else if (currentState == PLAYER_LOOP)
        {
            float analogPosition = Conductor.instance.loopPositionInAnalog * (8 / durationInBeats);
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
        ChangeAnimationState(PLAYER_LOOP, 0f);
    }
}
