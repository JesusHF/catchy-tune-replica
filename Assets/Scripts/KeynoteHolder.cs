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

    [SerializeField, Range(25f, 300f)]
    private float threshold = 200f;
    private Queue<Keynote> keynoteTimesL = new Queue<Keynote>();
    private Queue<Keynote> keynoteTimesR = new Queue<Keynote>();
    private Queue<Keynote> fruitsToSpawn = new Queue<Keynote>();
    private Queue<QueuedSfx> sfxToPlay = new Queue<QueuedSfx>();

    private void Update()
    {
        CheckNotesToSpawn();
        CheckPassedNotes(StairsSide.Left);
        CheckPassedNotes(StairsSide.Right);
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

    private void CheckPassedNotes(StairsSide side)
    {
        ref Queue<Keynote> currentSideNotes = ref GetNotesInSide(side);
        if (currentSideNotes.Count > 0)
        {
            float nextKeynoteMS = currentSideNotes.Peek().beat * Conductor.instance.secPerBeat * 1000f;
            if (Conductor.instance.songPositionMs > (nextKeynoteMS + (threshold / 2)))
            {
                Keynote note = currentSideNotes.Dequeue();
                GameManager.instance.NotifyNotePassedInSide(side, GetFruitType(note.instrument));
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

    private void AddNoteToSide(StairsSide side, Keynote note)
    {
        if (side == StairsSide.Left)
        {
            keynoteTimesL.Enqueue(note);
        }
        else
        {
            keynoteTimesR.Enqueue(note);
        }
    }

    private ref Queue<Keynote> GetNotesInSide(StairsSide side)
    {
        return ref side == StairsSide.Left ? ref keynoteTimesL : ref keynoteTimesR;
    }

    public void QueueNoteInBeats(float beatsFromNow, Instrument instrument)
    {
        float currentBeat = (float)Mathf.Round(Conductor.instance.songPositionInBeats * 10f) / 10f;
        float beat = currentBeat + beatsFromNow;
        AddNoteToSide(GetSide(instrument), new Keynote(beat, instrument));
    }

    public void QueueSoundEffectInBeats(float beatsFromNow, string sfxName, float volume)
    {
        float currentBeat = (float)Mathf.Round(Conductor.instance.songPositionInBeats * 10f) / 10f;
        float beat = currentBeat + beatsFromNow;
        sfxToPlay.Enqueue(new QueuedSfx(beat, sfxName, volume));
    }

    public FruitType CheckCurrentBeatHasAnyFruitInSide(StairsSide side)
    {
        ref Queue<Keynote> currentSideNotes = ref GetNotesInSide(side);
        if (currentSideNotes.Count > 0)
        {
            float nextKeynoteMS = currentSideNotes.Peek().beat * Conductor.instance.secPerBeat * 1000f;
            if (Mathf.Abs(Conductor.instance.songPositionMs - nextKeynoteMS) < threshold)
            {
                Instrument note = currentSideNotes.Dequeue().instrument;
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
                AddNoteToSide(GetSide(note.instrument), new Keynote(note.beat + 4f, note.instrument));
            }
            else
            {
                sfxList.Add(new QueuedSfx(note.beat + 1f, "bounce2", 1f));
                sfxList.Add(new QueuedSfx(note.beat + 3f, "bounce2", 1f));
                sfxList.Add(new QueuedSfx(note.beat + 5f, "bounce2", 1f));
                AddNoteToSide(GetSide(note.instrument), new Keynote(note.beat + 7f, note.instrument));
            }
        }
        List<QueuedSfx> sortedList = sfxList.OrderBy(o => o.beat).ToList();
        sfxToPlay = new Queue<QueuedSfx>(sortedList);
    }
}
