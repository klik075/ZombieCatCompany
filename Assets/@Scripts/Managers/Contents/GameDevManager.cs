using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static Define;
using System;

[System.Serializable]
public class GameDevProjectData
{
    public string gameTitle;
    public EGameDevType gameDevType;
    public int progress;
    public EGenreType selectedGenre;
    public EContentType selectedContent;
    public int EvaluationScore;

    // 품질 점수들
    public int funScore;
    public int nyangScore;
    public int graphicsScore;
    public int soundScore;
    public int bugScore;
    
    public GameDevProjectData()
    {
        gameDevType = EGameDevType.None;
        progress = 0;
        selectedGenre = EGenreType.ActionGame;
        selectedContent = EContentType.Box;
        EvaluationScore = 0;

        funScore = 0;
        nyangScore = 0;
        graphicsScore = 0;
        soundScore = 0;
        bugScore = 0;
    }

    public int GetTotalQualityScore()
    {
        return funScore + nyangScore + graphicsScore + soundScore;
    }
}
public class WorkResult
{
    public int tries;
    public EQualityType mainQuality;
    public Dictionary<EQualityType, int> gainedScores = new Dictionary<EQualityType, int>();
    public List<EQualityType> qualitySequence = new List<EQualityType>(); // 품질이 획득된 순서
}
public class GameDevManager : Singleton<GameDevManager>
{
    // 현재 프로젝트 - 모든 게임 개발 상태를 여기서 관리
    private GameDevProjectData _currentProject;
    public GameDevProjectData CurrentProject => _currentProject;

    // GameDevProjectData를 통한 통합 접근
    public EGameDevType CurrentGameDevType 
    { 
        get => _currentProject?.gameDevType ?? EGameDevType.None;
        private set 
        {
            if (_currentProject != null)
            {
                if (_currentProject.gameDevType == value)
                    return;

                _currentProject.gameDevType = value;
                EventManager.Instance.TriggerEvent(EEventType.GameDevStateChanged);
            }
        }
    }

    public int Progress 
    {
        get => _currentProject?.progress ?? 0;
        set 
        {
            if (_currentProject != null)
            { 
                _currentProject.progress = Mathf.Clamp(value, 0, 100);
                EventManager.Instance.TriggerEvent(EEventType.GameDevProgressChanged);
            }
        }
    }

    public string CurrentGameTitle 
    {
        get
        {
            if (GameManager.Instance.GameState == EGameState.Night)
                return "신규 개발 없음";

            return _currentProject?.gameTitle ?? $"{GameManager.Instance.Year}번째 게임";
        }
        set 
        {
            if (_currentProject != null)
            { 
                _currentProject.gameTitle = value;
                EventManager.Instance.TriggerEvent(EEventType.NewDevTitleChanged);
            }
        }
    }

    // 현재 선택된 장르와 콘텐츠는 프로젝트에서 가져오되, 캐시된 데이터로 반환
    public GenreData CurrentGenreData 
    {
        get 
        {
            if (_currentProject != null)
                return GetGenreData(_currentProject.selectedGenre);
            
            // 프로젝트가 없을 때 기본값
            return GetGenreData(EGenreType.ActionGame);
        }
    }

    public ContentData CurrentContentData 
    {
        get 
        {
            if (_currentProject != null)
                return GetContentData(_currentProject.selectedContent);
            
            // 프로젝트가 없을 때 기본값
            return GetContentData(EContentType.Box);
        }
    }
    public int CurrentEvaluationScore
    {
        get
        {
            if (_currentProject != null)
                return _currentProject.EvaluationScore;

            return 0;
        }
        set
        {
            if (_currentProject != null)
            {
                _currentProject.EvaluationScore = Mathf.Clamp(value,4, 40);
            }
        }
    }
    // 선택된 장르와 콘텐츠 기반 시너지
    public ESynergyType CurrentSynergy { get { return GetSynergyType(CurrentGenreData.GenreType, CurrentContentData.ContentType); } }

