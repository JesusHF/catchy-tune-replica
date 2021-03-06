﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

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
    public string animationName;
}

public class EndGameManager : MonoBehaviour
{
    public GameObject PanelContainer;
    public GameObject ScoreContainer;
    public Image blackSquareImage;
    public TextMeshProUGUI commentsFirstText;
    public TextMeshProUGUI commentsSecondText;
    public Slider slider60;
    public Slider slider80;
    public Slider slider100;
    public TextMeshProUGUI sliderNumberText;
    public Animator resultAnimator;
    public GameObject highscoreObject;
    public GameObject spaceIconObject;
    public GameObject backgroundObject;
    public EndGameResult tryAgainResult;
    public EndGameResult okResult;
    public EndGameResult superbResult;
    private bool isOver;

    void Start()
    {
        gameObject.SetActive(false);
        isOver = false;
    }

    public void StartEndGame(int score = 75)
    {
        StartCoroutine(ShowEndGame(score));
    }

    private IEnumerator ShowEndGame(int score)
    {
        PanelContainer.SetActive(false);
        ScoreContainer.SetActive(false);
        resultAnimator.gameObject.SetActive(false);
        highscoreObject.SetActive(false);
        spaceIconObject.SetActive(false);
        backgroundObject.SetActive(false);
        yield return new WaitForSeconds(2f);
        PanelContainer.SetActive(true);
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
            highscoreObject.SetActive(true);
            AudioManager.instance.PlaySfx("ui_highscore");
        }
        else
        {
            AudioManager.instance.PlaySfx("ui_score");
        }
        yield return new WaitForSeconds(1f);
        AudioManager.instance.PlaySfx(currentEnd.sfxName);
        resultAnimator.gameObject.SetActive(true);
        resultAnimator.Play(currentEnd.animationName);
        backgroundObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        AudioManager.instance.PlaySong(currentEnd.songName, true);
        spaceIconObject.SetActive(true);
        isOver = true;
    }

    private bool ShowHighScore(int score)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        int highscore = PlayerPrefs.GetInt(sceneName, 0);
        if (score >= 60 && score > highscore)
        {
            PlayerPrefs.SetInt(sceneName, score);
            return true;
        }
        return false;
    }

    private void Update()
    {
        if (isOver && Input.GetKeyDown(KeyCode.Space))
        {
            isOver = false;
            AudioManager.instance.PlaySfx("ui_click2");
            AudioManager.instance.FadeCurrentSong(2f);
            StartCoroutine(GameManager.FadeBlackSquare(blackSquareImage, 2f, 1f, () =>
            {
                SceneManager.LoadScene(0);
            }));
        }
    }
}
