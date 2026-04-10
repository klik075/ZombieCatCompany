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
    
    // 선택된 슬롯 추적
    private int selectedSlotIndex = -1;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.NextButton).onClick.AddListener(() => { PlayButtonClickSound(); NextPage(); });
        GetButton((int)Buttons.PreviousButton).onClick.AddListener(() => { PlayButtonClickSound(); PreviousPage(); });

        // 교육 방법 버튼들 클릭 이벤트 등록
        for (int i = 0; i < EDUCATION_METHODS_PER_PAGE; i++)
        {
            int index = i; // 클로저를 위한 지역 변수
            GetButton((int)Buttons.EducationMethodButton1 + i).onClick.AddListener(() => { PlayButtonClickSound(); OnClickEducationMethodButton(index); });
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        EventManager.Instance.AddEvent(EEventType.EducationCompleted, SetInfo);

        SetInfo();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.Instance.RemoveEvent(EEventType.EducationCompleted, SetInfo);
    }

    public void SetInfo()
    {
        currentPageIndex = 0;
        selectedSlotIndex = -1; // 선택 상태 초기화
        ResetAllAddIcons(); // + 아이콘 초기화
        UpdateContent();
    }

    public void UpdateContent()
    {
        // MemberManager에서 현재 선택된 멤버 가져오기
        Member player = MemberManager.Instance.SelectedMember;
        
        if (player == null || player?.CurrentMemberData == null)
        {
            Debug.LogWarning("Target member or member data is null!");
            return;
        }

        MemberData memberData = player.CurrentMemberData;

        // 제목 업데이트
        int totalPages = EducationManager.Instance.GetTotalPages(EDUCATION_METHODS_PER_PAGE); ;
        GetText((int)Texts.MainTitleText).text = $"교육 방법 선택 ({currentPageIndex + 1}/{Math.Max(1, totalPages)})";

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
        GetText((int)Texts.DescriptionText).text = "어떤 교육을 하시겠습니까?";
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
        
        methodText.text = $"{educationData.Name}";
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
            selectedSlotIndex = -1; // 페이지 변경 시 선택 초기화
            ResetAllAddIcons(); // + 아이콘 초기화
            UpdateContent();
        }
    }

    private void PreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            selectedSlotIndex = -1; // 페이지 변경 시 선택 초기화
            ResetAllAddIcons(); // + 아이콘 초기화
            UpdateContent();
        }
    }

    /// <summary>
    /// 교육 방법 버튼 클릭 시
    /// </summary>
    private void OnClickEducationMethodButton(int slotIndex)
    {
        List<EducationData> currentPageEducations = GetCurrentPageEducations();
        
        if (slotIndex >= currentPageEducations.Count)
        {
            Debug.LogWarning("Invalid education method slot clicked!");
            return;
        }

        // 같은 버튼을 다시 클릭했는지 확인
        if (selectedSlotIndex == slotIndex)
        {
            // 두 번째 클릭 → 교육 실행
            ExecuteEducation(slotIndex);
        }
        else
        {
            // 첫 번째 클릭 → 선택 상태로 표시
            SelectEducationMethod(slotIndex);
        }
    }

    /// <summary>
    /// 교육 방법 선택 (첫 번째 클릭)
    /// </summary>
    private void SelectEducationMethod(int slotIndex)
    {
        selectedSlotIndex = slotIndex;
        
        List<EducationData> currentPageEducations = GetCurrentPageEducations();
        EducationData educationData = currentPageEducations[slotIndex];

        if (educationData == null)
            return;

        // 모든 + 아이콘 초기화
        ResetAllAddIcons();

        for (int i = 0; i < 5; i++)
        {
            int increaseAmount = educationData.GetAbilityIncrease(i);

            if (increaseAmount > 0)
            {
                SetAddIconActive(i, increaseAmount);
            }
        }

        GetText((int)Texts.DescriptionText).text = $"선택 : {educationData.Name}";
    }

    /// <summary>
    /// 교육 실행 (두 번째 클릭)
    /// </summary>
    private void ExecuteEducation(int slotIndex)
    {
        List<EducationData> currentPageEducations = GetCurrentPageEducations();
        EducationData educationData = currentPageEducations[slotIndex];

        EducationManager.Instance.SelectedEducation = educationData;
        
        if (!EducationManager.Instance.CanExecuteEducation())
        {
            EducationManager.Instance.SelectedEducation = null;
            selectedSlotIndex = -1; // 선택 해제
            ResetAllAddIcons(); // + 아이콘 초기화
            
            // 자금 부족 팝업
            UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
            chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MessageManager.Instance.GetMessageScript(EMessageType.MoneyLow).Contents);
            return;
        }

        Debug.Log($"[Executed] Education: {educationData.Name}");
        
        UI_MemberEducationCompletionPopup educationCompletionPopup = UIManager.Instance.ShowPopupUI<UI_MemberEducationCompletionPopup>();
        
        selectedSlotIndex = -1; // 실행 후 선택 초기화
        ResetAllAddIcons(); // + 아이콘 초기화
    }

    /// <summary>
    /// 모든 + 아이콘 비활성화
    /// </summary>
    private void ResetAllAddIcons()
    {
        for (int i = 0; i < 5; i++)
        {
            GetImage((int)Images.AddIcon1_1 + i).gameObject.SetActive(false);
            GetImage((int)Images.AddIcon1_2 + i).gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 특정 능력치의 + 아이콘 활성화
    /// </summary>
    /// <param name="abilityIndex">능력치 인덱스 (0: Programming, 1: Scenario, 2: Graphics, 3: Sound, 4: Power)</param>
    /// <param name="increaseAmount">증가량</param>
    private void SetAddIconActive(int abilityIndex, int increaseAmount)
    {
        if (abilityIndex < 0 || abilityIndex >= 5)
        {
            Debug.LogWarning($"Invalid ability index: {abilityIndex}");
            return;
        }

        // + 아이콘은 항상 활성화 (_1)
        GetImage((int)Images.AddIcon1_1 + abilityIndex).gameObject.SetActive(true);

        // 증가량이 5 이상이면 ++ 아이콘도 활성화 (_2)
        if (increaseAmount >= 5)
        {
            GetImage((int)Images.AddIcon1_2 + abilityIndex).gameObject.SetActive(true);
        }

        Debug.Log($"[AddIcon] Ability {abilityIndex} ({(EAbilityType)abilityIndex}): +{increaseAmount} {(increaseAmount >= 5 ? "(++)" : "(+)")}");
    }
    private void PlayButtonClickSound()
    {
        SoundManager.Instance.Play2D(ESound.Effect, "Button");
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
        UpdateContent();
    }
}
