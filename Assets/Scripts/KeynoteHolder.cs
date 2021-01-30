using System;
using System.Collections.Generic;
using UnityEngine;

public class KeynoteHolder : MonoBehaviour
{
    [SerializeField, Range(100f, 300f)]
    private float threshold = 200f;
    private Queue<float> keynoteTimes = new Queue<float>();
    private Queue<float> keynotesToSpawn = new Queue<float>();

    private void Update()
    {
        CheckNotesToSpawn();
        CheckPassedNotes();
    }

    public void QueueNoteInBeat(float beatsFromNow)
    {
        float time = Conductor.instance.songPosition + (beatsFromNow * Conductor.instance.secPerBeat);
        time *= 1000f;
        keynoteTimes.Enqueue(time);
    }

    private void CheckNotesToSpawn()
    {
        if (keynotesToSpawn.Count > 0)
        {
            if (Conductor.instance.songPositionMs >= keynotesToSpawn.Peek())
            {
                keynotesToSpawn.Dequeue();
                GameManager.instance.CreateKeynoteNow();
            }
        }
    }

    private void CheckPassedNotes()
    {
        if (keynoteTimes.Count > 0)
        {
            if (Conductor.instance.songPositionMs > (keynoteTimes.Peek() + (threshold/2)))
            {
                keynoteTimes.Dequeue();
                GameManager.instance.NotifyNotePassed();
            }
        }
    }

    public bool CheckCurrentBeatHasAnyNote()
    {
        if (keynoteTimes.Count > 0)
        {
            if (Mathf.Abs(Conductor.instance.songPositionMs - keynoteTimes.Peek()) < threshold)
            {
                keynoteTimes.Dequeue();
                return true;
            }
        }
        return false;
    }

    public void PreprocessSongNotes(Keynote[] notes)
    {
        foreach (var note in notes)
        {
            float msTime = note.beat * Conductor.instance.secPerBeat * 1000f;
            int instrument = note.instrument;

            if (instrument == 1)
            {
                keynotesToSpawn.Enqueue(msTime);
            }
        }
    }

}
