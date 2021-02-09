﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public struct GridElement
{
    public RectTransform rect;
    public string name;
}

public class MuseumManager : MonoBehaviour
{
    public Image blackSquareImage;
    public GridElement[] gridElements;
    [Header("Selector")]
    public RectTransform selector;
    public TextMeshProUGUI selectorText;
    public float selectorXOffset;
    public float selectorYOffset;
    public float selectorYRange;
    private int levelSelected;
    private float selectorAnimationTime;
    private float selectorAnimationDirection;

    void Start()
    {
        selector.gameObject.SetActive(false);
        AudioManager.instance.PlaySong("museum", true);
        StartCoroutine(FadeBlackSquare(1f, 0f, InitMuseum));
    }

    void InitMuseum()
    {
        selector.gameObject.SetActive(true);
        levelSelected = 0;
        selectorAnimationTime = 0f;
        selectorAnimationDirection = 1f;
        UpdateSelectorText();
    }
    private void UpdateSelectorText()
    {
        selectorText.text = gridElements[levelSelected].name;
    }

    private void AnimateSelector()
    {
        if (selectorAnimationTime > 0.5f)
        {
            selectorAnimationDirection = -1f;
        }
        else if (selectorAnimationTime < -0.5f)
        {
            selectorAnimationDirection = +1f;
        }
        selectorAnimationTime += Time.deltaTime * selectorAnimationDirection;
        float xPosition = gridElements[levelSelected].rect.position.x + selectorXOffset;
        float initialYPosition = gridElements[levelSelected].rect.position.y + selectorYOffset;
        float offsetYPosition = initialYPosition + selectorYRange * selectorAnimationTime;
        selector.position = new Vector3(xPosition, offsetYPosition);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeLevelSelected(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeLevelSelected(+1);
        }
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            PlaySelectedLevel();
        }
        AnimateSelector();
    }

    private void ChangeLevelSelected(int direction)
    {
        if (levelSelected + direction >= 0 && levelSelected + direction < gridElements.Length)
        {
            levelSelected += direction;
            UpdateSelectorText();
        }
    }

    private void PlaySelectedLevel()
    {
        AudioManager.instance.FadeCurrentSong(2f);
        switch (levelSelected)
        {
            case 0:
                StartCoroutine(FadeBlackSquare(2f, 1f, () =>
                {
                    LoadLevel("level1");
                }));
                break;
            case 1:
                StartCoroutine(FadeBlackSquare(2f, 1f, () =>
                {
                    LoadLevel("level2");
                }));
                break;
            default:
                break;
        }
    }

    private void LoadLevel(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeBlackSquare(float duration, float finalAlpha = 0f, Action onFinished = null)
    {
        float currentTime = 0;
        float startAlpha = 1 - finalAlpha;
        Color c = blackSquareImage.color;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, finalAlpha, currentTime / duration);
            blackSquareImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        onFinished?.Invoke();
        yield break;
    }
}
