using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화면 전환 시 Fade 효과를 제공하는 특수 UI
/// DontDestroyOnLoad로 관리되며 모든 씬에서 재사용됨
/// </summary>
public class UI_FadePanel : UI_UGUI
{
    enum Images
    {
        FadeImage
    }

    private Image _fadeImage;
    private Coroutine _fadeCoroutine;

    protected override void Awake()
    {
        base.Awake();

        BindImages(typeof(Images));
        _fadeImage = GetImage((int)Images.FadeImage);

        // 검은색 전체 화면, 처음엔 투명
        _fadeImage.color = new Color(0, 0, 0, 0);
        _fadeImage.raycastTarget = true; // 페이드 중 클릭 차단
    }

    /// <summary>
    /// 화면을 검게 덮음 (Fade In)
    /// </summary>
    public void FadeIn(float duration, Action onComplete = null)
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        gameObject.SetActive(true);
        _fadeCoroutine = StartCoroutine(CoFade(0f, 1f, duration, onComplete));
    }

    /// <summary>
    /// 검은 화면을 제거 (Fade Out)
    /// </summary>
    public void FadeOut(float duration, Action onComplete = null)
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(CoFade(1f, 0f, duration, () =>
        {
            onComplete?.Invoke();
            gameObject.SetActive(false); // Fade Out 완료 후 비활성화
        }));
    }

    /// <summary>
    /// 즉시 검은 화면으로 설정
    /// </summary>
    public void SetBlack()
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        gameObject.SetActive(true);
        _fadeImage.color = new Color(0, 0, 0, 1);
    }

    /// <summary>
    /// 즉시 투명하게 설정
    /// </summary>
    public void SetTransparent()
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeImage.color = new Color(0, 0, 0, 0);
        gameObject.SetActive(false);
    }

    private IEnumerator CoFade(float startAlpha, float endAlpha, float duration, Action onComplete)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            _fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        _fadeImage.color = new Color(0, 0, 0, endAlpha);
        onComplete?.Invoke();
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
    }
}
