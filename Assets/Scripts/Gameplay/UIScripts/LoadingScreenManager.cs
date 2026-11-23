using System.Collections;
using UnityEngine;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [Header("Loading Screen")]
    [SerializeField] private CanvasGroup loadingScreen;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private GameObject loadingLabel;

    [Header("'Start' Message")]
    [SerializeField] private GameObject startLabel;
    [SerializeField] private float startDisplayDuration = 1f;
    [SerializeField] private Timer timerPanel;

    private void Awake()
    {
        Instance = this;

        loadingScreen.alpha = 1f;
        loadingScreen.blocksRaycasts = true;
        loadingScreen.interactable = true;

        loadingLabel.SetActive(true);
        startLabel.SetActive(false);
    }

    public void StartFade(System.Action onFinished)
    {
        loadingLabel.SetActive(false);

        StartCoroutine(FadeOut(onFinished));
    }

    private IEnumerator FadeOut(System.Action onFinished)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            loadingScreen.alpha =
                1f - Mathf.Clamp01(elapsed / fadeDuration);

            yield return null;
        }

        loadingScreen.alpha = 0f;
        loadingScreen.blocksRaycasts = false;
        loadingScreen.interactable = false;

        onFinished?.Invoke();
    }

    public void ShowGoMessage()
    {
        StartCoroutine(ShowGoCoroutine());
        timerPanel.StartTimer();
    }

    private IEnumerator ShowGoCoroutine()
    {
        startLabel.SetActive(true);

        yield return new WaitForSecondsRealtime(startDisplayDuration);

        startLabel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}