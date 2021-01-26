using UnityEngine;

public class SyncedAnimator : MonoBehaviour
{
    private Animator animator;
    private AnimatorStateInfo animatorStateInfo;
    private int currentState;

    private float beatDuration = 1f;

    void Start()
    {
        animator = GetComponent<Animator>();
        animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        currentState = animatorStateInfo.fullPathHash;
    }

    void Update()
    {
        float analogPosition = Conductor.instance.loopPositionInAnalog * (8 / beatDuration);
        animator.Play(currentState, -1, analogPosition);
        animator.speed = 0;
    }

    public void SetBeats(float beats)
    {
        this.beatDuration = beats;
    }
}