    // 캐시된 데이터
    private Dictionary<int, GenreData> _genreDataCache;
    private Dictionary<int, ContentData> _contentDataCache;
    private Dictionary<int, List<SynergyData>> _synergyDataCache;
    private float _progressIncreaseDuration = 4.0f; // Progress 증가 지속 시간

    private void Awake()
    {
        CacheGameDevData();

        EventManager.Instance.AddEvent(EEventType.GameDevStateChanged, OnGameDevStateChanged);
        EventManager.Instance.AddEvent(EEventType.WorkCompleted, OnMainWorkCompleted);
    }
    private void OnDestroy()
    {
        EventManager.Instance.RemoveEvent(EEventType.GameDevStateChanged, OnGameDevStateChanged);
        EventManager.Instance.RemoveEvent(EEventType.WorkCompleted, OnMainWorkCompleted);
    }

    #region 데이터 캐싱

    /// <summary>
    /// DataManager에서 장르, 콘텐츠, 시너지 데이터 캐싱
    /// </summary>
    private void CacheGameDevData()
    {
        _genreDataCache = DataManager.Instance.GenreDict;
        _contentDataCache = DataManager.Instance.ContentDict;
        _synergyDataCache = DataManager.Instance.SynergyDict;
    }

    #endregion

    #region 장르 관리

    /// <summary>
    /// 장르 선택
    /// </summary>
    /// <param name="genreType">선택할 장르 타입</param>
    public void SelectGenre(EGenreType genreType)
    {
        if (_currentProject != null)
        {
            _currentProject.selectedGenre = genreType;
        }
        Debug.Log($"Genre selected: {genreType}");
    }

    /// <summary>
    /// 장르 타입으로 장르 데이터 가져오기
    /// </summary>
    /// <param name="genreType">장르 타입</param>
    /// <returns>장르 데이터 또는 null</returns>
    public GenreData GetGenreData(EGenreType genreType)
    {
        if (_genreDataCache == null) 
            return null;

        foreach (var kvp in _genreDataCache)
        {
            if (kvp.Value.GenreType == genreType)
                return kvp.Value;
        }

        Debug.LogWarning($"GenreData not found for type: {genreType}");
        return null;
    }

    #endregion

    #region 콘텐츠 관리

    /// <summary>
    /// 콘텐츠 선택
    /// </summary>
    /// <param name="contentType">선택할 콘텐츠 타입</param>
    public void SelectContent(EContentType contentType)
    {
        if (_currentProject != null)
        {
            _currentProject.selectedContent = contentType;
        }
        Debug.Log($"Content selected: {contentType}");
    }

    /// <summary>
    /// 콘텐츠 타입으로 콘텐츠 데이터 가져오기
    /// </summary>
    /// <param name="contentType">콘텐츠 타입</param>
    /// <returns>콘텐츠 데이터 또는 null</returns>
    public ContentData GetContentData(EContentType contentType)
    {
        if (_contentDataCache == null) 
            return null;

        foreach (var kvp in _contentDataCache)
        {
            if (kvp.Value.ContentType == contentType)
                return kvp.Value;
        }

        Debug.LogWarning($"ContentData not found for type: {contentType}");
        return null;
    }

    #endregion

    #region 시너지 관리
    /// <summary>
    /// 장르와 콘텐츠 조합의 시너지 타입 가져오기
    /// </summary>
    public ESynergyType GetSynergyType(EGenreType genreType, EContentType contentType)
    {
        GenreData genreData = GetGenreData(genreType);
        ContentData contentData = GetContentData(contentType);

        if (genreData == null || contentData == null)
            return ESynergyType.Normal;

        // 장르 ID로 시너지 데이터 찾기
        if (_synergyDataCache != null && _synergyDataCache.TryGetValue(genreData.GenreId, out List<SynergyData> synergyList))
        {
            foreach (SynergyData synergy in synergyList)
            {
                if (synergy.ContentId == contentData.ContentId)
                {
                    return synergy.SynergyType;
                }
            }
        }

        return ESynergyType.Normal; // 기본값
    }
    public SynergyData GetSynergyData(EGenreType genreType, EContentType contentType)
    {
        GenreData genreData = GetGenreData(genreType);
        ContentData contentData = GetContentData(contentType);
        if (genreData == null || contentData == null)
            return null;
        // 장르 ID로 시너지 데이터 찾기
        if (_synergyDataCache != null && _synergyDataCache.TryGetValue(genreData.GenreId, out List<SynergyData> synergyList))
        {
            foreach (SynergyData synergy in synergyList)
            {
                if (synergy.ContentId == contentData.ContentId)
                {
                    return synergy;
                }
            }
        }

        return new SynergyData { GenreId = genreData.GenreId, ContentId = contentData.ContentId, SynergyType = ESynergyType.Normal }; // 기본값
    }

