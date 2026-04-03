using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static Define;
public class UI_MagazineReviewPopup : UI_UGUI, IUI_Popup, IClickableUI
{
    enum GameObjects
    {
        EvaluationTextFrame1,
        EvaluationTextFrame2,
        EvaluationTextFrame3,
        EvaluationTextFrame4,
    }
    enum Buttons
    {
        Click,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNameText,

        //Content
        EvaluationText1,
        EvaluationText2,
        EvaluationText3,
        EvaluationText4,

        EvaluationScoreText1,
        EvaluationScoreText2,
        EvaluationScoreText3,
        EvaluationScoreText4,

        //SubBottom
        SubBottomText,
    }
    enum Images
    {
        //Content
        EvaluatorsImage1,
        EvaluatorsImage2,
        EvaluatorsImage3,
        EvaluatorsImage4,
    }
    private int _totalScore = 0;
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.Click).onClick.AddListener(() => OnClickButton());
    }
    public void SetInfo()
    {
        Init();
        UpdateContent();
        CoroutineManager.Instance.StartCoroutine(CoEvaluationProcess());
    }
    private void UpdateContent()
    {
        GetText((int)Texts.MainTitleText).text = "잡지 리뷰";
        GetText((int)Texts.SubMiddleNameText).text = GameDevManager.Instance.CurrentGameTitle;
        UpdateTotalScore();
    }
    private void UpdateTotalScore()
    {
        GetText((int)Texts.SubBottomText).text = $"합계 {_totalScore}점입니다.";
    }
    private IEnumerator CoEvaluationProcess()
    {
        // 평가 시작 로직
        for (int i = 0; i < 4; i++)
        {
            int score = MagazineManager.Instance.GetEvaluationScore((EQualityType)i);
            yield return new WaitForSecondsRealtime(2f);

            GetObject((int)GameObjects.EvaluationTextFrame1 + i).SetActive(true);

            string dialogue = "버그다냥!"; // 기본값
            if (DataManager.Instance.EvaluationScriptDict.TryGetValue(score, out var dialogueList)
                && dialogueList != null && dialogueList.Count > 0)
            {
                // 리스트에서 랜덤하게 하나 선택
                int randomIndex = UnityEngine.Random.Range(0, dialogueList.Count);
                dialogue = dialogueList[randomIndex].Dialogue;
            }

            GetText((int)Texts.EvaluationText1 + i).text = dialogue;
            GetText((int)Texts.EvaluationScoreText1 + i).text = score.ToString();
            _totalScore += score;
            UpdateTotalScore();
        }

        yield return new WaitForSecondsRealtime(1f);

        GameDevManager.Instance.CurrentEvaluationScore = _totalScore;
        SetClickInteractable(true);
    }
    private void Init()
    {
        SetClickInteractable(false);
        _totalScore = 0;

        for (int i = 0; i < 4; i++)
        {
            GetObject((int)GameObjects.EvaluationTextFrame1 + i).SetActive(false);
        }
    }
    private void SetClickInteractable(bool isInteractable)
    {
        GetButton((int)Buttons.Click).interactable = isInteractable;
    }
    private void OnClickButton()
    {
        UIManager.Instance.ClosePopupUI();

        int year = GameManager.Instance.Year;
        GameBalanceConfig config = DataManager.Instance.GameBalanceConfig;

        // 판매량 계산 (평가 점수 기반)
        int salesRevenue = config.CalculateFinalIncome(year, _totalScore);
        int salesCount = salesRevenue / config.GamePrice;

        // 게임 판매 결과 데이터 생성
        var salesData = new GameSalesResultData(
            gameTitle: GameDevManager.Instance.CurrentGameTitle,
            salesCount: salesCount,
            salesRevenue: salesRevenue
        );

        // 결과 팝업 표시
        var resultPopup = UIManager.Instance.ShowPopupUI<UI_ResultsReportPopup>();
        resultPopup.SetInfo(salesData, () => { GameDevManager.Instance.AdvanceToNextStage(); });
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
