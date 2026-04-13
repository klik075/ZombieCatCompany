using System;
using UnityEngine;
using static Define;

public class EndingManager : Singleton<EndingManager>
{
    /// <summary>
    /// 새 게임 시작 시 현재 기록 초기화
    /// </summary>
    public void InitializeCurrentRecord()
    {
        var currentRecord = GameManager.Instance.MyGameData.CurrentEndingData;

        currentRecord.GameMode = GameManager.Instance.GameMode;
        currentRecord.CompanyName = GameManager.Instance.CompanyName;
        currentRecord.Year = GameManager.Instance.Year;

        currentRecord.TotalGold = 0;
        currentRecord.ConsumedFood = 0;
        currentRecord.DeadMembersCount = 0;
        currentRecord.KilledCatsCount = 0;
        currentRecord.HiredMembersCount = 0;
        currentRecord.EducationCount = 0;
        currentRecord.DispatchedMembersCount = 0;
        currentRecord.TotalEnhancementLevel = 1;
        currentRecord.EnhancementFailCount = 0;
    }

    /// <summary>
    /// 통계 기록 (게임 진행 중 호출)
    /// </summary>
    public void RecordStat(EEndingStatType statType, int amount = 1)
    {
        var currentRecord = GameManager.Instance.MyGameData.CurrentEndingData;

        switch (statType)
        {
            case EEndingStatType.TotalGold:
                currentRecord.TotalGold += amount;
                Debug.Log($"[EndingManager] Total Gold: +{amount} → {currentRecord.TotalGold}");
                break;
            case EEndingStatType.ConsumedFood:
                currentRecord.ConsumedFood += amount;
                Debug.Log($"[EndingManager] Consumed Food: +{amount} → {currentRecord.ConsumedFood}");
                break;
            case EEndingStatType.DeadMembers:
                currentRecord.DeadMembersCount += amount;
                Debug.Log($"[EndingManager] Dead Members: +{amount} → {currentRecord.DeadMembersCount}");
                break;
            case EEndingStatType.KilledCats:
                currentRecord.KilledCatsCount += amount;
                break;
            case EEndingStatType.HiredMembers:
                currentRecord.HiredMembersCount += amount;
                Debug.Log($"[EndingManager] Hired Members: +{amount} → {currentRecord.HiredMembersCount}");
                break;
            case EEndingStatType.Education:
                currentRecord.EducationCount += amount;
                Debug.Log($"[EndingManager] Education: +{amount} → {currentRecord.EducationCount}");
                break;
            case EEndingStatType.DispatchedMembers:
                currentRecord.DispatchedMembersCount += amount;
                Debug.Log($"[EndingManager] Dispatched Members: +{amount} → {currentRecord.DispatchedMembersCount}");
                break;
            case EEndingStatType.EnhancementLevel:
                currentRecord.TotalEnhancementLevel = amount;
                Debug.Log($"[EndingManager] Enhancement Level: +{amount} → {currentRecord.TotalEnhancementLevel}");
                break;
            case EEndingStatType.EnhancementFail:
                currentRecord.EnhancementFailCount += amount;
                Debug.Log($"[EndingManager] Enhancement Fail: +{amount} → {currentRecord.EnhancementFailCount}");
                break;
        }
    }
    public void TriggerGameStart()
    {
        CoroutineManager.Instance.Run(CoPlayEndingVideo(EEndingType.GameStart));
    }
    /// <summary>
    /// 엔딩 도달 시 호출 (엔딩 타입과 이름을 받아서 기록)
    /// </summary>
    public void TriggerEnding(EEndingType endingType)
    {
        var currentRecord = GameManager.Instance.MyGameData.CurrentEndingData;
        currentRecord.EndingType = endingType;
        currentRecord.Year = GameManager.Instance.Year;
        currentRecord.CompanyName = GameManager.Instance.CompanyName;

        TryUpdateRecord(currentRecord);// 최고 기록 갱신 시도
        SaveManager.Instance.DeleteGameData();

        //엔딩 영상 재생 후 씬 전환
        CoroutineManager.Instance.Run(CoPlayEndingVideo(endingType));
    }

