using UnityEngine;

[System.Serializable]
public struct Keynote
{
    public int instrument;
    public float beat;
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
