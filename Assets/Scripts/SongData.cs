using UnityEngine;
using UnityEditor;

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
    [HideInInspector]
    public int finish_beat = 0;
    public Keynote[] keynotes;
}

[CustomEditor(typeof(SongData))]
public class SongEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SongData songDataScript = target as SongData;
        EditorGUI.BeginDisabledGroup(songDataScript.loop);
        songDataScript.finish_beat = EditorGUILayout.IntField("Finish_beat", songDataScript.finish_beat);
        EditorGUI.EndDisabledGroup();
    }
}