    #endregion

    #region 비용 계산

    /// <summary>
    /// 현재 선택된 장르와 콘텐츠의 총 개발 비용 계산
    /// </summary>
    /// <returns>총 개발 비용</returns>
    public int GetTotalDevelopmentCost()
    {
        int genreCost = CurrentGenreData?.Cost ?? 0;
        int contentCost = CurrentContentData?.Cost ?? 0;
        
        return genreCost + contentCost;
    }

    /// <summary>
    /// 개발 가능한지 자금 확인
    /// </summary>
    /// <returns>개발 가능하면 true</returns>
    public bool CanAffordDevelopment()
    {
        int totalCost = GetTotalDevelopmentCost();
        return GameManager.Instance.Gold >= totalCost;
    }

    #endregion

    #region 저장 및 복원
    /// <summary>
    /// 게임 개발 관련 저장 데이터 생성
    /// </summary>
    public GameDevProjectData GetGameDevProjectData()
    {
        if (_currentProject == null)
            return new GameDevProjectData();

        GameDevProjectData saveData = new GameDevProjectData()
        {
            gameTitle = _currentProject.gameTitle,
            gameDevType = _currentProject.gameDevType,
            progress = _currentProject.progress,
            selectedGenre = _currentProject.selectedGenre,
            selectedContent = _currentProject.selectedContent,

            EvaluationScore = _currentProject.EvaluationScore,
            funScore = _currentProject.funScore,
            graphicsScore = _currentProject.graphicsScore,
            nyangScore = _currentProject.nyangScore,
            soundScore = _currentProject.soundScore,
            bugScore = _currentProject.bugScore,
        };

        return saveData;
    }

    /// <summary>
    /// 저장된 데이터로부터 게임 개발 상태 복원
    /// </summary>
    public void LoadFromSaveData(GameDevProjectData saveData)
    {
        if (saveData == null)
        {
            Debug.LogWarning("GameDevSaveData is null! Using default values.");
            InitNewProject();
            return;
        }

        // UserData의 GameDevProjectData 참조를 사용
        _currentProject = GameManager.Instance.UserData.MyGameData.NightData.GameDevProjectData;

        // 핵심 수정: saveData의 값을 _currentProject에 복사
        _currentProject.gameTitle = saveData.gameTitle;
        _currentProject.gameDevType = saveData.gameDevType;
        _currentProject.progress = saveData.progress;
        _currentProject.selectedGenre = saveData.selectedGenre;
        _currentProject.selectedContent = saveData.selectedContent;

        // ⭐ 평가 점수 복사
        _currentProject.EvaluationScore = saveData.EvaluationScore;

        // 품질 점수 복사
        _currentProject.funScore = saveData.funScore;
        _currentProject.nyangScore = saveData.nyangScore;
        _currentProject.graphicsScore = saveData.graphicsScore;
        _currentProject.soundScore = saveData.soundScore;
        _currentProject.bugScore = saveData.bugScore;

        EventManager.Instance.TriggerEvent(EEventType.GameDevStateChanged);
        Debug.Log($"GameDevData : gameDevType = {_currentProject.gameDevType}");
    }
    #endregion

    #region 페이징 데이터 제공

    /// <summary>
    /// 장르 데이터의 총 개수 반환
    /// </summary>
    public int GetGenreDataCount()
    {
        return _genreDataCache?.Count ?? 0;
    }

    /// <summary>
    /// 콘텐츠 데이터의 총 개수 반환
    /// </summary>
    public int GetContentDataCount()
    {
        return _contentDataCache?.Count ?? 0;
    }

