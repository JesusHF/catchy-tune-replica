using UnityEngine;

public class Conductor : MonoBehaviour
{
    private float songBpm;
    public float secPerBeat { get; private set; }
    public float songPosition { get; private set; }
    public int songPositionMs { get; private set; }
    public float songPositionInBeats { get; private set; }
    public float dspSongTime { get; private set; }

    private float beatsPerLoop;
    public int completedLoops { get; private set; }
    public float loopPositionInBeats { get; private set; }
    public float loopPositionInAnalog { get; private set; }

    #region Singleton
    public static Conductor instance { get; private set; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    #endregion

    public void StartSong(/*SongInfo songInfo*/)
    {
        songBpm = 130;
        secPerBeat = 60f / songBpm;
        dspSongTime = (float)AudioSettings.dspTime;
        beatsPerLoop = 8;

        //AudioManager.Instance().PlaySong(songInfo.song_clip);
        AudioManager.instance.PlaySong("tutorial", true);
    }

    void Update()
    {
        songPosition = (float)(AudioSettings.dspTime - dspSongTime);
        songPositionMs = (int)(songPosition * 1000);
        songPositionInBeats = songPosition / secPerBeat;

        if (songPositionInBeats >= (completedLoops + 1) * beatsPerLoop)
            completedLoops++;
        loopPositionInBeats = songPositionInBeats - completedLoops * beatsPerLoop;
        loopPositionInAnalog = loopPositionInBeats / beatsPerLoop;
    }

    public float GetTimeToNextBeat()
    {
        int nextBeat = Mathf.CeilToInt(Conductor.instance.songPositionInBeats);
        float timeToNextBeat = (nextBeat - Conductor.instance.songPositionInBeats);
        timeToNextBeat = timeToNextBeat / songBpm * 60f;
        return timeToNextBeat;
    }
}
