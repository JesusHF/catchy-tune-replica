using System;
using System.Collections.Generic;
using UnityEngine;

public class KeynoteHolder : MonoBehaviour
{
    [SerializeField, Range(100f, 300f)]
    private float threshold = 200f;
    private Queue<float> keynoteTimes;

    void Start()
    {
        keynoteTimes = new Queue<float>();
    }

    private void Update()
    {
        CheckPassedNotes();
    }

    public void CreateKeynote(float time)
    {
        time *= 1000f;
        keynoteTimes.Enqueue(time);
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

}