    /// <summary>
    /// 페이지별 장르 데이터 가져오기
    /// </summary>
    public List<GenreData> GetGenreDataByPage(int page, int itemsPerPage)
    {
        List<GenreData> result = new List<GenreData>();
        
        if (_genreDataCache == null || _genreDataCache.Count == 0)
            return result;

        List<GenreData> allGenres = new List<GenreData>(_genreDataCache.Values);
        int startIndex = page * itemsPerPage;
        int endIndex = Mathf.Min(startIndex + itemsPerPage, allGenres.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            result.Add(allGenres[i]);
        }

        return result;
    }

    /// <summary>
    /// 페이지별 콘텐츠 데이터 가져오기
    /// </summary>
    public List<ContentData> GetContentDataByPage(int page, int itemsPerPage)
    {
        List<ContentData> result = new List<ContentData>();
        
        if (_contentDataCache == null || _contentDataCache.Count == 0)
            return result;

        List<ContentData> allContents = new List<ContentData>(_contentDataCache.Values);
        int startIndex = page * itemsPerPage;
        int endIndex = Mathf.Min(startIndex + itemsPerPage, allContents.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            result.Add(allContents[i]);
        }

        return result;
    }

    #endregion

    #region 프로젝트 관리
    public void InitNewProject()
    {
        // UserData의 GameDevProjectData 참조를 직접 사용
        _currentProject = GameManager.Instance.UserData.MyGameData.NightData.GameDevProjectData;

        // 초기화
        _currentProject.gameDevType = EGameDevType.None;
        _currentProject.progress = 0;
        _currentProject.selectedGenre = EGenreType.ActionGame;
        _currentProject.selectedContent = EContentType.Box;
        _currentProject.EvaluationScore = 0;
        _currentProject.funScore = 0;
        _currentProject.nyangScore = 0;
        _currentProject.graphicsScore = 0;
        _currentProject.soundScore = 0;
        _currentProject.bugScore = 0;
    }
    /// <summary>
    /// 새 프로젝트 시작
    /// </summary>
    public void StartNewProject()
    {
        GameDevManager.Instance.InitNewProject();
        GameManager.Instance.GameState = EGameState.Dev;
        CurrentGameDevType = EGameDevType.Scenario;
        Progress = 0;
    }
    private void OnGameDevStateChanged()
    {
        switch (CurrentGameDevType)
        {
            case EGameDevType.None:
                break;
            case EGameDevType.Scenario:
                UI_ChatPopup scenarioPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
                scenarioPopup.SetInfo(
                    MemberManager.MAIN_CHARACTER_ID,
                    MessageManager.Instance.GetMessageScript(EMessageType.SelectPlanner).Contents,
                    new string[] { $"{GameManager.Instance.Year}",$"{GenreData.GenreToString(CurrentGenreData.GenreType)}", $"{ContentData.ContentToString(CurrentContentData.ContentType)}" },
                    OnClickChatPopup
                    );
                break;
            case EGameDevType.Graphics:
                UI_ChatPopup graphicsPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
                graphicsPopup.SetInfo(
                    MemberManager.MAIN_CHARACTER_ID,
                    MessageManager.Instance.GetMessageScript(EMessageType.SelectDesigner).Contents,
                    action : OnClickChatPopup
                    );
                break;
            case EGameDevType.Sound:
                UI_ChatPopup soundPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
                soundPopup.SetInfo(
                    MemberManager.MAIN_CHARACTER_ID,
                    MessageManager.Instance.GetMessageScript(EMessageType.SelectSoundWriter).Contents,
                    action: OnClickChatPopup
                    );
                break;
            case EGameDevType.Debug:
                UI_ChatPopup debugPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
                debugPopup.SetInfo(
                    MemberManager.MAIN_CHARACTER_ID,
                    MessageManager.Instance.GetMessageScript(EMessageType.StartDebugging).Contents,
                    action: OnClickChatPopup
                    );
                break;
            case EGameDevType.Complete:
                UI_ChatPopup completePopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
                completePopup.SetInfo(
                    MemberManager.MAIN_CHARACTER_ID,
                    MessageManager.Instance.GetMessageScript(EMessageType.CompleteGameDev).Contents,
                    new string[] { $"{GameManager.Instance.Year}" },
                    action: OnClickChatPopup
                    );
                break;
            case EGameDevType.EndDev:
                if (GameManager.Instance.GameMode == EGameMode.Purchase)
                { 
                    GameManager.Instance.GameState = EGameState.FoodPurchase;
                }
                else
                {
                    PurchaseManager.Instance.ExitAllMembersAndTransition();
                }
                break;
            default:
                break;
        }
    }
    private void OnClickChatPopup()
    {
        switch (CurrentGameDevType)
        {
            case EGameDevType.None:
                break;
            case EGameDevType.Scenario:
            case EGameDevType.Graphics:
            case EGameDevType.Sound:
                UI_GameDevMemberSelectionPopup selectionPopup = UIManager.Instance.ShowPopupUI<UI_GameDevMemberSelectionPopup>();
                selectionPopup.SetInfo();
                break;
            case EGameDevType.Debug:
                StartDebug();
                break;
            case EGameDevType.Complete:
                UI_GameDevCompletionPopup completionPopup = UIManager.Instance.ShowPopupUI<UI_GameDevCompletionPopup>();
                completionPopup.SetInfo();
                break;
            default:
                break;
        }
    }
    public void StartDebug()
    {
        QualityManager.Instance.StartIndividualWork();
    }
    /// <summary>
    /// 작업 결과 계산 (UI에서 사용)
    /// </summary>
    public WorkResult CalculateWorkResult(MemberData worker)
    {
        var result = new WorkResult();
        EQualityType mainQuality;

        switch (CurrentGameDevType)
        {
            case EGameDevType.Scenario:
                mainQuality = EQualityType.Nyang;
                break;
            case EGameDevType.Graphics:
                mainQuality = EQualityType.Graphics;
                break;
            case EGameDevType.Sound:
                mainQuality = EQualityType.Sound;
                break;
            default:
                mainQuality = EQualityType.Fun;
                break;
        }

        // 시도 횟수 계산
        int mainAbility = GetAbilityByQuality(worker, mainQuality);
        int minTries = 1 + (mainAbility / 5);
        int offset = GetOffset(worker, mainQuality);
        int tries = Mathf.Max(minTries, 1) + UnityEngine.Random.Range(0, offset + 1);

        result.tries = tries;
        result.mainQuality = mainQuality;

        // 각 시도에 대해 점수 계산
        for (int i = 0; i < tries; i++)
        {
            var (quality, score) = RollOneScore(mainQuality);
            
            // Bug는 점수 획득 불가
            if (quality == EQualityType.Bug)
                continue;

            if (!result.gainedScores.ContainsKey(quality))
                result.gainedScores[quality] = 0;
            
            result.gainedScores[quality] += score;
            result.qualitySequence.Add(quality); // 순서 기록
        }

        return result;
    }
    /// <summary>
    /// 개인 작업 결과 계산
    /// </summary>
    public WorkResult CalculateIndividualWorkResult(MemberData worker)
    {
        var result = new WorkResult();

        if (CurrentGameDevType == EGameDevType.Debug)
        {
            result.mainQuality = EQualityType.Bug;
            int programmingAbility = GetAbilityByQuality(worker, EQualityType.Bug);
            result.tries = CalculateDebugScore(programmingAbility);
            return result;
        }

        // 일반 모드: 역할별 작업
        EQualityType mainQuality = GetMainQualityByRole(worker);

        // 확률에 따라 실제 획득할 품질 결정
        EQualityType actualQuality = SelectQualityByProbability(mainQuality, isIndividual: true);

        // 획득한 품질에 해당하는 능력치로 점수 계산
        int abilityValue = GetAbilityByQuality(worker, actualQuality);
        int score = CalculateIndividualScore(abilityValue, actualQuality);

        result.mainQuality = actualQuality;
        result.tries = score;
        return result;
    }
    /// <summary>
    /// Debug 모드에서 버그 수정량 계산
    /// </summary>
    private int CalculateDebugScore(int programmingAbility)
    {
        // 프로그래밍 능력치 기반 버그 수정량
        // 능력치 10 → 1~2점, 50 → 5~7점
        int baseScore = Mathf.Max(1, programmingAbility / 10);
        int randomRange = Mathf.Max(1, programmingAbility / 20);

        int score = baseScore + UnityEngine.Random.Range(0, randomRange + 1);

        return Mathf.Clamp(score, 1, 10); // 최소 1, 최대 10
    }
    /// <summary>
    /// 개인 작업 점수 계산 (능력치 기반)
    /// </summary>
    private int CalculateIndividualScore(int abilityValue, EQualityType qualityType)
    {
        // Bug는 발생량 계산 (적을수록 좋음)
        if (qualityType == EQualityType.Bug)
        {
            // 능력치가 낮을수록 Bug 많이 발생
            // Programming 10 → 3~5점, 30 → 2~3점, 50 → 1~2점
            int maxBugScore = Mathf.Max(2, 60 / Mathf.Max(abilityValue, 10));
            int minBugScore = Mathf.Max(1, maxBugScore / 2);

            int bugScore = UnityEngine.Random.Range(minBugScore, maxBugScore + 1);
            return Mathf.Clamp(bugScore, 1, 10);
        }

        // 일반 품질은 능력치 비례 (많을수록 좋음)
        // 능력치 10 → 1점, 30 → 2~3점, 50 → 3~5점
        int baseScore = Mathf.Max(1, abilityValue / 15);
        int randomRange = Mathf.Max(1, abilityValue / 25);

        int score = baseScore + UnityEngine.Random.Range(0, randomRange + 1);

        return Mathf.Clamp(score, 1, 10); // 최소 1, 최대 10
    }
    private EQualityType GetMainQualityByRole(MemberData worker)
    {
        // 직업에 따라 메인 품질 결정
        switch (worker.Role)
        {
            case ERoleType.Boss:
                return EQualityType.Bug;
            case ERoleType.Planner:
                return EQualityType.Nyang;
            case ERoleType.Designer:
                return EQualityType.Graphics;
            case ERoleType.SoundWriter:
                return EQualityType.Sound;
            default:
                return EQualityType.Fun;
        }
    }
    private int GetIndividualTries(MemberData worker, EQualityType mainQuality, bool isDebug = false)
    {
        int ability = Mathf.Max(GetAbilityByQuality(worker, mainQuality), 1);
        int tries = 0;

        if (mainQuality == EQualityType.Bug)
        {
            tries = isDebug ? Mathf.Min((ability / 10) + 1, 10) : Mathf.Min((50 / ability), 10);
            tries = UnityEngine.Random.Range(tries/2, tries + 1);
            return Mathf.Max(tries, 1);
        }

        tries = (ability / 10);
        tries = UnityEngine.Random.Range(tries / 2, tries + 1);
        return Mathf.Max(tries, 1);
    }
    /// <summary>
    /// 한 번의 시도에서 품질과 점수 결정
    /// </summary>
    public (EQualityType quality, int score) RollOneScore(EQualityType mainQuality, bool isIndividual = false)
    {
        // 확률 기반으로 품질 선택
        EQualityType selectedQuality = SelectQualityByProbability(mainQuality, isIndividual);

        // 점수는 항상 1점
        int score = 1;
        
        return (selectedQuality, score);
    }