    /// <summary>
    /// 최고 기록 갱신 시도
    /// </summary>
    private void TryUpdateRecord(EndingData newRecord)
    {
        var endingRecords = GameManager.Instance.UserData.EndingRecords;
        
        if (!endingRecords.ContainsKey(newRecord.GameMode))
        {
            Debug.LogError($"[EndingManager] Invalid game mode: {newRecord.GameMode}");
            return;
        }

        int endingIndex = (int)newRecord.EndingType - 1;//1, 2, 3
        int shortestIndex = endingIndex * 2;      // 0, 2, 4
        int longestIndex = endingIndex * 2 + 1;   // 1, 3, 5

        var records = endingRecords[newRecord.GameMode];

        // 최단 기록 갱신 시도
        if (records[shortestIndex] == null || IsBetterRecord(newRecord, records[shortestIndex], ERecordType.Shortest))
        {
            records[shortestIndex] = CloneRecord(newRecord);
            Debug.Log($"[EndingManager] New SHORTEST record for {newRecord.EndingType}: Year {newRecord.Year}");
        }

        // 최장 기록 갱신 시도
        if (records[longestIndex] == null || IsBetterRecord(newRecord, records[longestIndex], ERecordType.Longest))
        {
            records[longestIndex] = CloneRecord(newRecord);
            Debug.Log($"[EndingManager] New LONGEST record for {newRecord.EndingType}: Year {newRecord.Year}");
        }

        SaveManager.Instance.SaveUserData();
    }

    /// <summary>
    /// 기록 비교 (newRecord가 기존 기록보다 좋은지 판정)
    /// </summary>
    private bool IsBetterRecord(EndingData newRecord, EndingData existingRecord, ERecordType recordType)
    {
        if (existingRecord == null)
            return true;

        // 최단 기록: 연차가 적을수록 좋음
        // 최장 기록: 연차가 많을수록 좋음
        if (recordType == ERecordType.Shortest)
        {
            if (newRecord.Year < existingRecord.Year)
                return true;

            if (newRecord.Year > existingRecord.Year)
                return false;
        }
        else // Longest
        {
            if (newRecord.Year > existingRecord.Year)
                return true;

            if (newRecord.Year < existingRecord.Year)
                return false;
        }

        // 연차가 동일하면 총 자금으로 비교
        if (newRecord.TotalGold > existingRecord.TotalGold)
            return true;

        if (newRecord.TotalGold < existingRecord.TotalGold)
            return false;

        if (newRecord.ConsumedFood < existingRecord.ConsumedFood)
            return true;

        if (newRecord.ConsumedFood > existingRecord.ConsumedFood)
            return false;

        // 모든 조건이 동일하면 기존 기록 유지
        return false;
    }
    private System.Collections.IEnumerator CoPlayEndingVideo(EEndingType endingType)
    {
        SoundManager.Instance.Stop(Define.ESound.Bgm);
        UIManager.Instance.CloseAllPopupUI();

        // 한 프레임 대기 (팝업들이 완전히 닫힐 때까지)
        yield return null;

        // 엔딩 영상 팝업 열기
        UI_EndingVideoPopup videoPopup = UIManager.Instance.ShowPopupUI<UI_EndingVideoPopup>();

        yield return null;

        bool videoFinished = false;
        videoPopup.PlayVideo(endingType, () => { videoFinished = true; });

        // 영상이 끝나거나 스킵될 때까지 대기
        while (!videoFinished)
        {
            yield return null;
        }

        Debug.Log("[EndingManager] Video finished, loading lobby scene");

        //GameStart이면 NightScene, 나머지는 MorningScene으로 이동
        if (endingType == EEndingType.GameStart)
        {
            SceneManager.Instance.LoadScene(EScene.NightScene, 0.5f);
        }
        else
        {
            SceneManager.Instance.LoadScene(EScene.LobbyScene, 0.5f);
        }
    }
    /// <summary>
    /// EndingData 복사
    /// </summary>
    private EndingData CloneRecord(EndingData source)
    {
        return new EndingData
        {
            GameMode = source.GameMode,
            EndingType = source.EndingType,
            CompanyName = source.CompanyName,
            Year = source.Year,
            TotalGold = source.TotalGold,
            ConsumedFood = source.ConsumedFood,
            DeadMembersCount = source.DeadMembersCount,
            KilledCatsCount = source.KilledCatsCount,
            HiredMembersCount = source.HiredMembersCount,
            EducationCount = source.EducationCount,
            DispatchedMembersCount = source.DispatchedMembersCount,
            TotalEnhancementLevel = source.TotalEnhancementLevel,
            EnhancementFailCount = source.EnhancementFailCount
        };
    }

    /// <summary>
    /// 특정 엔딩 타입의 최고 기록 가져오기
    /// </summary>
    public EndingData GetRecord(EGameMode gameMode, EEndingType endingType, ERecordType recordType)
    {
        var records = GameManager.Instance.UserData.EndingRecords;
        
        if (!records.ContainsKey(gameMode))
        {
            Debug.LogWarning($"[EndingManager] No records for game mode: {gameMode}");
            return null;
        }

        int index = (int)endingType * 2 + (int)recordType;
        return records[gameMode][index];
    }
}
