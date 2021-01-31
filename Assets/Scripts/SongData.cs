using UnityEngine;

public enum Instrument
{
    orangeL,
    orangeR,
    pineAppleL,
    pineAppleR
}

[System.Serializable]
public struct Keynote
{
    public float beat;
    public Instrument instrument;
    public Keynote(float beat, Instrument instrument)
    {
        this.beat = beat;
        this.instrument = instrument;
    }
}

[CreateAssetMenu(fileName = "New Song Data", menuName = "Songs/Song Data")]
public class SongData : ScriptableObject
{
    public int bpm;
    public int beats_per_loop;
    public string presong_clip;
    public string song_clip;
    public bool loop;
    public Keynote[] keynotes;
}
