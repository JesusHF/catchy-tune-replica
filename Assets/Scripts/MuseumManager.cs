using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MuseumManager : MonoBehaviour
{
    public Image blackSquareImage;

    void Start()
    {
        StartCoroutine(FadeBlackSquare(2f, 0f, () =>
        {
            AudioManager.instance.PlaySong("museum", true);
        }));
    }

    void Update()
    {

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
