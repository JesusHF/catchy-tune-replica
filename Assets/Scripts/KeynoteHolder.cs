using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public struct QueuedSfx
{
    public float beat;
    public string clipName;
    public float volume;
    public QueuedSfx(float beat, string clipName, float volume)
    {
        this.beat = beat;
        this.clipName = clipName;
        this.volume = volume;
    }
}

public class KeynoteHolder : MonoBehaviour
{
    [SerializeField, Range(100f, 300f)]
    private float threshold = 200f;
    private Queue<Keynote> keynoteTimes = new Queue<Keynote>();
    private Queue<Keynote> fruitsToSpawn = new Queue<Keynote>();
    private Queue<QueuedSfx> sfxToPlay = new Queue<QueuedSfx>();

    private void Update()
    {
        CheckNotesToSpawn();
        CheckPassedNotes();
        CheckToPlaySoundEffects();
    }

    private void CheckNotesToSpawn()
    {
        if (fruitsToSpawn.Count > 0)
        {
            float nextKeynoteMS = fruitsToSpawn.Peek().beat * Conductor.instance.secPerBeat * 1000f;
            if (Conductor.instance.songPositionMs >= nextKeynoteMS)
            {
                Keynote fruit = fruitsToSpawn.Dequeue();
                if (fruitsToSpawn.Count > 0 && fruitsToSpawn.Peek().beat == fruit.beat)
                {
                    Keynote doubleFruit = fruitsToSpawn.Dequeue();
                    GameManager.instance.CreateTwoFruitsNow(fruit.instrument, doubleFruit.instrument);
                }
                else
                {
                    GameManager.instance.CreateFruitNow(fruit.instrument);
                }
            }
        }
    }

    private void CheckPassedNotes()
    {
        if (keynoteTimes.Count > 0)
        {
            float nextKeynoteMS = keynoteTimes.Peek().beat * Conductor.instance.secPerBeat * 1000f;
            if (Conductor.instance.songPositionMs > (nextKeynoteMS + (threshold / 2)))
            {
                Keynote note = keynoteTimes.Dequeue();
                GameManager.instance.NotifyNotePassedInSide(GetSide(note.instrument), GetFruitType(note.instrument));
            }
        }
    }

    private void CheckToPlaySoundEffects()
    {
        if (sfxToPlay.Count > 0)
        {
            float nextBounceBeat = sfxToPlay.Peek().beat;
            if (Conductor.instance.songPositionInBeats >= nextBounceBeat)
            {
                QueuedSfx bounce = sfxToPlay.Dequeue();
                if (sfxToPlay.Count > 0 && sfxToPlay.Peek().beat == bounce.beat)
                {
                    QueuedSfx nextBounce = sfxToPlay.Dequeue();
                    if (nextBounce.clipName == bounce.clipName)
                    {
                        AudioManager.instance.PlaySfx(bounce.clipName, bounce.volume * 1.3f);
                    }
                    else
                    {
                        AudioManager.instance.PlaySfx(bounce.clipName, bounce.volume);
                        AudioManager.instance.PlaySfx(nextBounce.clipName, nextBounce.volume);
                    }
                }
                else
                {
                    AudioManager.instance.PlaySfx(bounce.clipName, bounce.volume);
                }
            }
        }
    }

    private StairsSide GetSide(Instrument instrument)
    {
        if (instrument == Instrument.orangeL || instrument == Instrument.pineAppleL)
        {
            return StairsSide.Left;
        }
        else
        {
            return StairsSide.Right;
        }
    }

    private FruitType GetFruitType(Instrument instrument)
    {
        if (instrument == Instrument.orangeL || instrument == Instrument.orangeR)
        {
            return FruitType.Orange;
        }
        else
        {
            return FruitType.PineApple;
        }
    }

    public void QueueNoteInBeats(float beatsFromNow, Instrument instrument)
    {
        float currentBeat = (float)Mathf.Round(Conductor.instance.songPositionInBeats * 10f) / 10f;
        float beat = currentBeat + beatsFromNow;
        keynoteTimes.Enqueue(new Keynote(beat, instrument));
    }

    public void QueueSoundEffectInBeats(float beatsFromNow, string sfxName, float volume)
    {
        float currentBeat = (float)Mathf.Round(Conductor.instance.songPositionInBeats * 10f) / 10f;
        float beat = currentBeat + beatsFromNow;
        sfxToPlay.Enqueue(new QueuedSfx(beat, sfxName, volume));
    }

    public FruitType CheckCurrentBeatHasAnyFruitInSide(StairsSide side)
    {
        if (keynoteTimes.Count > 0)
        {
            float nextKeynoteMS = keynoteTimes.Peek().beat * Conductor.instance.secPerBeat * 1000f;
            if (GetSide(keynoteTimes.Peek().instrument) == side &&
                Mathf.Abs(Conductor.instance.songPositionMs - nextKeynoteMS) < threshold)
            {
                Instrument note = keynoteTimes.Dequeue().instrument;
                FruitType type = note == Instrument.orangeL || note == Instrument.orangeR ?
                    FruitType.Orange : FruitType.PineApple;
                return type;
            }
        }
        return FruitType.None;
    }

    public void PreprocessSongNotes(Keynote[] notes)
    {
        List<QueuedSfx> sfxList = new List<QueuedSfx>();
        foreach (Keynote note in notes)
        {
            fruitsToSpawn.Enqueue(note);

            if (note.instrument == Instrument.orangeL || note.instrument == Instrument.orangeR)
            {
                sfxList.Add(new QueuedSfx(note.beat + 1f, "bounce", 1f));
                sfxList.Add(new QueuedSfx(note.beat + 2f, "bounce", 1f));
                sfxList.Add(new QueuedSfx(note.beat + 3f, "bounce", 1f));
                keynoteTimes.Enqueue(new Keynote(note.beat + 4f, note.instrument));
            }
            else
            {
                // todo: change provisional bounce sfx
                sfxList.Add(new QueuedSfx(note.beat + 1f, "bounce", 1f));
                sfxList.Add(new QueuedSfx(note.beat + 3f, "bounce", 1f));
                sfxList.Add(new QueuedSfx(note.beat + 5f, "bounce", 1f));
                keynoteTimes.Enqueue(new Keynote(note.beat + 7f, note.instrument));
            }
        }
        List<QueuedSfx> sortedList = sfxList.OrderBy(o=>o.beat).ToList();
        sfxToPlay = new Queue<QueuedSfx>(sortedList);
    }
}
