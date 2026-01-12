using JetBrains.Annotations;
using System;
using UnityEngine;
using static Define;

public class UI_MemberHirePopup : UI_UGUI, IUI_Popup
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
        PaymentNameText,
        PaymentText,

        //RightContent
        AbilityNameText1,
        AbilityNameText2,
        AbilityNameText3,
        AbilityNameText4,

        AbilityScoreText1,
        AbilityScoreText2,
        AbilityScoreText3,
        AbilityScoreText4,

        //SubBottom
        OkayButtonText,
    }
    enum Images
    {
        //LeftContent
        MemberImage,
    }
    private HireResult _hireResult;
    private int _currentIndex = 0;
    
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        // 버튼 이벤트 등록
        GetButton((int)Buttons.NextButton).onClick.AddListener(() => NextMemberInfoUpdate());
        GetButton((int)Buttons.PreviousButton).onClick.AddListener(() => PreviousMemberInfoUpdate());
        GetButton((int)Buttons.OkayButton).onClick.AddListener(() => OnOkayButtonClicked());
    }
    
    public void SetInfo(HireResult hireResult)
    {
        _hireResult = hireResult;
        _currentIndex = 0;
        UpdateContent(_currentIndex);
    }
    
    public void UpdateContent(int index)
    {
        if (_hireResult == null)
        {
            Debug.LogWarning("HireResult is null!");
            return;
        }

        if (_hireResult.MemberDatas == null || _hireResult.MemberDatas.Count == 0)
        {
            Debug.LogWarning("No member data available!");
            return;
        }

        if (index < 0 || index >= _hireResult.MemberDatas.Count)
        {
            Debug.LogWarning($"Invalid index: {index}");
            return;
        }

        MemberData memberData = _hireResult.MemberDatas[index];
        
        if (memberData == null)
        {
            Debug.LogWarning($"MemberData at index {index} is null!");
            return;
        }

        _currentIndex = index;

        // 타이틀 텍스트 (현재 멤버 / 전체 멤버 수)
        GetText((int)Texts.MainTitleText).text = $"@신규 고용 {_currentIndex + 1}/{_hireResult.MemberDatas.Count}";

        // 이름 표시
        GetText((int)Texts.SubMiddleNameText).text = memberData.Name;

        // 급여 표시
        GetText((int)Texts.SubMiddleSalaryNameText).text = "@연봉";
        GetText((int)Texts.SubMiddleSalaryText).text = $"{MemberManager.Instance.GetAnnualIncome(memberData)}G";

        // 역할 표시
        GetText((int)Texts.RoleText).text = memberData.RoleToString(memberData.Role);

        // 지불 금액 표시 (PaymentText는 급여와 동일하거나 다른 값일 수 있음)
        GetText((int)Texts.PaymentNameText).text = "@계약금";
        GetText((int)Texts.PaymentText).text = $"{MemberManager.Instance.GetHireCost(memberData)}G";

        // 능력치 이름 설정
        GetText((int)Texts.AbilityNameText1).text = memberData.AbilityToString(EAbilityType.Programming);
        GetText((int)Texts.AbilityNameText2).text = memberData.AbilityToString(EAbilityType.Scenario);
        GetText((int)Texts.AbilityNameText3).text = memberData.AbilityToString(EAbilityType.Graphics);
        GetText((int)Texts.AbilityNameText4).text = memberData.AbilityToString(EAbilityType.Sound);

        // 능력치 점수 설정
        GetText((int)Texts.AbilityScoreText1).text = memberData.Programming.ToString();
        GetText((int)Texts.AbilityScoreText2).text = memberData.Scenario.ToString();
        GetText((int)Texts.AbilityScoreText3).text = memberData.Graphics.ToString();
        GetText((int)Texts.AbilityScoreText4).text = memberData.Sound.ToString();

        // 멤버 이미지 설정
        string imagePath = memberData.NormalImagePath;
        
        if (!string.IsNullOrEmpty(imagePath))
        {
            Sprite memberSprite = ResourceManager.Instance.Get<Sprite>(imagePath);
            if (memberSprite != null)
            {
                GetImage((int)Images.MemberImage).sprite = memberSprite;
            }
            else
            {
                Debug.LogWarning($"Failed to load sprite at path: {imagePath}");
            }
        }

        // 확인 버튼 텍스트
        GetText((int)Texts.OkayButtonText).text = "@고용";
    }
    
    private void NextMemberInfoUpdate()
    {
        if (_hireResult == null || _hireResult.MemberDatas == null || _hireResult.MemberDatas.Count == 0)
            return;
        
        int nextIndex = (_currentIndex + 1) % _hireResult.MemberDatas.Count;
        UpdateContent(nextIndex);
    }
    
    private void PreviousMemberInfoUpdate()
    {
        if (_hireResult == null || _hireResult.MemberDatas == null || _hireResult.MemberDatas.Count == 0)
            return;
        
        int prevIndex = (_currentIndex - 1 + _hireResult.MemberDatas.Count) % _hireResult.MemberDatas.Count;
        UpdateContent(prevIndex);
    }
    
    private void OnOkayButtonClicked()
    {
        if (_hireResult == null || _hireResult.MemberDatas == null || _currentIndex >= _hireResult.MemberDatas.Count)
        {
            Debug.LogWarning("Cannot hire: Invalid hire result!");
            return;
        }
        
        MemberData selectedMember = _hireResult.MemberDatas[_currentIndex];
        
        if (selectedMember == null)
        {
            Debug.LogWarning("Cannot hire: Selected member is null!");
            return;
        }
        
        // 팀이 가득 찼는지 확인
        if (MemberManager.Instance.IsTeamFull())
        {
            Debug.LogWarning("Cannot hire: Team is full!");
            // TODO: 팝업으로 "팀이 가득 찼습니다" 메시지 표시
            return;
        }
        
        //// 자금이 충분한지 확인
        //int hireCost = MemberManager.Instance.GetHireCost(selectedMember);
        //if (GameManager.Instance.Gold < hireCost)
        //{
        //    Debug.LogWarning($"Cannot hire: Not enough gold! Need {hireCost}, have {GameManager.Instance.Gold}");
        //    // TODO: 팝업으로 "자금이 부족합니다" 메시지 표시
        //    return;
        //}
        
        // 실제 멤버 고용
        if (MemberManager.Instance.HireMember(selectedMember))
        {
            //// 자금 차감
            //GameManager.Instance.Gold -= hireCost;
            
            Debug.Log($"<color=green>Successfully hired {selectedMember.Name}!</color>");
            _hireResult.MemberDatas.RemoveAt(_currentIndex);

            if (_hireResult.MemberDatas.Count > 0)
            {
                UpdateContent(0);
            }
            else
                UIManager.Instance.ClosePopupUI();

        }
        else
        {
            Debug.LogError("Failed to hire member!");
        }
    }
    
    public override void RefreshUI()
    {
        base.RefreshUI();
        // TODO: Localization
    }
}
