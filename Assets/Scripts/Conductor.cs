using UnityEngine;

public class Conductor : MonoBehaviour
{
    private float songBpm;
    public float secPerBeat { get; private set; }
    public float songPosition { get; private set; }
    public float songPositionMs { get; private set; }
    public float songPositionInBeats { get; private set; }
    public float dspSongTime { get; private set; }

    private float beatsPerLoop;
    public int completedLoops { get; private set; }
    public float loopPositionInBeats { get; private set; }
    public float loopPositionInAnalog { get; private set; }
    private bool paused;


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

    public void StartSong(SongData songData)
    {
        StopSong();
        paused = false;
        songBpm = songData.bpm;
        secPerBeat = 60f / songBpm;
        dspSongTime = (float)AudioSettings.dspTime - secPerBeat;
        beatsPerLoop = songData.beats_per_loop;

        AudioManager.instance.PlaySong(songData.song_clip, songData.loop);
    }

    public void StopSong()
    {
        paused = true;
        dspSongTime = 0f;
        songPosition = 0f;
        songPositionMs = 0f;
        songPositionInBeats = 0f;
        completedLoops = 0;
        loopPositionInBeats = 0f;
        loopPositionInAnalog = 0f;
    }

    void Update()
    {
        if (!paused)
        {
            songPosition = (float)(AudioSettings.dspTime - dspSongTime);
            songPositionMs = songPosition * 1000f;
            songPositionInBeats = songPosition / secPerBeat;

            if (songPositionInBeats >= (completedLoops + 1) * beatsPerLoop)
                completedLoops++;
            loopPositionInBeats = songPositionInBeats - completedLoops * beatsPerLoop;
            loopPositionInAnalog = loopPositionInBeats / beatsPerLoop;
        }
    }

    public float GetTimeToNextBeat()
    {
        int nextBeat = Mathf.CeilToInt(songPositionInBeats);
        float timeToNextBeat = (nextBeat - songPositionInBeats);
        timeToNextBeat = timeToNextBeat / songBpm * 60f;
        return timeToNextBeat;
    }
}