    /// <summary>
    /// 확률에 따른 품질 선택
    /// Fun: 20%, Main: 70%, 나머지 서브 품질들: 각각 5%
    /// </summary>
    private EQualityType SelectQualityByProbability(EQualityType mainQuality, bool isIndividual = false)
    {
        float randomValue = UnityEngine.Random.value;
        
        // Fun: 20% (0.0 ~ 0.2)
        if (randomValue < 0.2f)
        {
            return EQualityType.Fun;
        }

        if (isIndividual)
        {
            if (mainQuality == EQualityType.Bug)
            {
                if (randomValue < 0.8f)
                {
                    return EQualityType.Bug;
                }
            }
            else
            {
                if (randomValue < 0.3f)
                {
                    return EQualityType.Bug;
                }
            }
        }

        if (randomValue < 0.8f)
        {
            return mainQuality;
        }
        
        // 나머지 10%를 각 서브에 할당
        var (sub1, sub2) = GetSubQualityTypes(mainQuality);
        return randomValue < 0.9f ? sub1 : sub2;
    }
    /// <summary>
    /// 품질 타입에 해당하는 능력치 반환
    /// </summary>
    public int GetAbilityByQuality(MemberData worker, EQualityType quality)
    {
        switch (quality)
        {
            case EQualityType.Fun:
            case EQualityType.Bug:
                return worker.GetAbilityValue(EAbilityType.Programming);
            case EQualityType.Nyang:
                return worker.GetAbilityValue(EAbilityType.Scenario);
            case EQualityType.Graphics:
                return worker.GetAbilityValue(EAbilityType.Graphics);
            case EQualityType.Sound:
                return worker.GetAbilityValue(EAbilityType.Sound);
            default:
                return 0;
        }
    }

