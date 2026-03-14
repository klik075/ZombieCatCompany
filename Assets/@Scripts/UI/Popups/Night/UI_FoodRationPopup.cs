using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using static Define;
using TMPro;

public class UI_FoodRationPopup : UI_UGUI, IUI_Popup, IClickableUI
{
    enum GameObjects
    {
        //BG - 상호작용 x
        BG,

        //Content
        MemberFrame1,
        MemberFrame2,
        MemberFrame3,
        MemberFrame4,

        Toggle1,
        Toggle2,
        Toggle3,
        Toggle4,

        Tooltip1,
        Tooltip2,
        Tooltip3,
        Tooltip4,
    }
    enum Buttons
    {
        //MainTitle

        //SubBottom
        OkayButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNecessaryFoodNameText,
        SubMiddleNecessaryFoodText,

        //Content
        FoddText1,
        FoddText2,
        FoddText3,
        FoddText4,

        MemberNameText1,
        MemberNameText2,
        MemberNameText3,
        MemberNameText4,

        MemberStateText1,
        MemberStateText2,
        MemberStateText3,
        MemberStateText4,

        //SubBottom
        OkayButtonText,
    }
    enum Images
    {
        //Content
        MemberImage1,
        MemberImage2,
        MemberImage3,
        MemberImage4,

        DispatchImage1,
        DispatchImage2,
        DispatchImage3,
        DispatchImage4,
    }

