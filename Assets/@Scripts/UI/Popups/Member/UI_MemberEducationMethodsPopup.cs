using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class UI_MemberEducationMethodsPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG - 상호작용 x
        BG,
    }
    enum Buttons
    {
        //MainTitle
        NextButton,
        PreviousButton,

        //LeftContent
        EducationMethodButton1,
        EducationMethodButton2,
        EducationMethodButton3,
        EducationMethodButton4,
        EducationMethodButton5,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNameText,

        //LeftContent
        EducationMethod1,
        EducationMethod2,
        EducationMethod3,
        EducationMethod4,
        EducationMethod5,

        EducationMethodCost1,
        EducationMethodCost2,
        EducationMethodCost3,
        EducationMethodCost4,
        EducationMethodCost5,

        //RightContent
        AbilityScoreText1,
        AbilityScoreText2, 
        AbilityScoreText3,
        AbilityScoreText4,
        AbilityScoreText5,

        //SubBottom
        DescriptionText,
    }
    enum Images
    {
        //RightContent
        AbilityIcon1,
        AbilityIcon2,
        AbilityIcon3,
        AbilityIcon4,
        AbilityIcon5,

        AddIcon1_1,
        AddIcon2_1,
        AddIcon3_1,
        AddIcon4_1,
        AddIcon5_1,

        AddIcon1_2,
        AddIcon2_2,
        AddIcon3_2,
        AddIcon4_2,
        AddIcon5_2,
    }
    
    private const int EDUCATION_METHODS_PER_PAGE = 5;
    private int currentPageIndex = 0;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.NextButton).onClick.AddListener(NextPage);
        GetButton((int)Buttons.PreviousButton).onClick.AddListener(PreviousPage);

        EventManager.Instance.AddEvent(EEventType.EducationCompleted, SetInfo);

        // 교육 방법 버튼들 클릭 이벤트 등록
        for (int i = 0; i < EDUCATION_METHODS_PER_PAGE; i++)
        {
            int index = i; // 클로저를 위한 지역 변수
            GetButton((int)Buttons.EducationMethodButton1 + i).onClick.AddListener(() => OnClickEducationMethodButton(index));
        }
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        SetInfo();
    }

    public void SetInfo()
    {
        currentPageIndex = 0;
        UpdateContent();
    }

    public void UpdateContent()
    {
        // MemberManager에서 현재 선택된 멤버 가져오기
        Player player = MemberManager.Instance.SelectedPlayer;
        
        if (player == null || player?.CurrentMemberData == null)
        {
            Debug.LogWarning("Target member or member data is null!");
            return;
        }

        MemberData memberData = player.CurrentMemberData;

        // 제목 업데이트
        int totalPages = EducationManager.Instance.GetTotalPages(EDUCATION_METHODS_PER_PAGE); ;
        GetText((int)Texts.MainTitleText).text = $"@교육 방법 선택 ({currentPageIndex + 1}/{Math.Max(1, totalPages)})";

        // 멤버 이름 표시
        GetText((int)Texts.SubMiddleNameText).text = memberData.Name;

        // 현재 페이지의 교육 데이터 가져오기
        List<EducationData> currentPageEducations = GetCurrentPageEducations();

        // UI 요소 업데이트
        for (int i = 0; i < EDUCATION_METHODS_PER_PAGE; i++)
        {
            EducationData educationData = i < currentPageEducations.Count ? currentPageEducations[i] : null;
            UpdateEducationSlot(i, educationData, memberData);
        }

        // 네비게이션 버튼 상태 업데이트
        UpdateNavigationButtons();
        
        // 기본 설명 텍스트 설정
        GetText((int)Texts.DescriptionText).text = "@어떤 교육을 하시겠습니까?";
    }

    private List<EducationData> GetCurrentPageEducations()
    {
        return EducationManager.Instance.GetEducationsForPage(currentPageIndex);
    }

    private void UpdateEducationSlot(int slotIndex, EducationData educationData, MemberData memberData)
    {
        // 버튼과 UI 요소들 가져오기
        var methodButton = GetButton((int)Buttons.EducationMethodButton1 + slotIndex);
        var methodText = GetText((int)Texts.EducationMethod1 + slotIndex);
        var costText = GetText((int)Texts.EducationMethodCost1 + slotIndex);
        var abilityText = GetText((int)Texts.AbilityScoreText1 + slotIndex);

        if (educationData == null)
        {
            // 빈 슬롯 - 비활성화
            methodButton.gameObject.SetActive(false);
            return;
        }

        // 슬롯 활성화 및 데이터 설정
        methodButton.gameObject.SetActive(true);
        
        methodText.text = $"@{educationData.Name}";
        costText.text = $"{educationData.Cost:N0}G";
        
        // 멤버 현재 능력치 정보 설정
        abilityText.text = memberData.GetAbilityValue((EAbilityType)slotIndex).ToString();
    }

    private void UpdateNavigationButtons()
    {
        int totalPages = EducationManager.Instance.GetTotalPages(EDUCATION_METHODS_PER_PAGE);

        GetButton((int)Buttons.PreviousButton).interactable = currentPageIndex > 0;
        GetButton((int)Buttons.NextButton).interactable = currentPageIndex < totalPages - 1;
    }

    private void NextPage()
    {
        int totalPages = EducationManager.Instance.GetTotalPages(EDUCATION_METHODS_PER_PAGE);

        if (currentPageIndex < totalPages - 1)
        {
            currentPageIndex++;
            UpdateContent();
        }
    }

    private void PreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdateContent();
        }
    }

    private void OnClickEducationMethodButton(int slotIndex)
    {
        List<EducationData> currentPageEducations = GetCurrentPageEducations();
        
        if (slotIndex >= currentPageEducations.Count)
        {
            Debug.LogWarning("Invalid education method slot clicked!");
            return;
        }

        EducationManager.Instance.SelectedEducation = currentPageEducations[slotIndex];
        
        if (!EducationManager.Instance.CanExecuteEducation())
        {
            EducationManager.Instance.SelectedEducation = null;
            UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
            chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, "@자금이 부족하다냥");
            return;
        }

        UI_MemberEducationCompletionPopup educationCompletionPopup = UIManager.Instance.ShowPopupUI<UI_MemberEducationCompletionPopup>();
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        UpdateContent();
    }
}
