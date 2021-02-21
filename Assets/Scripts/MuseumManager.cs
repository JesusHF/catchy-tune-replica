using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MuseumManager : MonoBehaviour
{
    public Image blackSquareImage;
    public RectTransform patternRect;
    public Level[] levels;
    public Sprite silverFrame;
    public Sprite goldenFrame;

    [Header("Selector")]
    public RectTransform selector;
    public TextMeshProUGUI selectorText;
    public float selectorXOffset;
    public float selectorYOffset;
    public float selectorYRange;
    private int levelSelected;
    private float selectorAnimationTime;
    private float selectorAnimationDirection;
    private float patternAnimationTime;
    private bool lockedSelector;

    void Start()
    {
        lockedSelector = true;
        patternAnimationTime = 0f;
        selector.gameObject.SetActive(false);
        AudioManager.instance.PlaySong("museum", true, 0.3f);
        ShowLevels(false);
        StartCoroutine(FadeBlackSquare(1f, 0f, InitMuseum));
    }

    void InitMuseum()
    {
        ShowLevels(true);
        selector.gameObject.SetActive(true);
        levelSelected = 0;
        lockedSelector = false;
        selectorAnimationTime = 0f;
        selectorAnimationDirection = 1f;
        UpdateSelectorText();
    }

    private void ShowLevels(bool show)
    {
        foreach (Level level in levels)
        {
            level.gameObject.SetActive(show);
            if (show == true)
            {
                int highscore = PlayerPrefs.GetInt(level.level, 0);
                level.scoreText.text = highscore.ToString();
                if (highscore >= 80)
                {
                    level.scoreText.color = new Color(1f, 0.8228265f, 0f);
                    level.frameImage.sprite = goldenFrame;
                }
            }
        }
    }

    private void UpdateSelectorText()
    {
        selectorText.text = levels[levelSelected].name;
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
        float xPosition = levels[levelSelected].rect.position.x + selectorXOffset;
        float initialYPosition = levels[levelSelected].rect.position.y + selectorYOffset;
        float offsetYPosition = initialYPosition + selectorYRange * selectorAnimationTime;
        selector.position = new Vector3(xPosition, offsetYPosition);
    }

    private void AnimatePattern()
    {
        patternAnimationTime += Time.deltaTime;
        if (patternAnimationTime >= 3f)
        {
            patternAnimationTime = 0f;
        }
        patternRect.anchoredPosition = new Vector2(-100f * patternAnimationTime, -100f * patternAnimationTime);
    }
    void Update()
    {
        if (!lockedSelector)
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
        }
        AnimateSelector();
        AnimatePattern();
    }

    private void ChangeLevelSelected(int direction)
    {
        if (levelSelected + direction >= 0 && levelSelected + direction < levels.Length)
        {
            AudioManager.instance.PlaySfx("ui_click");
            levelSelected += direction;
            UpdateSelectorText();
        }
    }

    private void PlaySelectedLevel()
    {
        lockedSelector = true;
        LoadLevel(levels[levelSelected].level);
    }

    private void LoadLevel(string sceneName)
    {
        AudioManager.instance.PlaySfx("ui_click2");
        AudioManager.instance.FadeCurrentSong(2f);
        StartCoroutine(FadeBlackSquare(2f, 1f, () =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }));
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
