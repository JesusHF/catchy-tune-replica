using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    public GameObject orange;

    private Animator orangeAnimator;
    private bool orangeIsEnabled;
    private float orangelifeSpawn;
    private float animationOffset;

    void Start()
    {
        orangeAnimator = orange.GetComponent<Animator>();
        orangeIsEnabled = false;
        orange.SetActive(false);
        orangelifeSpawn = 0f;
        animationOffset = 0f;
    }

    public void SpawnOrange()
    {
        orange.SetActive(true);
        orangeIsEnabled = true;
        orangeAnimator.Play("orange_spawn");
        orangelifeSpawn = 4 * Conductor.instance.secPerBeat;
        animationOffset = Conductor.instance.loopPositionInAnalog;
    }

    private void Update()
    {
        if (orangelifeSpawn > 0f)
        {
            UpdateOrangeAnimation();
            orangelifeSpawn -= Time.deltaTime;
        }
        else
        {
            if (orangeIsEnabled)
            {
                orangeIsEnabled = false;
                orange.SetActive(false);
            }
        }
    }

    private void UpdateOrangeAnimation()
    {
        float analogPosition = Conductor.instance.loopPositionInAnalog - animationOffset;
        analogPosition *= 2;
        int loopHash = orangeAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        orangeAnimator.Play(loopHash, -1, analogPosition);
    }
}
