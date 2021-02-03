using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Fruit
{
    public const string ORANGE_LOOP = "orange_spawn";
    public const string ORANGE_HIT = "orange_hit";
    public const string PINEAPPLE_LOOP = "pineapple_spawn";
    public const string PINEAPPLE_HIT = "pineapple_hit";
    public GameObject fruitObject;
    public Animator animator;
    public Instrument type;
    public float lifeSpawn;
    public float animationOffset;
    public bool isSynced;
    public Fruit(GameObject fruitObject, Instrument type, float lifeSpawn, float animationOffset, bool isSynced = true)
    {
        this.fruitObject = fruitObject;
        this.animator = fruitObject.GetComponent<Animator>();
        this.type = type;
        this.lifeSpawn = lifeSpawn;
        this.animationOffset = animationOffset;
        this.isSynced = isSynced;
    }

    internal void PlaySyncedAnimation(string animationName, float time)
    {
        isSynced = true;
        fruitObject.SetActive(true);
        animator.speed = 0f;
        animator.Play(animationName);
        lifeSpawn = time;
        animationOffset = Conductor.instance.loopPositionInAnalog;
    }

    internal void Update()
    {
        float analogPosition = Conductor.instance.loopPositionInAnalog - animationOffset;
        if (type == Instrument.orangeL || type == Instrument.orangeR)
        {
            analogPosition *= (8f / 4f);
        }
        else
        {
            analogPosition *= (8f / 7f);
        }
        int loopHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        animator.Play(loopHash, -1, analogPosition);
        lifeSpawn -= Time.deltaTime;
    }

    internal void PlayLastAnimation()
    {
        string animationName = GetLastAnimationName();
        isSynced = false;
        animator.Play(animationName);
        animator.speed = 1.5f;
    }

    internal void Release()
    {
        fruitObject.SetActive(false);
        isSynced = false;
        lifeSpawn = 0f;
        animationOffset = 0f;
    }

    private string GetLastAnimationName()
    {
        if (type == Instrument.orangeL || type == Instrument.orangeR)
        {
            return ORANGE_HIT;
        }
        else
        {
            return PINEAPPLE_HIT;
        }
    }
}

public class FruitSpawner : MonoBehaviour
{
    public GameObject fruitPrefab;

    private GameObject containerLeft;
    private GameObject containerRight;
    private List<Fruit> fruitBasket = new List<Fruit>();

    void Start()
    {
        containerLeft = new GameObject("ContainerLeft");
        containerRight = new GameObject("ContainerRight");
        containerLeft.transform.localScale = new Vector3(-1f, 1f, 1f);
        containerLeft.transform.parent = this.gameObject.transform;
        containerRight.transform.parent = this.gameObject.transform;
        InitFruitBasket();

        GameManager.OnKeynotePressedSuccessfully += StopFruitHitAnimations;
    }

    private void InitFruitBasket()
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject orangeLObject = Instantiate(fruitPrefab, containerLeft.transform);
            GameObject orangeRObject = Instantiate(fruitPrefab, containerRight.transform);
            orangeLObject.name = "OrangeLeft";
            orangeRObject.name = "OrangeRight";
            orangeLObject.SetActive(false);
            orangeRObject.SetActive(false);
            Fruit orangeL = new Fruit(orangeLObject, Instrument.orangeL, 0f, 0f, false);
            Fruit orangeR = new Fruit(orangeRObject, Instrument.orangeR, 0f, 0f, false);
            fruitBasket.Add(orangeL);
            fruitBasket.Add(orangeR);
        }
        for (int i = 0; i < 2; i++)
        {
            GameObject pineappleLObject = Instantiate(fruitPrefab, containerLeft.transform);
            GameObject pineappleRObject = Instantiate(fruitPrefab, containerRight.transform);
            pineappleLObject.name = "PineAppleLeft";
            pineappleRObject.name = "PineAppleRight";
            pineappleLObject.SetActive(false);
            pineappleRObject.SetActive(false);
            Fruit pineappleL = new Fruit(pineappleLObject, Instrument.pineAppleL, 0f, 0f, false);
            Fruit pineappleR = new Fruit(pineappleRObject, Instrument.pineAppleR, 0f, 0f, false);
            fruitBasket.Add(pineappleL);
            fruitBasket.Add(pineappleR);
        }
    }

    public void SpawnFruit(Instrument instrument)
    {
        switch (instrument)
        {
            case Instrument.orangeL:
                SpawnOrange(StairsSide.Left);
                break;
            case Instrument.orangeR:
                SpawnOrange(StairsSide.Right);
                break;
            case Instrument.pineAppleL:
                SpawnPineapple(StairsSide.Left);
                break;
            case Instrument.pineAppleR:
                SpawnPineapple(StairsSide.Right);
                break;
            default:
                break;
        }
    }

    public void SpawnOrange(StairsSide side)
    {
        Instrument fruitType = side == StairsSide.Left ? Instrument.orangeL : Instrument.orangeR;
        int orangeIndex = GetFruitFromPool(fruitType);
        float orangelifeSpawn = 4 * Conductor.instance.secPerBeat;
        fruitBasket[orangeIndex].PlaySyncedAnimation(Fruit.ORANGE_LOOP, orangelifeSpawn);
    }

    public void SpawnPineapple(StairsSide side)
    {
        Instrument fruitType = side == StairsSide.Left ? Instrument.pineAppleL : Instrument.pineAppleR;
        int pineIndex = GetFruitFromPool(fruitType);
        float pinelifeSpawn = 7 * Conductor.instance.secPerBeat;
        fruitBasket[pineIndex].PlaySyncedAnimation(Fruit.PINEAPPLE_LOOP, pinelifeSpawn);
    }

    void Update()
    {
        if (fruitBasket.Count > 0)
        {
            foreach (Fruit fruit in fruitBasket)
            {
                if (fruit.lifeSpawn > 0f)
                {
                    fruit.Update();
                }
                else if (fruit.isSynced)
                {
                    fruit.PlayLastAnimation();
                    StartCoroutine(StartReleaseCountDown(fruit, 0.7f));
                }
            }
        }
    }

    private IEnumerator StartReleaseCountDown(Fruit fruit, float time)
    {
        yield return new WaitForSeconds(time);
        fruit.Release();
    }

    void StopFruitHitAnimations()
    {
        if (fruitBasket.Count > 0)
        {
            foreach (var fruit in fruitBasket)
            {
                if (fruit.fruitObject.activeInHierarchy && fruit.lifeSpawn <= 0.25f)
                {
                    fruit.Release();
                    break;
                    // todo: check fruit side
                }
            }
        }
    }

    int GetFruitFromPool(Instrument fruitType)
    {
        for (int i = 0; i < fruitBasket.Count; i++)
        {
            if (fruitBasket[i].lifeSpawn == 0f && fruitBasket[i].type == fruitType)
            {
                return i;
            }
        }
        return -1;
    }
}
