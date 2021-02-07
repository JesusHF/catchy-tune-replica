using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public struct EndGameResult
{
    [TextArea]
    public string commentFirstLine;
    [TextArea]
    public string commentSecondLine;
    public Color color;
    public string sfxName;
    public string songName;
    public Image image;
}

public class EndGameManager : MonoBehaviour
{
    public GameObject EndGameContainer;
    public GameObject ScoreContainer;
    public TextMeshProUGUI commentsFirstText;
    public TextMeshProUGUI commentsSecondText;
    public Slider slider60;
    public Slider slider80;
    public Slider slider100;
    public TextMeshProUGUI sliderNumberText;
    public EndGameResult tryAgainResult;
    public EndGameResult okResult;
    public EndGameResult superbResult;

    void Start()
    {
        EndGameContainer.SetActive(false);
    }

    public void StartEndGame(int score = 75)
    {
        StartCoroutine(ShowEndGame(score));
    }

    private IEnumerator ShowEndGame(int score)
    {
        yield return new WaitForSeconds(2f);
        EndGameContainer.SetActive(true);
        ScoreContainer.SetActive(false);
        commentsFirstText.text = "";
        commentsSecondText.text = "";
        AudioManager.instance.PlaySfx("ui_show1");
        yield return new WaitForSeconds(1f);
        EndGameResult currentEnd;
        if (score < 60)
        {
            currentEnd = tryAgainResult;
        }
        else if (score < 80)
        {
            currentEnd = okResult;
        }
        else
        {
            currentEnd = superbResult;
        }
        commentsFirstText.text = currentEnd.commentFirstLine;
        if (currentEnd.commentSecondLine != "")
        {
            AudioManager.instance.PlaySfx("ui_show2");
            yield return new WaitForSeconds(1f);
            commentsSecondText.text = currentEnd.commentSecondLine;
            AudioManager.instance.PlaySfx("ui_show3");
        }
        else
        {
            AudioManager.instance.PlaySfx("ui_show3");
        }

        yield return new WaitForSeconds(1f);
        ScoreContainer.SetActive(true);
        slider60.value = 0;
        slider80.gameObject.SetActive(false);
        slider100.gameObject.SetActive(false);
        sliderNumberText.text = "0";
        sliderNumberText.color = tryAgainResult.color;
        yield return new WaitForSeconds(0.5f);
        AudioManager.instance.PlaySfx("ui_slider");
        float initialTime = 2.3f * score / 100f;
        float timeRemaining = initialTime;
        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            float analogTime = (initialTime - timeRemaining) / initialTime;
            int scoreOverTime = (int)Mathf.Lerp(0f, score, analogTime);
            sliderNumberText.text = scoreOverTime.ToString();
            if (scoreOverTime < 60)
            {
                slider60.value = scoreOverTime;
            }
            else if (scoreOverTime < 80)
            {
                if (score > 61 && !slider80.gameObject.activeInHierarchy)
                {
                    slider80.gameObject.SetActive(true);
                }
                slider80.value = scoreOverTime;
                sliderNumberText.color = okResult.color;
            }
            else
            {
                if (!slider100.gameObject.activeInHierarchy)
                {
                    slider100.gameObject.SetActive(true);
                }
                slider100.value = scoreOverTime;
                sliderNumberText.color = superbResult.color;
            }
            yield return null;
        }
        AudioManager.instance.StopCurrentSfx();
        sliderNumberText.text = score.ToString();
        if (ShowHighScore(score))
        {
            // todo: show highscore
            AudioManager.instance.PlaySfx("ui_highscore");
        }
        else
        {
            AudioManager.instance.PlaySfx("ui_score");
        }
        yield return new WaitForSeconds(1f);
        AudioManager.instance.PlaySfx(currentEnd.sfxName);
        yield return new WaitForSeconds(1.5f);
        // todo: add songs
        // AudioManager.instance.PlaySong(currentEnd.songName, true);
        // todo: show current end image
    }

    private bool ShowHighScore(int score)
    {
        // todo: handle saving high scores
        return false;
    }
}