    /// <summary>
    /// 오프셋 계산: (메인, 전투력을 제외한 능력치의 합) / 20
    /// </summary>
    public int GetOffset(MemberData worker, EQualityType mainQuality)
    {
        // 전투력을 제외한 모든 능력치 합
        int total = worker.Programming + worker.Scenario + worker.Graphics + worker.Sound;
        
        // 메인 능력치 제외
        int mainAbility = GetAbilityByQuality(worker, mainQuality);
        total -= mainAbility;
        
        return Mathf.Max(0, total / 20);
    }

    /// <summary>
    /// 메인 품질에 따른 서브 품질 2개 반환
    /// </summary>
    public (EQualityType sub1, EQualityType sub2) GetSubQualityTypes(EQualityType mainQuality)
    {
        switch (mainQuality)
        {
            case EQualityType.Fun:
                return (EQualityType.Graphics, EQualityType.Sound);
            case EQualityType.Nyang:
                return (EQualityType.Graphics, EQualityType.Sound);
            case EQualityType.Graphics:
                return (EQualityType.Nyang, EQualityType.Sound);
            case EQualityType.Sound:
                return (EQualityType.Nyang, EQualityType.Graphics);
            case EQualityType.Bug:
                return (EQualityType.Fun, EQualityType.Nyang);
            default:
                return (EQualityType.Graphics, EQualityType.Sound);
        }
    }