    private Toggle[] _toggles = new Toggle[MemberManager.MAX_MEMBERS];
    private int[] _memberFoodCosts = new int[MemberManager.MAX_MEMBERS];
    private GameObject[] _tooltips = new GameObject[MemberManager.MAX_MEMBERS];

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.OkayButton).onClick.AddListener(OnOkayButtonClicked);
    }

    protected override void OnEnable()
    {
        // 팝업이 활성화될 때마다 초기화
        InitializeTooltips();
        InitializeToggles();
        SetupMemberFrames();
        UpdateTotalFoodCost();
    }

    private void InitializeTooltips()
    {
        for (int i = 0; i < MemberManager.MAX_MEMBERS; i++)
        {
            GameObject tooltipObj = GetObject((int)GameObjects.Tooltip1 + i);
            _tooltips[i] = tooltipObj;
            
            // 처음에는 모두 비활성화
            tooltipObj.SetActive(false);
        }
    }

    private void InitializeToggles()
    {
        List<Member> members = MemberManager.Instance.GetAllMembers();
        
        for (int i = 0; i < MemberManager.MAX_MEMBERS; i++)
        {
            GameObject toggleObj = GetObject((int)GameObjects.Toggle1 + i);
            _toggles[i] = toggleObj.GetComponent<Toggle>();

            // 기존 리스너 제거 (중복 방지)
            _toggles[i].onValueChanged.RemoveAllListeners();

            if(i < members.Count && members[i].IsDispatched)
            {
                _toggles[i].interactable = false;
                _toggles[i].isOn = false;
                continue;
            }

            if (_toggles[i] != null)
            {
                _toggles[i].interactable = true;
                int index = i; // 클로저 캡처 방지
                _toggles[i].isOn = false;
                _toggles[i].onValueChanged.AddListener((isOn) => OnToggleValueChanged(index, isOn));
            }
        }
    }

    private void SetupMemberFrames()
    {
        int memberCount = MemberManager.Instance.MemberCount;

        for (int i = 0; i < MemberManager.MAX_MEMBERS; i++)
        {
            GameObject frame = GetObject((int)GameObjects.MemberFrame1 + i);
            
            if (i < memberCount)
            {
                // 멤버가 있으면 활성화
                frame.SetActive(true);
                SetupMemberFrame(i);
            }
            else
            {
                // 멤버가 없으면 비활성화
                frame.SetActive(false);
            }
        }
    }

    private void SetupMemberFrame(int index)
    {
        Member member = MemberManager.Instance.GetMember(index);
        
        if (member == null || member.CurrentMemberData == null)
        {
            Debug.LogWarning($"[UI_FoodRationPopup] Member at index {index} is null");
            return;
        }

        MemberData memberData = member.CurrentMemberData;

        GetText((int)Texts.MemberNameText1 + index).text = memberData.Name;

        // 멤버 이미지 설정
        Image memberImage = GetImage((int)Images.MemberImage1 + index);
        memberImage.sprite = member.GetMemberSprite();

        // 기존 EventTrigger 제거 후 새로 추가 (중복 방지)
        EventTrigger eventTrigger = memberImage.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger != null)
        {
            eventTrigger.triggers.Clear();
        }
        
        // 멤버 이미지에 마우스 호버 이벤트 추가
        SetupImageHoverEvents(memberImage.gameObject, index, memberData);

        Image dispatchImage = GetImage((int)Images.DispatchImage1 + index);

        if (member.IsDispatched)
        {
            // 파견 중이면 MemberImage를 반투명으로
            Color imageColor = memberImage.color;
            imageColor.a = 0.5f;
            memberImage.color = imageColor;

            // DispatchImage 활성화
            dispatchImage.gameObject.SetActive(true);
        }
        else
        {
            // 파견 중이 아니면 MemberImage 원래대로
            Color imageColor = memberImage.color;
            imageColor.a = 1.0f;
            memberImage.color = imageColor;

            // DispatchImage 비활성화
            dispatchImage.gameObject.SetActive(false);
        }

        // 식량 비용 설정
        int foodCost = memberData.SalaryToValue(ESalaryType.Food);
        _memberFoodCosts[index] = foodCost;

        GetText((int)Texts.FoddText1 + index).text = foodCost.ToString();
    }

    private void SetupImageHoverEvents(GameObject imageObject, int index, MemberData memberData)
    {
        // EventTrigger 컴포넌트 추가 또는 가져오기
        EventTrigger eventTrigger = imageObject.GetOrAddComponent<EventTrigger>();

        // PointerEnter 이벤트 (마우스 진입)
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((data) => { OnMemberImageHoverEnter(index, memberData); });
        eventTrigger.triggers.Add(pointerEnter);

        // PointerExit 이벤트 (마우스 나감)
        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => { OnMemberImageHoverExit(index); });
        eventTrigger.triggers.Add(pointerExit);
    }

    private void OnMemberImageHoverEnter(int index, MemberData memberData)
    {
        if (_tooltips[index] == null)
            return;

        GetText((int)Texts.MemberNameText1 + index).text = $"{memberData.Name}";
        GetText((int)Texts.MemberStateText1 + index).text = $"{MemberData.StateToString(memberData.State)}";

        // Tooltip 활성화
        _tooltips[index].SetActive(true);
    }

    private void OnMemberImageHoverExit(int index)
    {
        if (_tooltips[index] == null)
            return;

        // Tooltip 비활성화
        _tooltips[index].SetActive(false);
    }

    private void OnToggleValueChanged(int index, bool isOn)
    {
        UpdateTotalFoodCost();
    }

    private void UpdateTotalFoodCost()
    {
        int totalFood = CaculateFoodCost();

        if (totalFood > GameManager.Instance.Food)
        {
            GetText((int)Texts.SubMiddleNecessaryFoodText).color = Color.red;
        }
        else
        {
            GetText((int)Texts.SubMiddleNecessaryFoodText).color = Color.black;
        }

        GetText((int)Texts.SubMiddleNecessaryFoodText).text = totalFood.ToString();
    }
    private int CaculateFoodCost()
    {
        int totalFoodCost = 0;

        for (int i = 0; i < MemberManager.MAX_MEMBERS; i++)
        {
            if (_toggles[i] != null && _toggles[i].isOn)
            {
                totalFoodCost += _memberFoodCosts[i];
            }
        }

        return totalFoodCost;
    }
    private void OnOkayButtonClicked()
    {
        int totalFood = CaculateFoodCost();

        if (totalFood > GameManager.Instance.Food)
        {
            UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
            chatPopup.SetInfo(
                MemberManager.Instance.MainCharacter.CurrentMemberData.EmployeeID,
                MessageManager.Instance.GetMessageScript(EMessageType.FoodShortage).Contents,
                null,
                null);
            return;
        }

        GameManager.Instance.Food -= totalFood;

        // TODO : 선택된 멤버 배고픔 단계 이전으로 두 칸 이동

        UIManager.Instance.ClosePopupUI();
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        SetupMemberFrames();
        UpdateTotalFoodCost();
    }
}
