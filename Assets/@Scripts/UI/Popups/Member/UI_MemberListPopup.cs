using UnityEngine;
using static Define;

public class UI_MemberListPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG - 상호작용 x
        BG,
    }
    public enum Buttons
    {
        //Content
        EmployeeFrame1,
        EmployeeFrame2,
        EmployeeFrame3,
        EmployeeFrame4,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNameText,
        SubMiddleRoleText,
        SubMiddleSalaryText,

        //Content
        NameText1,
        NameText2,
        NameText3,
        NameText4,

        RoleText1,
        RoleText2,
        RoleText3,
        RoleText4,

        SalaryText1,
        SalaryText2,
        SalaryText3,
        SalaryText4,

        //SubBottom
        SubBottomSumNameText,
        SubBottomSumMemberText,
        SubBottomSumSalaryText,
    }
    private int[] _displayedMemberIndices;
    protected override void Awake()
    {
        base.Awake();

        _displayedMemberIndices = new int[MemberManager.MAX_MEMBERS];

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.EmployeeFrame1).onClick.AddListener(() => { PlayButtonClickSound(); OpenNextUI(Buttons.EmployeeFrame1); });
        GetButton((int)Buttons.EmployeeFrame2).onClick.AddListener(() => { PlayButtonClickSound(); OpenNextUI(Buttons.EmployeeFrame2); });
        GetButton((int)Buttons.EmployeeFrame3).onClick.AddListener(() => { PlayButtonClickSound(); OpenNextUI(Buttons.EmployeeFrame3); });
        GetButton((int)Buttons.EmployeeFrame4).onClick.AddListener(() => { PlayButtonClickSound(); OpenNextUI(Buttons.EmployeeFrame4); });
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        EventManager.Instance.AddEvent(EEventType.MemberListChanged, UpdateContent);

        UpdateContent();
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        EventManager.Instance.RemoveEvent(EEventType.MemberListChanged, UpdateContent);
    }
    public void OpenNextUI(Buttons buttonType)
    {
        // 버튼 타입에서 멤버 인덱스 계산
        int slot = (int)buttonType - (int)Buttons.EmployeeFrame1;
        
        // 유효성 검사
        if (slot < 0 || slot >= MemberManager.MAX_MEMBERS)
        {
            Debug.LogWarning($"Invalid member index: {slot}");
            return;
        }

        int actualIndex = _displayedMemberIndices[slot];
        if (actualIndex < 0 || actualIndex >= MemberManager.Instance.MemberCount)
        {
            Debug.LogWarning($"Invalid displayed member index for slot {slot}: {actualIndex}");
            return;
        }

        UIManager.Instance.ClosePopupUI();//현재 팝업 닫기

        UI_MemberSelectionPopup selectionPopup = UIManager.Instance.ShowPopupUI<UI_MemberSelectionPopup>();
        selectionPopup.SetInfo(EMemberSelectionType.Education, actualIndex); // 기본적으로 교육으로 설정
    }
    public void UpdateContent()
    {
        int memberCount = MemberManager.Instance.MemberCount;
        int totalSalary = 0;
        for (int slot = 0; slot < MemberManager.MAX_MEMBERS; slot++)
        {
            var nameText = GetText((int)Texts.NameText1 + slot);
            var roleText = GetText((int)Texts.RoleText1 + slot);
            var salaryText = GetText((int)Texts.SalaryText1 + slot);
            var frameButton = GetButton((int)Buttons.EmployeeFrame1 + slot);
            if (slot < memberCount)
            {
                // 구성원 정보 표시
                nameText.gameObject.SetActive(true);
                roleText.gameObject.SetActive(true);
                salaryText.gameObject.SetActive(true);
                frameButton.gameObject.SetActive(true);

                _displayedMemberIndices[slot] = slot; // 현재 UI는 간단한 1:1 매핑

                MemberData memberData = MemberManager.Instance.GetMember(slot).CurrentMemberData;
                if (memberData != null)
                {
                    nameText.text = memberData.Name;
                    roleText.text = MemberData.RoleToString(memberData.Role);
                    salaryText.text = memberData.SalaryToString(ESalaryType.Food);
                    totalSalary += memberData.Salary;
                }
                else
                {
                    nameText.text = "";
                    roleText.text = "";
                    salaryText.text = "";
                }
            }
            else
            {
                _displayedMemberIndices[slot] = -1;

                frameButton.gameObject.SetActive(false);
            }
        }

        GetText((int)Texts.MainTitleText).text = "구성원 목록";
        GetText((int)Texts.SubMiddleNameText).text = "이름";
        GetText((int)Texts.SubMiddleRoleText).text = "직업";
        GetText((int)Texts.SubMiddleSalaryText).text = "식비";

        GetText((int)Texts.SubBottomSumNameText).text = "합계";
        GetText((int)Texts.SubBottomSumMemberText).text = $"{memberCount}마리";
        GetText((int)Texts.SubBottomSumSalaryText).text = $"{totalSalary}개";
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