    /// <summary>
    /// 품질 점수 추가
    /// </summary>
    public void AddQualityScore(EQualityType qualityType, int score)
    {
        if (_currentProject == null) 
            return;

        switch (qualityType)
        {
            case EQualityType.Fun:
                _currentProject.funScore += score;
                break;
            case EQualityType.Nyang:
                _currentProject.nyangScore += score;
                break;
            case EQualityType.Graphics:
                _currentProject.graphicsScore += score;
                break;
            case EQualityType.Sound:
                _currentProject.soundScore += score;
                break;
            case EQualityType.Bug:
                if (CurrentGameDevType == EGameDevType.Debug)
                {
                    int temp = _currentProject.bugScore - score;
                    _currentProject.bugScore = Mathf.Max(0, temp);
                }
                else
                { 
                    _currentProject.bugScore += score;
                }
                break;
        }
        EventManager.Instance.TriggerEvent(EEventType.QualityChanged);
    }

    /// <summary>
    /// 품질 점수 가져오기
    /// </summary>
    public int GetQualityScore(EQualityType qualityType)
    {
        if (_currentProject == null) 
            return 0;

        switch (qualityType)
        {
            case EQualityType.Fun: return _currentProject.funScore;
            case EQualityType.Nyang: return _currentProject.nyangScore;
            case EQualityType.Graphics: return _currentProject.graphicsScore;
            case EQualityType.Sound: return _currentProject.soundScore;
            case EQualityType.Bug: return _currentProject.bugScore;
            default: return 0;
        }
    }

    #endregion

    #region Progress 관리

