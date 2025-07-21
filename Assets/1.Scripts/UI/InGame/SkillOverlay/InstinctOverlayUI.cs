using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.InGame.SkillOverlay
{
    public class InstinctOverlayUI : MonoBehaviour
    {
 [Header("UI Reference")]
    [Tooltip("이 RawImage의 Material에 스크롤/페이드를 적용합니다.")]
    public RawImage overlayImage;

    [Header("Scroll Settings")]
    [Tooltip("텍스처가 움직일 방향")]
    public Vector2 scrollDirection = new Vector2(1, 0);
    [Tooltip("기본 스크롤 속도")]
    public float scrollSpeed = 1f;

    [Header("Fade Settings")]
    [Tooltip("페이드 인/아웃 지속 시간 (초)")]
    public float fadeDuration = 0.2f;

    // Shader property IDs
    private static readonly int ScrollDirID     = Shader.PropertyToID("_ScrollDir");
    private static readonly int ScrollSpeedID   = Shader.PropertyToID("_ScrollSpeed");
    private static readonly int OverlayAlphaID  = Shader.PropertyToID("_OverlayAlpha");

    private Material runtimeMat;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        // 1) overlayImage 세팅
        if (overlayImage == null)
            overlayImage = GetComponent<RawImage>();

        // 2) 인스펙터용 Material 인스턴스화 (공유 머티리얼 건드리지 않도록)
        runtimeMat = Instantiate(overlayImage.material);
        overlayImage.material = runtimeMat;

        // 3) 초기값 입력
        runtimeMat.SetVector(ScrollDirID, scrollDirection);
        runtimeMat.SetFloat(ScrollSpeedID, scrollSpeed);
        runtimeMat.SetFloat(OverlayAlphaID, 0f); // 시작 시엔 투명
    }

    void Update()
    {
        // 만약 이동 속도와 연동하고 싶으면 이 부분에서 scrollSpeed을 덮어쓰기
        runtimeMat.SetFloat(ScrollSpeedID, scrollSpeed);
        runtimeMat.SetVector(ScrollDirID, scrollDirection);
    }

    /// <summary>
    /// 오버레이를 페이드 인 합니다.
    /// </summary>
    public void ShowOverlay()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAlpha(1f));
    }

    /// <summary>
    /// 오버레이를 페이드 아웃 합니다.
    /// </summary>
    public void HideOverlay()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAlpha(0f));
    }

    private IEnumerator FadeAlpha(float target)
    {
        float start = runtimeMat.GetFloat(OverlayAlphaID);
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float a = Mathf.Lerp(start, target, time / fadeDuration);
            runtimeMat.SetFloat(OverlayAlphaID, a);
            yield return null;
        }
        runtimeMat.SetFloat(OverlayAlphaID, target);
    }
    }
}