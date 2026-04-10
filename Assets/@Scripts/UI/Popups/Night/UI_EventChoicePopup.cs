using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_EventChoicePopup : UI_UGUI, IUI_Popup, IClickableUI
{
    enum GameObjects
    {
        BG,
        YesToggle,
        NoToggle,
    }
    enum Buttons
    {
        //SubBottom
        ConfirmButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //Content
        EventText,
        YesToggleFrameText,
        NoToggleFrameText,

        //SubBottom
        ConfirmButtonText,
    }
    enum Images
    {

    }
    private EEventPopupType _currentType;
    private Action _onYesCallback;
    private Action _onNoCallback;

    private Toggle _yesToggle;
    private Toggle _noToggle;
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        _yesToggle = GetObject((int)GameObjects.YesToggle).GetComponent<Toggle>();
        _noToggle = GetObject((int)GameObjects.NoToggle).GetComponent<Toggle>();

        GetButton((int)Buttons.ConfirmButton).onClick.AddListener(() => { PlayButtonClickSound(); OnClickConfirmButton(); });
    }

    /// <summary>
    /// 이벤트 팝업 정보 설정
    /// </summary>
    public void SetInfo(EEventPopupType type, string eventText, Action yesCallback, Action noCallback)
    {
        _currentType = type;
        _onYesCallback = yesCallback;
        _onNoCallback = noCallback;

        SetTitleByType(_currentType);
        GetText((int)Texts.EventText).text = eventText;

        InitializeToggles();

        Canvas canvas = GetComponent<Canvas>();
        CoroutineManager.Instance.StartCoroutine(ForceUpdateLayout(canvas));
    }
    private void InitializeToggles()
    {
        // 리스너 일시 제거 (무한 루프 방지)
        _yesToggle.onValueChanged.RemoveAllListeners();
        _noToggle.onValueChanged.RemoveAllListeners();

        // 둘 다 체크 해제
        _yesToggle.isOn = false;
        _noToggle.isOn = false;

        UpdateConfirmButtonState();

        // 리스너 다시 등록
        _yesToggle.onValueChanged.AddListener(OnYesToggleChanged);
        _noToggle.onValueChanged.AddListener(OnNoToggleChanged);
    }
    /// <summary>
    /// Yes Toggle 변경 시
    /// </summary>
    private void OnYesToggleChanged(bool isOn)
    {
        if (isOn)
        {
            // Yes가 체크되면 No 해제
            _noToggle.onValueChanged.RemoveAllListeners(); // 무한 루프 방지
            _noToggle.isOn = false;
            _noToggle.onValueChanged.AddListener(OnNoToggleChanged);
        }

        // 확인 버튼 활성화 상태 업데이트
        UpdateConfirmButtonState();
    }

    /// <summary>
    /// No Toggle 변경 시
    /// </summary>
    private void OnNoToggleChanged(bool isOn)
    {
        if (isOn)
        {
            // No가 체크되면 Yes 해제
            _yesToggle.onValueChanged.RemoveAllListeners(); // 무한 루프 방지
            _yesToggle.isOn = false;
            _yesToggle.onValueChanged.AddListener(OnYesToggleChanged);
        }

        // 확인 버튼 활성화 상태 업데이트
        UpdateConfirmButtonState();
    }
    /// <summary>
    /// 확인 버튼 활성화 상태 업데이트
    /// </summary>
    private void UpdateConfirmButtonState()
    {
        // 둘 중 하나라도 체크되어 있으면 활성화
        GetButton((int)Buttons.ConfirmButton).interactable = _yesToggle.isOn || _noToggle.isOn;
    }
    /// <summary>
    /// 확인 버튼 클릭
    /// </summary>
    private void OnClickConfirmButton()
    {
        // Yes가 체크되어 있으면 YesCallback 호출
        if (_yesToggle.isOn)
        {
            UIManager.Instance.ClosePopupUI();
            _onYesCallback?.Invoke();
        }
        // No가 체크되어 있으면 NoCallback 호출
        else if (_noToggle.isOn)
        {
            UIManager.Instance.ClosePopupUI();
            _onNoCallback?.Invoke();
        }
        else
        {
            Debug.LogWarning("[UI_EventChoicePopup] No toggle selected!");
        }
    }
    private void SetTitleByType(EEventPopupType type)
    {
        string title = type switch
        {
            EEventPopupType.YearEvent => $"{GameManager.Instance.Year}년차",
            EEventPopupType.DispatchResult => "파견 결과",
            _ => ""
        };

        GetText((int)Texts.MainTitleText).text = title;
    }

    private IEnumerator ForceUpdateLayout(Canvas mainCanvas)
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(mainCanvas.GetComponent<RectTransform>());
    }
    private void PlayButtonClickSound()
    {
        SoundManager.Instance.Play2D(ESound.Effect, "Button");
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
        //SetInfo();
    }
}
