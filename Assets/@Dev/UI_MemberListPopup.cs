using UnityEngine;

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
        SubBottomSumText,
        SubBottomSumMemberText,
        SubBottomSumSalaryText,
    }
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.EmployeeFrame1).onClick.AddListener(() => OpenNextUI(Buttons.EmployeeFrame1));
        GetButton((int)Buttons.EmployeeFrame2).onClick.AddListener(() => OpenNextUI(Buttons.EmployeeFrame2));
        GetButton((int)Buttons.EmployeeFrame3).onClick.AddListener(() => OpenNextUI(Buttons.EmployeeFrame3));
        GetButton((int)Buttons.EmployeeFrame4).onClick.AddListener(() => OpenNextUI(Buttons.EmployeeFrame4));
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        UpdateContent();
    }
    public void SetInfo()
    {

    }
    public void OpenNextUI(Buttons buttonType)
    {
        // 버튼 타입에서 멤버 인덱스 추출
        int memberIndex = (int)buttonType - (int)Buttons.EmployeeFrame1;
        
        // 유효성 검사
        if (memberIndex < 0 || memberIndex >= MemberManager.Instance.MemberCount)
        {
            Debug.LogWarning($"Invalid member index: {memberIndex}");
            return;
        }
        
        // 해당 멤버 정보 가져오기
        Player member = MemberManager.Instance.GetMember(memberIndex);
        if (member == null || member.CurrentMemberData == null)
        {
            Debug.LogWarning($"Member data not found at index: {memberIndex}");
            return;
        }

        UIManager.Instance.ClosePopupUI();//현재 팝업 지우기

        UI_MemberSelectionPopup popup = UIManager.Instance.ShowPopupUI<UI_MemberSelectionPopup>();
        popup.SetInfo(member);
    }
    public void UpdateContent()
    {
        //MemberManager의 구성원 정보를 불러와서 UI에 반영,
        int memberCount = MemberManager.Instance.MemberCount;
        for (int i = 0; i < 4; i++)
        {
            var nameText = GetText((int)Texts.NameText1 + i);
            var roleText = GetText((int)Texts.RoleText1 + i);
            var salaryText = GetText((int)Texts.SalaryText1 + i);
            var frameButton = GetButton((int)Buttons.EmployeeFrame1 + i);
            if (i < memberCount)
            {
                // 구성원 정보 표시
                nameText.gameObject.SetActive(true);
                roleText.gameObject.SetActive(true);
                salaryText.gameObject.SetActive(true);
                frameButton.gameObject.SetActive(true);
                
                MemberData memberData = MemberManager.Instance.GetMember(i).CurrentMemberData;
                if (memberData != null)
                {
                    nameText.text = memberData.Name;
                    roleText.text = memberData.RoleToString(memberData.Role);
                    salaryText.text = $"${memberData.Salary}개";//Localize
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
                // 빈 슬롯 처리
                //nameText.gameObject.SetActive(false);
                //roleText.gameObject.SetActive(false);
                //salaryText.gameObject.SetActive(false);
                frameButton.gameObject.SetActive(false);
            }
        }
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
    }
}
