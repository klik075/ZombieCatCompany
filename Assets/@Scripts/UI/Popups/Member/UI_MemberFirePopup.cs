using UnityEngine;
using static Define;

public class UI_MemberFirePopup : UI_UGUI, IUI_Popup
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

        //SubBottom
        OkayButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNameText,
        SubMiddleSalaryNameText,
        SubMiddleSalaryText,

        //LeftContent
        RoleText,
        StateNameText,
        StateText,

        //RightContent
        AbilityNameText1,
        AbilityScoreText1,
        AbilityNameText2,
        AbilityScoreText2,
        AbilityNameText3,
        AbilityScoreText3,
        AbilityNameText4,
        AbilityScoreText4,
        AbilityNameText5,
        AbilityScoreText5,

        //SubBottom
        OkayButtonText,
    }
    enum Images
    {
        //LeftContent
        MemberImage,
    }
    private EFireType _eFireType;
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.NextButton).onClick.AddListener(() => { PlayButtonClickSound(); MemberManager.Instance.SelectNextMember(); });
        GetButton((int)Buttons.PreviousButton).onClick.AddListener(() => { PlayButtonClickSound(); MemberManager.Instance.SelectPreviousMember(); });
        GetButton((int)Buttons.OkayButton).onClick.AddListener(() => { PlayButtonClickSound(); OnClickOkayButton(); });
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.Instance.AddEvent(EEventType.SelectedMemberChanged, UpdateContent);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.Instance.RemoveEvent(EEventType.SelectedMemberChanged, UpdateContent);
    }
    public void SetInfo(EFireType eFireType, int index = 0)
    {
        _eFireType = eFireType;
        MemberManager.Instance.SelectMemberByIndex(index);//현재 선택된 멤버 설정
    }

    public void UpdateContent()
    {
        Member member = MemberManager.Instance.SelectedMember;

        if (member == null || member.CurrentMemberData == null)
        {
            Debug.LogWarning("Player, MemberData is null!");
            return;
        }

        MemberData memberData = member.CurrentMemberData;

        Member dispatchMember = MemberManager.Instance.GetDispatchMember();
        bool isDispatchMember = dispatchMember != null && dispatchMember == member;

        // 이름 표시
        GetText((int)Texts.SubMiddleNameText).text = memberData.Name;

        // 급여 표시
        GetText((int)Texts.SubMiddleSalaryNameText).text = "식비";
        GetText((int)Texts.SubMiddleSalaryText).text = memberData.SalaryToString(ESalaryType.Food);

        // 역할 표시
        GetText((int)Texts.RoleText).text = MemberData.RoleToString(memberData.Role);

        // 상태 표시
        GetText((int)Texts.StateNameText).text = "상태";
        GetText((int)Texts.StateText).text = MemberData.StateToString(memberData.State, isDispatchMember);

        // 능력치 이름 설정
        GetText((int)Texts.AbilityNameText1).text = MemberData.AbilityToString(EAbilityType.Programming);
        GetText((int)Texts.AbilityNameText2).text = MemberData.AbilityToString(EAbilityType.Scenario);
        GetText((int)Texts.AbilityNameText3).text = MemberData.AbilityToString(EAbilityType.Graphics);
        GetText((int)Texts.AbilityNameText4).text = MemberData.AbilityToString(EAbilityType.Sound);
        GetText((int)Texts.AbilityNameText5).text = MemberData.AbilityToString(EAbilityType.Power);

        // 능력치 점수 설정
        GetText((int)Texts.AbilityScoreText1).text = memberData.Programming.ToString();
        GetText((int)Texts.AbilityScoreText2).text = memberData.Scenario.ToString();
        GetText((int)Texts.AbilityScoreText3).text = memberData.Graphics.ToString();
        GetText((int)Texts.AbilityScoreText4).text = memberData.Sound.ToString();
        GetText((int)Texts.AbilityScoreText5).text = memberData.Power.ToString();

        // 멤버 이미지 설정 (상태에 따라 normal/zombie 이미지 선택)
        string imagePath = memberData.ZombieImagePath;
        
        if (!string.IsNullOrEmpty(imagePath))
        {
            Sprite memberSprite = ResourceManager.Instance.Get<Sprite>(imagePath);

            if (memberSprite != null)
                GetImage((int)Images.MemberImage).sprite = memberSprite;
        }

        // 확인 버튼 텍스트
        GetText((int)Texts.OkayButtonText).text = "해고";
        
        int currentIndex = MemberManager.Instance.SelectedMemberIndex;
        int order = currentIndex + 1;
        GetText((int)Texts.MainTitleText).text = $"구성원 해고 {order}/{MemberManager.Instance.MemberCount}";
        
        // 해고 버튼 활성화/비활성화 처리
        bool canFire = MemberManager.Instance.CanFireMember(currentIndex);
        GetButton((int)Buttons.OkayButton).interactable = canFire;
    }

    public void OnClickOkayButton()
    {
        Member player = MemberManager.Instance.SelectedMember;

        if (player == null || player.CurrentMemberData == null)
        {
            Debug.LogWarning("Player, MemberData is null!");
            return;
        }

        MemberData memberData = player.CurrentMemberData;
        //멤버 해고 팝업
        UI_MessagePopup messagePopup = UIManager.Instance.ShowPopupUI<UI_MessagePopup>();
        messagePopup.SetInfo(MessageManager.Instance.GetMessageScript(EMessageType.MemberFired).Contents, new string[] {memberData.Name}, FireSelectedMember);
    }
    private void FireSelectedMember()
    {
        Member selectedPlayer = MemberManager.Instance.SelectedMember;

        if (selectedPlayer == null || selectedPlayer.CurrentMemberData == null)
        {
            Debug.LogWarning("Player, MemberData is null!");
            return;
        }

        MemberData memberData = selectedPlayer.CurrentMemberData;

        bool success = MemberManager.Instance.FireSelectedMember();

        if (_eFireType == EFireType.Normal)
        {
            //멤버 해고 완료 팝업
            UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
            chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MessageManager.Instance.GetMessageScript(EMessageType.MemberFiredConfirm).Contents, new string[] { memberData.Name });
        }
        else if(_eFireType == EFireType.Swap)
        {

            //멤버 교체 팝업
            UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
            chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MessageManager.Instance.GetMessageScript(EMessageType.MemberSwapped).Contents, new string[] { memberData.Name, MemberManager.Instance.SelectedHireMemberData.Name }, OnStartSwap);
        }

        if (success && MemberManager.Instance.SelectedMemberIndex != MemberManager.Instance.MemberCount)
            MemberManager.Instance.SelectNextMember();
    }
    public void OnStartSwap()
    {
        MemberData selectedHireMember = MemberManager.Instance.SelectedHireMemberData;
        if (MemberManager.Instance.HireMember(selectedHireMember))
        {
            UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
            chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MessageManager.Instance.GetMessageScript(EMessageType.MemberHired).Contents, new string[] { selectedHireMember.Name }, OnCompleteSwap);
            UpdateContent();
        }
    }
    public void OnCompleteSwap()
    {
        UIManager.Instance.ClosePopupUI();
        EventManager.Instance.TriggerEvent(EEventType.MemberSwapped);
    }
    private void PlayButtonClickSound()
    {
        SoundManager.Instance.Play2D(ESound.Effect, "Button");
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
        UpdateContent();
        //TODO : Localization
    }
}
