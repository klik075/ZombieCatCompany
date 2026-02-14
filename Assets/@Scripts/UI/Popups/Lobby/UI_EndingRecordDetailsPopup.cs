using UnityEngine;
using static Define;
public class UI_EndingRecordDetailsPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG
        BG,
    }
    enum Buttons
    {
        //MainTitle
        NextButton,
        PreviousButton,

        //SubBottom
        OkayButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        EndingNameText,
        ModeNameText,

        //LeftContent
        LeftContentLeftText1,
        LeftContentLeftText2,
        LeftContentLeftText3,
        LeftContentLeftText4,
        LeftContentLeftText5,
        LeftContentLeftText6,

        LeftContentRightText1,
        LeftContentRightText2,
        LeftContentRightText3,
        LeftContentRightText4,
        LeftContentRightText5,
        LeftContentRightText6,

        SurvivalRecordNameText,
        //RightContent
        RightContentLeftText1,
        RightContentLeftText2,
        RightContentLeftText3,
        RightContentLeftText4,
        RightContentLeftText5,
        RightContentLeftText6,

        RightContentRightText1,
        RightContentRightText2,
        RightContentRightText3,
        RightContentRightText4,
        RightContentRightText5,
        RightContentRightText6,

        //NoRecord
        NoRecordText,

        //SubBottom
        OkayButtonText,
    }
    enum Images
    {
        NoRecordImage,
    }

    private EGameMode _currentMode;
    private int _currentIndex = 0;
    private EndingData[] _currentRecords;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.NextButton).onClick.AddListener(() => OnNextButtonClicked());
        GetButton((int)Buttons.PreviousButton).onClick.AddListener(() => OnPreviousButtonClicked());
        GetButton((int)Buttons.OkayButton).onClick.AddListener(() => UIManager.Instance.ClosePopupUI());
    }

    public void SetInfo(EGameMode mode)
    {
        _currentMode = mode;
        _currentIndex = 0;

        // GameManager에서 해당 모드의 엔딩 기록 가져오기
        if (GameManager.Instance.UserData.EndingRecords.ContainsKey(mode))
        {
            _currentRecords = GameManager.Instance.UserData.EndingRecords[mode];
        }

        UpdateContent();
    }

    private void UpdateContent()
    {
        UpdateUIByMode();
        UpdateRecordDisplay();
        UpdateNavigationButtons();
        
        GetText((int)Texts.OkayButtonText).text = "확인";
    }

    private void UpdateUIByMode()
    {
        string modeName = ModeData.GetModeName(_currentMode);
        GetText((int)Texts.MainTitleText).text = $"엔딩 기록 {_currentIndex + 1}/{_currentRecords.Length}";
        GetText((int)Texts.ModeNameText).text = modeName;
    }

    private void UpdateRecordDisplay()
    {
        EndingData currentData = _currentRecords[_currentIndex];

        if (currentData == null)
        {
            // 기록이 없는 경우
            ShowNoRecord();
        }
        else
        {
            // 기록이 있는 경우
            ShowRecord(currentData);
        }
    }

    private void ShowNoRecord()
    {
        GetText((int)Texts.NoRecordText).gameObject.SetActive(true);
        GetImage((int)Images.NoRecordImage).gameObject.SetActive(true);

        GetText((int)Texts.EndingNameText).text = "?";
        GetText((int)Texts.NoRecordText).text = "기록 없음";

        // 모든 콘텐츠 텍스트 숨기기
        AllContentTextsSetActive(false);
    }

    private void ShowRecord(EndingData data)
    {
        GetText((int)Texts.NoRecordText).gameObject.SetActive(false);
        GetImage((int)Images.NoRecordImage).gameObject.SetActive(false);

        // 엔딩 정보
        GetText((int)Texts.EndingNameText).text = data.EndingName;

        // 왼쪽 콘텐츠 (라벨)
        GetText((int)Texts.LeftContentLeftText1).text = "회사명";
        GetText((int)Texts.LeftContentLeftText2).text = "연차";
        GetText((int)Texts.LeftContentLeftText3).text = "총 자금";
        GetText((int)Texts.LeftContentLeftText4).text = "먹은 통조림";
        GetText((int)Texts.LeftContentLeftText5).text = "죽은 멤버";
        GetText((int)Texts.LeftContentLeftText6).text = "잡은 고양이";

        // 왼쪽 콘텐츠 (값)
        GetText((int)Texts.LeftContentRightText1).text = data.CompanyName;
        GetText((int)Texts.LeftContentRightText2).text = $"{data.Year}년차";
        GetText((int)Texts.LeftContentRightText3).text = $"{data.TotalGold:N0}";
        GetText((int)Texts.LeftContentRightText4).text = $"{data.ConsumedFood}개";
        GetText((int)Texts.LeftContentRightText5).text = $"{data.DeadMembersCount}명";
        GetText((int)Texts.LeftContentRightText6).text = $"{data.KilledCatsCount}마리";

        // 모든 콘텐츠 텍스트 표시
        AllContentTextsSetActive(true);
    }

    private void AllContentTextsSetActive(bool isActive)
    {
        for (int i = 1; i <= 6; i++)
        {
            GetText((int)Texts.LeftContentLeftText1 + i - 1).gameObject.SetActive(isActive);
            GetText((int)Texts.LeftContentRightText1 + i - 1).gameObject.SetActive(isActive);
        }
    }

    private void UpdateNavigationButtons()
    {
        GetButton((int)Buttons.PreviousButton).interactable = _currentIndex > 0;
        GetButton((int)Buttons.NextButton).interactable = _currentIndex < _currentRecords.Length - 1;
    }

    private void OnNextButtonClicked()
    {
        if (_currentIndex < _currentRecords.Length - 1)
        {
            _currentIndex++;
            UpdateContent();
        }
    }

    private void OnPreviousButtonClicked()
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            UpdateContent();
        }
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
    }
}
