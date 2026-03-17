using System;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using static Define;

public class UI_EndingVideoPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        VideoPanel,
        VideoRawImage
    }
    enum Buttons
    {
        SkipButton
    }
    enum Images
    {
        
    }

    private VideoPlayer _videoPlayer;
    private RawImage _rawImage;
    private Action _onVideoFinished;
    private RenderTexture _renderTexture;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindImages(typeof(Images));

        // RawImage 가져오기 (한 번만)
        _rawImage = GetObject((int)GameObjects.VideoRawImage).GetComponent<RawImage>();

        // VideoPlayer 설정 (한 번만)
        GameObject videoPanel = GetObject((int)GameObjects.VideoPanel);
        _videoPlayer = videoPanel.GetOrAddComponent<VideoPlayer>();

        // VideoPlayer 기본 설정 (한 번만)
        _videoPlayer.playOnAwake = false;
        _videoPlayer.isLooping = false;
        _videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        _videoPlayer.aspectRatio = VideoAspectRatio.FitInside;

        // Skip 버튼 리스너 (한 번만)
        GetButton((int)Buttons.SkipButton).onClick.AddListener(OnSkipButtonClicked);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // RenderTexture 생성 (팝업 열릴 때마다)
        _renderTexture = new RenderTexture(1080, 2280, 0);
        
        // VideoPlayer에 RenderTexture 설정
        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        _videoPlayer.targetTexture = _renderTexture;
        _rawImage.texture = _renderTexture;

        // 영상 종료 콜백 등록 (팝업 열릴 때마다)
        _videoPlayer.loopPointReached += OnVideoFinished;

        Debug.Log("[UI_EndingVideoPopup] Popup enabled, RenderTexture created");
    }

    /// <summary>
    /// 엔딩 영상 재생
    /// </summary>
    public void PlayVideo(EEndingType endingType, Action onFinished)
    {
        Debug.Log($"[UI_EndingVideoPopup] PlayVideo called with {endingType}");
        _onVideoFinished = onFinished;

        // 엔딩 타입에 따라 비디오 경로 설정
        string videoPath = GetVideoPath(endingType);
        string fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoPath);

        Debug.Log($"[UI_EndingVideoPopup] Video path: {fullPath}");
        
        // 파일 존재 확인
        if (!System.IO.File.Exists(fullPath))
        {
            Debug.LogError($"[UI_EndingVideoPopup] Video file not found: {fullPath}");
            FinishVideo();
            return;
        }

        _videoPlayer.url = fullPath;
        
        // errorReceived 이벤트도 등록
        _videoPlayer.errorReceived += OnVideoError;
        
        _videoPlayer.Prepare();
        _videoPlayer.prepareCompleted += OnPrepareCompleted;
    }

    private void OnPrepareCompleted(VideoPlayer source)
    {
        Debug.Log($"[UI_EndingVideoPopup] Video prepared, duration: {source.length}s");
        source.Play();
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"[UI_EndingVideoPopup] Video error: {message}");
        FinishVideo();
    }

    /// <summary>
    /// 엔딩 타입별 비디오 경로 반환
    /// </summary>
    private string GetVideoPath(EEndingType endingType)
    {
        return endingType switch
        {
            EEndingType.GameStart => "Videos/GameStart.mp4",
            EEndingType.Starvation => "Videos/Ending_Starvation.mp4",
            EEndingType.Exposed => "Videos/Ending_Exposed.mp4",
            EEndingType.Serum => "Videos/Ending_Serum.mp4",
            _ => "Videos/Ending_Default.mp4"
        };
    }

    /// <summary>
    /// 영상 재생 완료 콜백
    /// </summary>
    private void OnVideoFinished(VideoPlayer videoPlayer)
    {
        FinishVideo();
    }

    /// <summary>
    /// Skip 버튼 클릭 시
    /// </summary>
    private void OnSkipButtonClicked()
    {
        _videoPlayer.Stop();
        FinishVideo();
    }

    /// <summary>
    /// 영상 종료 처리
    /// </summary>
    private void FinishVideo()
    {
        _onVideoFinished?.Invoke();
        //UIManager.Instance.ClosePopupUI();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // 이벤트 해제 (팝업 닫힐 때마다)
        if (_videoPlayer != null)
        {
            _videoPlayer.loopPointReached -= OnVideoFinished;
            _videoPlayer.prepareCompleted -= OnPrepareCompleted;
            _videoPlayer.errorReceived -= OnVideoError;
            _videoPlayer.Stop(); // 재생 중이면 정지
        }

        // RenderTexture 해제 (팝업 닫힐 때마다)
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
            _renderTexture = null;
        }

        Debug.Log("[UI_EndingVideoPopup] Popup disabled, RenderTexture released");
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
    }
}
