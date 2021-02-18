using UnityEditor;

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
