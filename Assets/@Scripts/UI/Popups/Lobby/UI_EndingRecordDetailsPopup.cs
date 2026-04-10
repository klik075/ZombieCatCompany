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

        GetButton((int)Buttons.NextButton).onClick.AddListener(() => { PlayButtonClickSound(); OnNextButtonClicked(); });
        GetButton((int)Buttons.PreviousButton).onClick.AddListener(() => { PlayButtonClickSound(); OnPreviousButtonClicked(); });
        GetButton((int)Buttons.OkayButton).onClick.AddListener(() => { PlayButtonClickSound(); UIManager.Instance.ClosePopupUI(); });
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

        UpdateSurvivalRecordNameText();
    }

    private void ShowNoRecord()
    {
        GetText((int)Texts.NoRecordText).gameObject.SetActive(true);
        GetImage((int)Images.NoRecordImage).gameObject.SetActive(true);

        GetText((int)Texts.EndingNameText).text = "엔딩 : ?";
        GetText((int)Texts.NoRecordText).text = "기록 없음";

        // 모든 콘텐츠 텍스트 숨기기
        AllContentTextsSetActive(false);
    }

    private void ShowRecord(EndingData data)
    {
        GetText((int)Texts.NoRecordText).gameObject.SetActive(false);
        GetImage((int)Images.NoRecordImage).gameObject.SetActive(false);

        // 엔딩 정보
        GetText((int)Texts.EndingNameText).text = $"엔딩 : {EndingData.EndingTypeToString(data.EndingType)}";

        // 왼쪽 컨텐츠 (라벨)
        GetText((int)Texts.LeftContentLeftText1).text = "회사 명";
        GetText((int)Texts.LeftContentLeftText2).text = "생존 연차";
        GetText((int)Texts.LeftContentLeftText3).text = "총 자금";
        GetText((int)Texts.LeftContentLeftText4).text = "먹은 통조림";
        GetText((int)Texts.LeftContentLeftText5).text = "죽은 직원";
        GetText((int)Texts.LeftContentLeftText6).text = "고양이 처치";

        // 왼쪽 컨텐츠 (값)
        GetText((int)Texts.LeftContentRightText1).text = data.CompanyName;
        GetText((int)Texts.LeftContentRightText2).text = $"{data.Year}";
        GetText((int)Texts.LeftContentRightText3).text = $"{data.TotalGold:N0}";
        GetText((int)Texts.LeftContentRightText4).text = $"{data.ConsumedFood}";
        GetText((int)Texts.LeftContentRightText5).text = $"{data.DeadMembersCount}";
        GetText((int)Texts.LeftContentRightText6).text = $"{data.KilledCatsCount}";

        // 오른쪽 컨텐츠 (라벨)
        GetText((int)Texts.RightContentLeftText1).text = "고용 횟수";
        GetText((int)Texts.RightContentLeftText2).text = "교육 횟수";
        GetText((int)Texts.RightContentLeftText3).text = "파견 횟수";
        GetText((int)Texts.RightContentLeftText4).text = "펜스 강화";
        GetText((int)Texts.RightContentLeftText5).text = "강화 실패";
        GetText((int)Texts.RightContentLeftText6).text = "";

        // 오른쪽 컨텐츠 (값)
        GetText((int)Texts.RightContentRightText1).text = $"{data.HiredMembersCount}";
        GetText((int)Texts.RightContentRightText2).text = $"{data.EducationCount}";
        GetText((int)Texts.RightContentRightText3).text = $"{data.DispatchedMembersCount}";
        GetText((int)Texts.RightContentRightText4).text = $"{data.TotalEnhancementLevel}";
        GetText((int)Texts.RightContentRightText5).text = $"{data.EnhancementFailCount}";
        GetText((int)Texts.RightContentRightText6).text = "";

        // 모든 콘텐츠 텍스트 표시
        AllContentTextsSetActive(true);
    }
    private void UpdateSurvivalRecordNameText()
    {
        bool isShortest = (_currentIndex % 2 == 0);
        string recordTypeName = isShortest ? "최단 생존 연차 기록" : "최장 생존 연차 기록";
        GetText((int)Texts.SurvivalRecordNameText).text = recordTypeName;
    }

    private void AllContentTextsSetActive(bool isActive)
    {
        for (int i = 1; i <= 6; i++)
        {
            GetText((int)Texts.LeftContentLeftText1 + i - 1).gameObject.SetActive(isActive);
            GetText((int)Texts.LeftContentRightText1 + i - 1).gameObject.SetActive(isActive);
            GetText((int)Texts.RightContentLeftText1 + i - 1).gameObject.SetActive(isActive);
            GetText((int)Texts.RightContentRightText1 + i - 1).gameObject.SetActive(isActive);
        }
    }

    private void OnNextButtonClicked()
    {
        _currentIndex = (_currentIndex + 1) % _currentRecords.Length;
        UpdateContent();
    }

    private void OnPreviousButtonClicked()
    {
        _currentIndex = (_currentIndex - 1 + _currentRecords.Length) % _currentRecords.Length;
        UpdateContent();
    }
    /// <summary>
     /// 버튼 클릭 효과음 재생
     /// </summary>
    private void PlayButtonClickSound()
    {
        SoundManager.Instance.Play2D(ESound.Effect, "Button");
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
    }
}
