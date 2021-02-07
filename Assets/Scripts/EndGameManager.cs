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
        yield return new WaitForSeconds(1f);
        string firstComment = "";
        string secondComent = "";
        if (score < 60)
        {
            firstComment = tryAgainResult.commentFirstLine;
            secondComent = tryAgainResult.commentSecondLine;
        }
        else if (score < 80)
        {
            firstComment = okResult.commentFirstLine;
            secondComent = okResult.commentSecondLine;
        }
        else
        {
            firstComment = superbResult.commentFirstLine;
            secondComent = superbResult.commentSecondLine;
        }
        commentsFirstText.text = firstComment;
        if (secondComent != "")
        {
            yield return new WaitForSeconds(1f);
            commentsSecondText.text = secondComent;
        }

        yield return new WaitForSeconds(1f);
        ScoreContainer.SetActive(true);
        slider60.value = 0;
        slider80.gameObject.SetActive(false);
        slider100.gameObject.SetActive(false);
        sliderNumberText.text = "0";
        sliderNumberText.color = tryAgainResult.color;
        yield return new WaitForSeconds(0.5f);
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
        sliderNumberText.text = score.ToString();
        // todo: show highscore
        // todo: show try again/ok/superb image
    }
}
