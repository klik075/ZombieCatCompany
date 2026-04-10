using System;
using UnityEngine;
using static Define;

/// <summary>
/// 결과 보고 데이터 기본 클래스
/// </summary>
public abstract class ResultReportData
{
    public EResultsReportType Type { get; protected set; }
    
    public abstract string GetMainTitle();
    public abstract (string name, string value)[] GetResultItems();
    public abstract int GetRewardAmount();
}

/// <summary>
/// 게임 판매 결과 데이터
/// </summary>
public class GameSalesResultData : ResultReportData
{
    public string GameTitle { get; }
    public int SalesCount { get; }
    public int SalesRevenue { get; }

    public GameSalesResultData(string gameTitle, int salesCount, int salesRevenue)
    {
        Type = EResultsReportType.GameSales;
        GameTitle = gameTitle;
        SalesCount = salesCount;
        SalesRevenue = salesRevenue;
    }

    public override string GetMainTitle() => "게임 결과 보고서";

    public override (string name, string value)[] GetResultItems()
    {
        return new[]
        {
            ("게임 이름", GameTitle),
            ("판매량", $"{SalesCount:N0}"),
            ("영업 수익", $"{SalesRevenue:N0}")
        };
    }

    public override int GetRewardAmount() => SalesRevenue;
}

/// <summary>
/// 디펜스 결과 데이터
/// </summary>
public class DefenseResultData : ResultReportData
{
    public int DefeatedCats { get; }
    public int DefenseReward { get; }
    public int DefeatedFoods { get; }

    public DefenseResultData(int defeatedCats, int defenseReward, int defeatedFoods)
    {
        Type = EResultsReportType.Defense;
        DefeatedCats = defeatedCats;
        DefenseReward = defenseReward;
        DefeatedFoods = defeatedFoods;
    }

    public override string GetMainTitle() => "디펜스 결과 보고서";

    public override (string name, string value)[] GetResultItems()
    {
        return new[]
        {
            ("고양이 처치", $"{DefeatedCats:N0}"),
            ("강탈 자금", $"{DefenseReward:N0}"),
            ("강탈 식량", $"{DefeatedFoods:N0}"),
        };
    }

    public override int GetRewardAmount() => DefenseReward;
}

public class UI_ResultsReportPopup : UI_UGUI, IUI_Popup, IClickableUI
{
    enum GameObjects
    {
        BG,
    }
    enum Buttons
    {
        OkayButton,
    }
    enum Texts
    {
        MainTitleText,
        ResultNameText1,
        ResultNameText2,
        ResultNameText3,
        ResultText1,
        ResultText2,
        ResultText3,
        OkayButtonText,
    }
    enum Images
    {
    }

    private ResultReportData _resultData;
    private Action _onConfirm;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.OkayButton).onClick.AddListener(() => { PlayButtonClickSound(); OnClickOkayButton(); });
    }

    public void SetInfo(ResultReportData data, Action onConfirm = null)
    {
        _resultData = data;
        _onConfirm = onConfirm;
        
        UpdateContent();
        GetReward();
    }

    private void UpdateContent()
    {
        GetText((int)Texts.MainTitleText).text = _resultData.GetMainTitle();

        var items = _resultData.GetResultItems();

        for (int i = 0; i < 3; i++)
        {
            if (i < items.Length)
            {
                GetText((int)Texts.ResultNameText1 + i).text = items[i].name;
                GetText((int)Texts.ResultText1 + i).text = items[i].value;
            }
            else
            {
                GetText((int)Texts.ResultNameText1 + i).text = "-";
                GetText((int)Texts.ResultText1 + i).text = "-";
            }
        }

        GetText((int)Texts.OkayButtonText).text = "확인";
    }

    private void GetReward()
    {
        int reward = _resultData.GetRewardAmount();
        GameManager.Instance.Gold += reward;
        
        Debug.Log($"[ResultsReport] {_resultData.Type} reward: {reward}원");
    }

    private void OnClickOkayButton()
    {
        UIManager.Instance.ClosePopupUI();
        _onConfirm?.Invoke();
    }
    private void PlayButtonClickSound()
    {
        SoundManager.Instance.Play2D(ESound.Effect, "Button");
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
    }
}
