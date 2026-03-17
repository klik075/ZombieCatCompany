using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;
public class UI_ModeSelectionPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
    }
    enum Buttons
    {
        BG,

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

        //LeftContent
        RoleText,
        FundsNameText,
        FundsText,

        //RightContent
        DescriptionText,
        FoodNameText,
        FoodText,

        //SubBottom
        OkayButtonText,
    }
    enum Images
    {
        //LeftContent
        MemberImage,
    }

    private List<int> _modeKeys;
    private int _currentIndex = 0;
    private Define.EGameMode _selectedMode;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.BG).onClick.AddListener(() => UIManager.Instance.ClosePopupUI());
        GetButton((int)Buttons.NextButton).onClick.AddListener(OnClickNextButton);
        GetButton((int)Buttons.PreviousButton).onClick.AddListener(OnClickPreviousButton);
        GetButton((int)Buttons.OkayButton).onClick.AddListener(OnClickOkayButton);
    }
    private void OnClickNextButton()
    {
        //다음 모드로 내용 업데이트
        if (_modeKeys == null || _modeKeys.Count == 0)
            return;

        _currentIndex = (_currentIndex + 1) % _modeKeys.Count;
        UpdateContent();
    }
    private void OnClickPreviousButton()
    {
        //이전 모드로 내용 업데이트
        if (_modeKeys == null || _modeKeys.Count == 0)
            return;

        _currentIndex--;
        if (_currentIndex < 0)
            _currentIndex = _modeKeys.Count - 1;
        
        UpdateContent();
    }
    private void OnClickOkayButton()
    {
        UI_InputFieldPopup inputPopup = UIManager.Instance.ShowPopupUI<UI_InputFieldPopup>();
        inputPopup.SetInfo(EInputFieldType.CompanyName, OnInputCompleted);
    }
    protected override void Start()
    {
        base.Start();

        // 모드 목록 초기화
        _modeKeys = DataManager.Instance.ModeDict.Keys.OrderBy(k => k).ToList();
        
        if (_modeKeys.Count > 0)
        {
            _currentIndex = 0;
            UpdateContent();
        }
    }
    
    private void UpdateContent()
    {
        //DataManager에서 ModeDict의 목록을 가져와서 현재 인덱스에 맞는 모드의 정보를 UI에 업데이트
        if (_modeKeys == null || _currentIndex < 0 || _currentIndex >= _modeKeys.Count)
            return;

        int currentKey = _modeKeys[_currentIndex];
        
        if (!DataManager.Instance.ModeDict.TryGetValue(currentKey, out ModeData modeData))
            return;

        _selectedMode = modeData.GameMode;

        // UI 업데이트
        GetText((int)Texts.MainTitleText).text = $"모드 선택 {_currentIndex + 1}/{_modeKeys.Count}";
        GetText((int)Texts.SubMiddleNameText).text = ModeData.GetModeName(modeData.GameMode);
        GetText((int)Texts.DescriptionText).text = modeData.Description;
        GetText((int)Texts.FundsText).text = modeData.Gold.ToString();
        GetText((int)Texts.FoodText).text = modeData.Food.ToString();

        // 버튼 활성화/비활성화 (선택사항)
        GetButton((int)Buttons.NextButton).interactable = _modeKeys.Count > 1;
        GetButton((int)Buttons.PreviousButton).interactable = _modeKeys.Count > 1;
    }
    private void OnInputCompleted(string input)
    {
        SaveManager.Instance.NewGame();

        GameManager.Instance.GameMode = _selectedMode;
        GameManager.Instance.GameState = EGameState.Night;
        GameManager.Instance.CompanyName = input;
        //GameData gameData = GameManager.Instance.MyGameData;
        //gameData.GameMode = _selectedMode;
        //gameData.GameState = EGameState.Night;
        //gameData.CompanyData.CompanyName = input;

        SaveManager.Instance.SaveGameData();
        SaveManager.Instance.SaveUserData();

        UIManager.Instance.CloseAllPopupUI();

        EndingManager.Instance.TriggerGameStart();
        //SceneManager.Instance.LoadScene(EScene.NightScene);
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        //TODO : Localization
        UpdateContent();
    }
}
