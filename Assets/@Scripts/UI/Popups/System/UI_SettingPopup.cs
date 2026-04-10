using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_SettingPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //Content   
        Slider1,
        Slider2,
    }
    enum Buttons
    {
        //BG
        BG,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //Content
        SettingNameText1,
        SettingNameText2,
    }
    enum Images
    {

    }

    private Slider _bgmSlider;
    private Slider _sfxSlider;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.BG).onClick.AddListener(() => UIManager.Instance.ClosePopupUI());
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        InitializeSliders();

        RefreshUI();
    }

    private void InitializeSliders()
    {
        // Slider 컴포넌트 가져오기
        _bgmSlider = GetObject((int)GameObjects.Slider1).GetComponent<Slider>();
        _sfxSlider = GetObject((int)GameObjects.Slider2).GetComponent<Slider>();

        if (_bgmSlider == null || _sfxSlider == null)
        {
            Debug.LogError("[UI_SettingPopup] Slider component not found!");
            return;
        }

        // Slider 범위 설정 (0~1)
        _bgmSlider.minValue = 0f;
        _bgmSlider.maxValue = 1f;
        _sfxSlider.minValue = 0f;
        _sfxSlider.maxValue = 1f;

        // 이벤트 리스너 등록
        _bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        _sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

        // 초기 볼륨 값으로 설정
        _bgmSlider.value = SoundManager.Instance.GetVolume(ESound.Bgm);
        _sfxSlider.value = SoundManager.Instance.GetVolume(ESound.Effect);
    }

    /// <summary>
    /// BGM 볼륨 변경 시 호출
    /// </summary>
    private void OnBgmVolumeChanged(float value)
    {
        SoundManager.Instance.SetVolume(ESound.Bgm, value);
    }

    /// <summary>
    /// SFX 볼륨 변경 시 호출
    /// </summary>
    private void OnSfxVolumeChanged(float value)
    {
        SoundManager.Instance.SetVolume(ESound.Effect, value);
    }

    private void UpdateContent()
    {
        GetText((int)Texts.MainTitleText).text = "설정";
        GetText((int)Texts.SettingNameText1).text = "배경음";
        GetText((int)Texts.SettingNameText2).text = "효과음";
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        // 슬라이더가 초기화되어 있으면 현재 볼륨으로 업데이트
        if (_bgmSlider != null)
        {
            _bgmSlider.value = SoundManager.Instance.GetVolume(ESound.Bgm);
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.value = SoundManager.Instance.GetVolume(ESound.Effect);
        }

        UpdateContent();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // 메모리 누수 방지를 위해 리스너 제거
        if (_bgmSlider != null)
        {
            _bgmSlider.onValueChanged.RemoveListener(OnBgmVolumeChanged);
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
        }
    }
}
