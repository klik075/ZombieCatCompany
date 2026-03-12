using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 교육 시스템을 관리하는 매니저
/// UI_MemberEducationMethodsPopup과 UI_MemberEducationCompletionPopup에서 사용하는 교육 관련 기능을 제공
/// </summary>
public class EducationManager : Singleton<EducationManager>
{
    private const int ITEMS_PER_PAGE = 5;
    private bool _isDataLoaded = false;
    // 교육 데이터 캐시
    private List<EducationData> _cachedEducations;
    public int EducationCount { get { return _cachedEducations?.Count ?? 0; } }
    public EducationData SelectedEducation { get; set; }

    private void Awake()
    {
        LoadEducationData();
    }

    private void LoadEducationData()
    {
        if (DataManager.Instance.EducationDict != null)
        {
            _cachedEducations = DataManager.Instance.EducationDict.Values.ToList();
            _cachedEducations.Sort((a, b) => a.EducationId.CompareTo(b.EducationId));
            _isDataLoaded = true;
        }
        else
        {
            _cachedEducations = new List<EducationData>();
            Debug.LogWarning("EducationManager: EducationDict is null or not loaded yet");
        }
    }

    #region Public API - 교육 데이터 접근

    /// <summary>
    /// 모든 교육 데이터를 반환 (정렬된 복사본)
    /// </summary>
    public List<EducationData> GetAllEducations()
    {
        if (!_isDataLoaded)
        {
            LoadEducationData();
        }
        
        return new List<EducationData>(_cachedEducations ?? new List<EducationData>());
    }

    /// <summary>
    /// 페이지별로 교육 데이터를 반환 (UI_MemberEducationMethodsPopup에서 사용)
    /// </summary>
    public List<EducationData> GetEducationsForPage(int pageIndex, int itemsPerPage = ITEMS_PER_PAGE)
    {
        var allEducations = GetAllEducations();
        int startIndex = pageIndex * itemsPerPage;
        int count = Mathf.Min(itemsPerPage, allEducations.Count - startIndex);
        
        if (startIndex >= allEducations.Count || count <= 0)
            return new List<EducationData>();
        
        return allEducations.GetRange(startIndex, count);
    }

    /// <summary>
    /// 총 페이지 수 계산
    /// </summary>
    public int GetTotalPages(int itemsPerPage = ITEMS_PER_PAGE)
    {
        return Mathf.CeilToInt((float)EducationCount / itemsPerPage);
    }

    /// <summary>
    /// ID로 특정 교육 데이터 검색
    /// </summary>
    public EducationData GetEducationById(int educationId)
    {
        return GetAllEducations().FirstOrDefault(e => e.EducationId == educationId);
    }
    #endregion

    #region Public API - 교육 가능 여부 체크

    /// <summary>
    /// 교육 비용을 감당할 수 있는지 체크
    /// </summary>
    public bool CanAffordEducation()
    {
        if (SelectedEducation == null) 
            return false;

        return GameManager.Instance.Gold >= SelectedEducation.Cost;
    }

    /// <summary>
    /// 멤버가 교육을 받을 수 있는 상태인지 체크
    /// </summary>
    public bool CanReceiveEducation()
    {
        Member player = MemberManager.Instance.SelectedMember;

        if (player == null || player?.CurrentMemberData == null) 
            return false;

        return !player.IsDispatched;
    }

    /// <summary>
    /// 교육이 가능한지 종합적으로 체크
    /// </summary>
    public bool CanExecuteEducation()
    {
        return CanReceiveEducation() && CanAffordEducation();
    }
    #endregion
}
