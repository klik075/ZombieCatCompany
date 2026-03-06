using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_DefensePanel : UI_UGUI
{
    enum GameObjects
    {
        Slider,
    }
    enum Buttons
    {
        SpeedButton,
    }
    enum Texts
    {
        SpeedButtonText,
        RemainingPercentageText,
        UserCatText
    }

    private Slider _progressSlider;
    private bool _waveCompleted = false;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.SpeedButton).onClick.AddListener(OnClickSpeedButton);
        _progressSlider = GetObject((int)GameObjects.Slider).GetComponent<Slider>();
        
        if (_progressSlider != null)
        {
            _progressSlider.minValue = 0f;
            _progressSlider.maxValue = 100f;
            _progressSlider.value = 0f;
        }

        gameObject.SetActive(false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        EventManager.Instance.AddEvent(EEventType.DefenseProgressChanged, RefreshUI);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        EventManager.Instance.RemoveEvent(EEventType.DefenseProgressChanged, RefreshUI);

        Time.timeScale = 1f; // 속도 초기화
        _waveCompleted = false; // 플래그 초기화
    }

    public void Init()
    {
        _waveCompleted = false;
        
        // 디펜스 시작
        DefenseManager.Instance.StartDefense();
        
        // 초기 UI 갱신
        RefreshUI();
        
        Debug.Log("[UI_DefensePanel] Defense started and UI initialized");
    }

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        // 웨이브 완료 체크만 수행 (프레임당 1회, 가벼운 연산)
        if (!_waveCompleted && DefenseManager.Instance.IsWaveCompleted())
        {
            _waveCompleted = true;
            OnWaveCompleted();
        }
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

        // 진행도 가져오기
        float progressPercentage = DefenseManager.Instance.GetProgressPercentage();
        float remainingPercentage = DefenseManager.Instance.GetRemainingPercentage();
        
        int totalCats = DefenseManager.Instance.GetTotalCatsCount();
        int remainingCats = DefenseManager.Instance.GetRemainingCatsCount();
        int currentWave = DefenseManager.Instance.GetCurrentWave();

        // Slider 업데이트
        if (_progressSlider != null)
        {
            _progressSlider.value = progressPercentage;
        }

        // 텍스트 업데이트
        GetText((int)Texts.RemainingPercentageText).text = $"{remainingPercentage:F0}%";
        GetText((int)Texts.UserCatText).text = $"화가난 유저 고양이 무리 {remainingCats}/{totalCats}";

        if (Time.timeScale > 0f)
        {
            GetText((int)Texts.SpeedButtonText).text = $"배속 {Time.timeScale:F0}x";
        }
    }

    private void OnWaveCompleted()
    {
        Debug.Log("[UI_DefensePanel] Wave completed!");
        
        // 마지막 UI 갱신 (0/10, 100% 표시)
        RefreshUI();
        
        // 잠시 후 패널 닫기
        //Invoke(nameof(ClosePanel), 1f);
    }

    public void OnClickSpeedButton()
    {
        // 게임 속도 변경 (1x → 2x → 1x)
        if (Time.timeScale == 1f)
            Time.timeScale = 2f;
        else if (Time.timeScale == 2f)
            Time.timeScale = 1f;
        else
            Time.timeScale = 1f;

        RefreshUI();
    }
}
