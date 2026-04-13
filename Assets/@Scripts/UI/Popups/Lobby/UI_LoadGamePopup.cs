using UnityEngine;
using static Define;
public class UI_LoadGamePopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        
    }
    enum Buttons
    {
        //BG
        BG,

        ContentFrame
    }
    enum Texts
    {
        LoadGameText,

        //Top
        ModeText,
        CompanyText,

        //Bottom
        YearText,
        FundsText
    }
    enum Images
    {

    }

    private bool _hasGameData;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.BG).onClick.AddListener(() => UIManager.Instance.ClosePopupUI());
        GetButton((int)Buttons.ContentFrame).onClick.AddListener(() => { PlayButtonClickSound(); LoadGame(); });

        UpdateUI();
    }

    private void LoadGame()
    {
        Define.EScene targetScene = Define.EScene.NightScene; // 기본값

        if (SaveManager.Instance.HasGameData())
        {
            GameData gameData = SaveManager.Instance.GetGameData();

            if (gameData != null && gameData.GameState == Define.EGameState.Morning)
            {
                targetScene = Define.EScene.MorningScene;
                Debug.Log("Loading MorningScene from saved game state");
            }
            else
            {
                Debug.Log("Loading NightScene from saved game state");
            }
        }
        else
        {
            Debug.Log("No saved game data found. Loading NightScene as default");
        }

        SceneManager.Instance.LoadScene(targetScene);
    }
    private void UpdateUI()
    {
        _hasGameData = SaveManager.Instance.HasGameData();

        UpdateContent();
        UpdateButton();
    }
    private void UpdateContent()
    {
        if (!_hasGameData)
        {
            // 저장된 데이터가 없는 경우
            GetText((int)Texts.LoadGameText).text = "저장된 데이터가 없습니다.";
            return;
        }

        GameData gameData = SaveManager.Instance.GetGameData();

        // UI 업데이트
        GetText((int)Texts.LoadGameText).text = "중단된 데이터에서 재개";

        // 게임 모드
        GetText((int)Texts.ModeText).text = ModeData.GetModeName(gameData.GameMode);

        // 회사 이름
        GetText((int)Texts.CompanyText).text = gameData.CompanyData.CompanyName;

        // 연차
        GetText((int)Texts.YearText).text = $"연차: {gameData.CompanyData.Year}년";

        // 자금
        GetText((int)Texts.FundsText).text = $"자금: {gameData.CompanyData.Gold:N0}G";
    }
    private void UpdateButton()
    {
        GetButton((int)Buttons.ContentFrame).interactable = _hasGameData;
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
