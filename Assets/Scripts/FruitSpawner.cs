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
    public StairsSide side;
    public FruitType type;
    public float lifeSpawn;
    public float animationOffset;
    public float loopOffset;
    public bool isSynced;
    /// <summary>True while this fruit is taken from the pool, from spawn until Release.</summary>
    public bool isActive;
    public string currentAnimationName;
    public Fruit(GameObject fruitObject, StairsSide side, FruitType type, float lifeSpawn, float animationOffset, bool isSynced = true)
    {
        this.fruitObject = fruitObject;
        this.animator = fruitObject.GetComponent<Animator>();
        this.side = side;
        this.type = type;
        this.lifeSpawn = lifeSpawn;
        this.animationOffset = animationOffset;
        this.loopOffset = 0f;
        this.isSynced = isSynced;
        this.isActive = false;
    }

    internal void PlaySyncedAnimation(string animationName, float time)
    {
        isActive = true;
        isSynced = true;
        fruitObject.SetActive(true);
        animator.speed = 0f;
        animator.Play(animationName);
        currentAnimationName = animationName;
        lifeSpawn = time;
        animationOffset = Conductor.instance.loopPositionInAnalog;
        loopOffset = Conductor.instance.GetTimeToNextLoop() < lifeSpawn ?
           animationOffset : 0f;
    }

    internal void Update()
    {
        float analogPosition = Conductor.instance.loopPositionInAnalog;
        analogPosition += analogPosition < loopOffset ? (1f - animationOffset) : -animationOffset;
        if (type == FruitType.Orange)
        {
            analogPosition *= (8f / 4f);
        }
        else if (type == FruitType.PineApple)
        {
            analogPosition *= (8f / 7f);
        }
        // Play by the tracked name, not GetCurrentAnimatorStateInfo: on the frame
        // the fruit is spawned the animator has not evaluated the Play request
        // from PlaySyncedAnimation yet, so the current state is still the
        // controller default and replaying its hash would override the
        // animation we just queued.
        animator.Play(currentAnimationName, -1, analogPosition);
        lifeSpawn -= Time.deltaTime;
    }

    internal void PlayLastAnimation()
    {
        string animationName = GetLastAnimationName();
        isSynced = false;
        animator.Play(animationName);
        currentAnimationName = animationName;
        animator.speed = 1.5f;
    }

    internal void Release()
    {
        fruitObject.SetActive(false);
        isActive = false;
        isSynced = false;
        lifeSpawn = 0f;
        animationOffset = 0f;
    }

    private string GetLastAnimationName()
    {
        if (type == FruitType.Orange)
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
    public GameObject orangePrefab;
    public GameObject pineapplePrefab;

    private GameObject containerLeft;
    private GameObject containerRight;
    private List<Fruit> fruitBasket = new List<Fruit>();

    private void Start()
    {
        Init();
        GameManager.OnGameEnded += Destroy;
    }

    void Init()
    {
        containerLeft = new GameObject("ContainerLeft");
        containerRight = new GameObject("ContainerRight");
        containerLeft.transform.localScale = new Vector3(-1f, 1f, 1f);
        containerLeft.transform.parent = this.gameObject.transform;
        containerRight.transform.parent = this.gameObject.transform;
        InitFruitBasket();

        // Both events are the authoritative hit/miss decisions from KeynoteHolder.
        // The fruit pool must follow them instead of its own countdown, otherwise a
        // valid grab can arrive after the countdown expired (or be rejected because
        // the countdown is still slightly positive) and the fruit plays its hit
        // animation even though it was grabbed.
        GameManager.OnKeynotePressedSuccessfully += ReleaseGrabbedFruit;
        GameManager.OnKeynoteNotPressed += PlayMissedFruitAnimation;
    }

    void Destroy()
    {
        GameManager.OnKeynotePressedSuccessfully -= ReleaseGrabbedFruit;
        GameManager.OnKeynoteNotPressed -= PlayMissedFruitAnimation;
    }

    private void InitFruitBasket()
    {
        for (int i = 0; i < 2; i++)
        {
            CreateFruit(StairsSide.Left, FruitType.Orange);
            CreateFruit(StairsSide.Right, FruitType.Orange);
        }
        for (int i = 0; i < 2; i++)
        {
            CreateFruit(StairsSide.Left, FruitType.PineApple);
            CreateFruit(StairsSide.Right, FruitType.PineApple);
        }
    }

    private Fruit CreateFruit(StairsSide side, FruitType type)
    {
        Transform container = side == StairsSide.Left ? containerLeft.transform : containerRight.transform;
        GameObject prefab = type == FruitType.Orange ? orangePrefab : pineapplePrefab;
        GameObject fruitObject = Instantiate(prefab, container);
        fruitObject.name = type == FruitType.Orange
            ? (side == StairsSide.Left ? "OrangeLeft" : "OrangeRight")
            : (side == StairsSide.Left ? "PineAppleLeft" : "PineAppleRight");
        fruitObject.SetActive(false);

        Fruit fruit = new Fruit(fruitObject, side, type, 0f, 0f, false);
        fruitBasket.Add(fruit);
        return fruit;
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
        Fruit fruit = GetFruitFromPool(side, FruitType.Orange);
        float orangeLifeSpawn = 4 * Conductor.instance.secPerBeat;
        fruit.PlaySyncedAnimation(Fruit.ORANGE_LOOP, orangeLifeSpawn);
    }

    public void SpawnPineapple(StairsSide side)
    {
        Fruit fruit = GetFruitFromPool(side, FruitType.PineApple);
        float pineLifeSpawn = 7 * Conductor.instance.secPerBeat;
        fruit.PlaySyncedAnimation(Fruit.PINEAPPLE_LOOP, pineLifeSpawn);
    }

    void Update()
    {
        // Only fruits that are still awaiting their note need the synchronized loop
        // animation. Once a miss is confirmed the hit animation plays on its own.
        foreach (Fruit fruit in fruitBasket)
        {
            if (fruit.isActive && fruit.isSynced)
            {
                fruit.Update();
            }
        }
    }

    private IEnumerator StartReleaseCountDown(Fruit fruit, float time)
    {
        yield return new WaitForSeconds(time);
        fruit.Release();
    }

    /// <summary>
    /// The player grabbed the note: hide the fruit that belongs to it immediately.
    /// </summary>
    void ReleaseGrabbedFruit(StairsSide side, FruitType type)
    {
        Fruit fruit = GetPendingFruit(side, type);
        if (fruit != null)
        {
            fruit.Release();
        }
    }

    /// <summary>
    /// The note was not grabbed: play the hit animation, then return the fruit to the pool.
    /// </summary>
    void PlayMissedFruitAnimation(StairsSide side, FruitType type)
    {
        Fruit fruit = GetPendingFruit(side, type);
        if (fruit == null)
        {
            return;
        }

        fruit.PlayLastAnimation();
        StartCoroutine(StartReleaseCountDown(fruit, 0.7f));
    }

    /// <summary>
    /// Returns the fruit that is waiting for the note currently being judged. Notes are
    /// consumed in order, so the pending fruit closest to (or past) its hit time is the
    /// one the event refers to.
    /// </summary>
    Fruit GetPendingFruit(StairsSide side, FruitType type)
    {
        Fruit pending = null;
        foreach (Fruit fruit in fruitBasket)
        {
            if (!fruit.isActive || !fruit.isSynced || fruit.side != side || fruit.type != type)
            {
                continue;
            }

            if (pending == null || fruit.lifeSpawn < pending.lifeSpawn)
            {
                pending = fruit;
            }
        }
        return pending;
    }

    Fruit GetFruitFromPool(StairsSide side, FruitType type)
    {
        foreach (Fruit fruit in fruitBasket)
        {
            if (!fruit.isActive && fruit.side == side && fruit.type == type)
            {
                return fruit;
            }
        }

        // Missing notes keep fruits busy longer than the initial pool covers. Grow the
        // pool on demand instead of failing to spawn (or reusing a fruit mid-animation).
        return CreateFruit(side, type);
    }
}
