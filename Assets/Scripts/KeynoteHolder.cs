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
    private Queue<Keynote> keynotesToSpawn = new Queue<Keynote>();
    private Queue<QueuedSfx> sfxToPlay = new Queue<QueuedSfx>();

    private void Update()
    {
        CheckNotesToSpawn();
        CheckPassedNotes();
        CheckToPlaySoundEffects();
    }

    private void CheckNotesToSpawn()
    {
        if (keynotesToSpawn.Count > 0)
        {
            float nextKeynoteMS = keynotesToSpawn.Peek().beat * Conductor.instance.secPerBeat * 1000f;
            if (Conductor.instance.songPositionMs >= nextKeynoteMS)
            {
                Keynote note = keynotesToSpawn.Dequeue();
                if (keynotesToSpawn.Count > 0 && keynotesToSpawn.Peek().beat == note.beat)
                {
                    Keynote doubleNote = keynotesToSpawn.Dequeue();
                    GameManager.instance.CreateTwoKeynotesNow(note.instrument, doubleNote.instrument);
                }
                else
                {
                    GameManager.instance.CreateKeynoteNow(note.instrument);
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
                GameManager.instance.NotifyNotePassedInSide(GetSide(note.instrument));
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

    public void QueueNoteInBeat(float beatsFromNow, Instrument instrument)
    {
        float beat = Mathf.Floor(Conductor.instance.songPositionInBeats) + beatsFromNow;
        keynoteTimes.Enqueue(new Keynote(beat, instrument));
    }

    public bool CheckCurrentBeatHasAnyNoteInSide(StairsSide side)
    {
        if (keynoteTimes.Count > 0)
        {
            float nextKeynoteMS = keynoteTimes.Peek().beat * Conductor.instance.secPerBeat * 1000f;
            if (GetSide(keynoteTimes.Peek().instrument) == side &&
                Mathf.Abs(Conductor.instance.songPositionMs - nextKeynoteMS) < threshold)
            {
                keynoteTimes.Dequeue();
                return true;
            }
        }
        return false;
    }

    public void PreprocessSongNotes(Keynote[] notes)
    {
        foreach (Keynote note in notes)
        {
            keynotesToSpawn.Enqueue(note);
        }
    }

    public void ScheduleSoundEffect(float beat, string sfxName, float volume)
    {
        sfxToPlay.Enqueue(new QueuedSfx(beat, sfxName, volume));
    }
}