    /// <summary>
    /// 작업 완료 이벤트 핸들러
    /// </summary>
    private void OnMainWorkCompleted()
    {
        if (_currentProject == null)
        {
            Debug.LogWarning("No active project for work completion!");
            return;
        }

        Debug.Log($"Work completed for {CurrentGameDevType} stage");

        if (CurrentGameDevType == EGameDevType.Scenario)
            MemberManager.Instance.MoveAllMembersToSeats();

        // 현재 단계에 따른 Progress 증가 시작
        CoroutineManager.Instance.StartCoroutine(CoIncreaseProgressForCurrentStage());
    }

    /// <summary>
    /// 현재 단계에 따라 Progress를 증가시키는 코루틴
    /// </summary>
    private IEnumerator CoIncreaseProgressForCurrentStage()
    {
        int targetProgress = GetTargetProgressForStage(CurrentGameDevType);
        int startProgress = Progress;
        
        Debug.Log($"Starting progress increase: {startProgress} -> {targetProgress}% for {CurrentGameDevType}");
        
        float baseDuration = _progressIncreaseDuration;
        bool startIndividualWork = false;
        
        float acceleratedTime = 0f; // 가속된 시간 누적
        
        while (Progress < targetProgress)
        {
            yield return new WaitWhile(() => Time.timeScale == 0 || !MemberManager.Instance.IsAnyMemberAtSeat());

            if (startIndividualWork == false)
            {
                QualityManager.Instance.StartIndividualWork();
                startIndividualWork = true;
            }

            int sittingCount = MemberManager.Instance.HowManyActiveMemberSitting();
            float memberRatio = sittingCount / (float)MemberManager.MAX_MEMBERS;
            acceleratedTime += Time.deltaTime * memberRatio;
            
            // duration 기준으로 진행도 계산
            float t = Mathf.Clamp01(acceleratedTime / baseDuration);
            
            // 선형 보간으로 Progress 증가
            int newProgress = Mathf.RoundToInt(Mathf.Lerp(startProgress, targetProgress, t));
            Progress = newProgress;
            
            yield return null;
        }
        
        while(QualityManager.Instance.isAnimating)
        {
            yield return null;
        }

        // 최종 목표 Progress 설정
        Progress = targetProgress;
        Debug.Log($"Progress increase completed: {Progress}% for {CurrentGameDevType}");
        AdvanceToNextStage();
    }

    /// <summary>
    /// 현재 단계에 따른 목표 Progress 반환
    /// </summary>
    private int GetTargetProgressForStage(EGameDevType stage)
    {
        return stage switch
        {
            EGameDevType.Scenario => 33,
            EGameDevType.Graphics => 66,
            EGameDevType.Sound => 100,
            _ => Progress // 다른 단계들은 현재 Progress 유지
        };
    }

    /// <summary>
    /// 다음 단계로 전환
    /// </summary>
    public void AdvanceToNextStage()
    {
        EGameDevType nextStage = GetNextStage(CurrentGameDevType);
        
        if (nextStage != CurrentGameDevType)
        {
            Debug.Log($"Advancing from {CurrentGameDevType} to {nextStage}");
            CurrentGameDevType = nextStage;
        }
    }

    /// <summary>
    /// 현재 단계의 다음 단계 반환
    /// </summary>
    private EGameDevType GetNextStage(EGameDevType currentStage)
    {
        return currentStage switch
        {
            EGameDevType.Scenario => EGameDevType.Graphics,
            EGameDevType.Graphics => EGameDevType.Sound,
            EGameDevType.Sound => EGameDevType.Debug,
            EGameDevType.Debug => EGameDevType.Complete,
            EGameDevType.Complete => EGameDevType.EndDev,
            _ => currentStage 
        };
    }

    #endregion

    /// <summary>
    /// 게임 개발 타입을 None으로 초기화 (씬 전환 시 사용)
    /// </summary>
    public void ResetGameDevType()
    {
        if (_currentProject != null)
        {
            CurrentGameDevType = EGameDevType.None;
            Debug.Log("[GameDevManager] GameDevType reset to None");
        }
    }
}

