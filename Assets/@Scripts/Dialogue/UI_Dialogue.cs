using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UI_Dialogue : MonoBehaviour
{
    public Image Portrait; // 캐릭터 이미지, 에디터에서 할당/수정 가능
    public Button Blocker; // 투명 버튼
    public TMP_Text CharacterName; // 캐릭터 이름
    public TMP_Text DialogueText; // 대사 텍스트

    [Header("선택지 버튼")]
    public Button LeftButton;
    public Button RightButton;
    public TMP_Text LeftButtonText;
    public TMP_Text RightButtonText;

    [Header("타이핑 효과 간격(초)")]
    public float interval = 0.05f;

    private Coroutine typingCoroutine;
    private string fullText;
    private bool isTyping = false;
    private bool isChoiceDialogue = false;
    private Action onLeftChoice;
    private Action onRightChoice;

    void Awake()
    {
        Blocker.onClick.AddListener(HandleNext);
        if (LeftButton != null) LeftButton.onClick.AddListener(OnLeftButtonClicked);
        if (RightButton != null) RightButton.onClick.AddListener(OnRightButtonClicked);
        HideChoiceButtons();
    }

    public void SetDialogue(string characterName, string dialogue, Sprite portrait = null)
    {
        isChoiceDialogue = false;
        HideChoiceButtons();
        
        CharacterName.text = characterName;
        fullText = dialogue;
        Portrait.sprite = portrait;
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(CoTypeText(fullText));
    }

    public void SetChoiceDialogue(string characterName, string dialogue, Sprite portrait,
        string leftText, string rightText, Action onLeft, Action onRight)
    {
        isChoiceDialogue = true;
        onLeftChoice = onLeft;
        onRightChoice = onRight;
        
        CharacterName.text = characterName;
        fullText = dialogue;
        Portrait.sprite = portrait;
        
        // 선택지 버튼 텍스트 설정
        if (LeftButtonText != null) LeftButtonText.text = leftText;
        if (RightButtonText != null) RightButtonText.text = rightText;
        
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(CoTypeTextWithChoice(fullText));
    }

    private System.Collections.IEnumerator CoTypeText(string text)
    {
        isTyping = true;
        DialogueText.text = "";
        for (int i = 0; i < text.Length; i++)
        {
            DialogueText.text += text[i];
            yield return new WaitForSeconds(interval);
        }
        DialogueText.text = text;
        isTyping = false;
    }

    private System.Collections.IEnumerator CoTypeTextWithChoice(string text)
    {
        isTyping = true;
        HideChoiceButtons();
        DialogueText.text = "";
        for (int i = 0; i < text.Length; i++)
        {
            DialogueText.text += text[i];
            yield return new WaitForSeconds(interval);
        }
        DialogueText.text = text;
        isTyping = false;
        
        // 타이핑 완료 후 선택지 버튼 표시
        ShowChoiceButtons();
    }

    private void ShowChoiceButtons()
    {
        if (LeftButton != null) LeftButton.gameObject.SetActive(true);
        if (RightButton != null) RightButton.gameObject.SetActive(true);
    }

    private void HideChoiceButtons()
    {
        if (LeftButton != null) LeftButton.gameObject.SetActive(false);
        if (RightButton != null) RightButton.gameObject.SetActive(false);
    }

    private void OnLeftButtonClicked()
    {
        HideChoiceButtons();
        onLeftChoice?.Invoke();
        onLeftChoice = null;
        onRightChoice = null;
    }

    private void OnRightButtonClicked()
    {
        HideChoiceButtons();
        onRightChoice?.Invoke();
        onLeftChoice = null;
        onRightChoice = null;
    }

    private void HandleNext()
    {
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            DialogueText.text = fullText;
            isTyping = false;
            
            // ChoiceDialogue인 경우 타이핑 스킵 시 바로 선택지 표시
            if (isChoiceDialogue)
            {
                ShowChoiceButtons();
            }
        }
        else if (!isChoiceDialogue)
        {
            // 일반 Dialogue만 클릭으로 넘어감
            DialogueManager.Instance.OnDialogueFinished();
        }
        // ChoiceDialogue는 버튼 클릭으로만 넘어감
    }
}
