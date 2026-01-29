using UnityEngine;
using System.Collections.Generic;
using static Define;

public class UI_GameSelectionPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG
        BG,
    }
    enum Buttons
    {
        //MainTitle
        NextButton,
        PreviousButton,

        //Content
        ContentButton1,
        ContentButton2,
        ContentButton3,
        ContentButton4,
        ContentButton5,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleContentNameText,
        SubMiddleCostNameText,

        //Content
        ContentText1,
        ContentText2,
        ContentText3,
        ContentText4,
        ContentText5,

        CostText1,
        CostText2,
        CostText3,
        CostText4,
        CostText5,

        //SubBottom
        SynergyNameText,
        SynergyText,
    }
    
    private EProposalType _proposalType;
    private int _currentPage = 0;
    private readonly int _itemsPerPage = 5;
    
    // 마지막 선택된 항목 추적용 변수들
    private int _lastSelectedSlotIndex = -1;
    private int _lastSelectedPage = -1;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        // 버튼 이벤트 등록
        GetButton((int)Buttons.NextButton).onClick.AddListener(OnClickNextPage);
        GetButton((int)Buttons.PreviousButton).onClick.AddListener(OnClickPreviousPage);

        // Content 버튼들 이벤트 등록
        for (int i = 0; i < _itemsPerPage; i++)
        {
            int slotIndex = i; // 클로저 변수 캡처 방지
            GetButton((int)Buttons.ContentButton1 + i).onClick.AddListener(() => OnClickContentButton(slotIndex));
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        ResetSelection();
    }
    
    public void SetInfo(EProposalType eProposalType)
    {
        _proposalType = eProposalType;
        ResetToFirstPage();
        UpdateContent();
    }

    private void ResetToFirstPage()
    {
        _currentPage = 0;
        ResetSelection();
    }

    private void ResetSelection()
    {
        _lastSelectedSlotIndex = -1;
        _lastSelectedPage = -1;
    }
    
    public void UpdateContent()
    {
        UpdateTitleTexts();
        UpdateContentItems();
        UpdateNavigationButtons();
        UpdateSynergyInfo();
    }

    private void UpdateTitleTexts()
    {
        bool isGenreSelection = _proposalType == EProposalType.Genre;
        
        GetText((int)Texts.MainTitleText).text = isGenreSelection ? "@장르 선택" : "@내용 선택";
        GetText((int)Texts.SubMiddleContentNameText).text = isGenreSelection ? "@장르" : "@내용";
        GetText((int)Texts.SubMiddleCostNameText).text = "@비용";
    }

    private void UpdateContentItems()
    {
        var (genreData, contentData) = GetCurrentPageData();
        int itemsInCurrentPage = GetCurrentPageItemCount();
        
        for (int i = 0; i < _itemsPerPage; i++)
        {
            bool hasData = i < itemsInCurrentPage;
            GetButton((int)Buttons.ContentButton1 + i).gameObject.SetActive(hasData);
            
            if (hasData)
            {
                UpdateSingleItemUI(i, genreData, contentData);
            }
        }
    }

    private (List<GenreData>, List<ContentData>) GetCurrentPageData()
    {
        if (_proposalType == EProposalType.Genre)
        {
            var genreData = GameDevManager.Instance.GetGenreDataByPage(_currentPage, _itemsPerPage);
            return (genreData, null);
        }
        else
        {
            var contentData = GameDevManager.Instance.GetContentDataByPage(_currentPage, _itemsPerPage);
            return (null, contentData);
        }
    }
    
    private void UpdateSingleItemUI(int slotIndex, List<GenreData> genreData, List<ContentData> contentData)
    {
        if (_proposalType == EProposalType.Genre && genreData != null && slotIndex < genreData.Count)
        {
            GenreData data = genreData[slotIndex];
            GetText((int)Texts.ContentText1 + slotIndex).text = GenreData.GenreToString(data.GenreType);
            GetText((int)Texts.CostText1 + slotIndex).text = data.CostToString();
        }
        else if (_proposalType == EProposalType.Content && contentData != null && slotIndex < contentData.Count)
        {
            ContentData data = contentData[slotIndex];
            GetText((int)Texts.ContentText1 + slotIndex).text = ContentData.ContentToString(data.ContentType);
            GetText((int)Texts.CostText1 + slotIndex).text = data.CostToString();
        }
    }
    
    private void UpdateNavigationButtons()
    {
        int totalPages = GetTotalPageCount();
        
        GetButton((int)Buttons.PreviousButton).interactable = _currentPage > 0;
        GetButton((int)Buttons.NextButton).interactable = _currentPage < totalPages - 1;
    }
    
    private void UpdateSynergyInfo()
    {
        GenreData genreData = GameDevManager.Instance.CurrentGenreData;
        ContentData contentData = GameDevManager.Instance.CurrentContentData;
        SynergyData synergyData = GameDevManager.Instance.GetSynergyData(genreData.GenreType, contentData.ContentType);

        string combinationText = _proposalType == EProposalType.Genre 
            ? $"{ContentData.ContentToString(contentData.ContentType)}(와)과 조합 = "
            : $"{GenreData.GenreToString(genreData.GenreType)}(와)과 조합 = ";

        GetText((int)Texts.SynergyNameText).text = combinationText;
        GetText((int)Texts.SynergyText).text = SynergyData.SynergyTypeToString(synergyData.SynergyType);
    }
    
    private int GetTotalItemCount()
    {
        return _proposalType == EProposalType.Genre 
            ? GameDevManager.Instance.GetGenreDataCount()
            : GameDevManager.Instance.GetContentDataCount();
    }

    private int GetTotalPageCount()
    {
        return Mathf.CeilToInt((float)GetTotalItemCount() / _itemsPerPage);
    }
    
    private int GetCurrentPageItemCount()
    {
        int totalItems = GetTotalItemCount();
        int startIndex = _currentPage * _itemsPerPage;
        return Mathf.Min(_itemsPerPage, totalItems - startIndex);
    }
    
    private void OnClickNextPage()
    {
        if (_currentPage < GetTotalPageCount() - 1)
        {
            _currentPage++;
            ResetSelection(); // 페이지 변경 시 선택 상태 초기화
            UpdateContent();
        }
    }
    
    private void OnClickPreviousPage()
    {
        if (_currentPage > 0)
        {
            _currentPage--;
            ResetSelection(); // 페이지 변경 시 선택 상태 초기화
            UpdateContent();
        }
    }
    
    private void OnClickContentButton(int slotIndex)
    {
        if (IsSameAsLastSelected(slotIndex))
        {
            ConfirmSelection(slotIndex);
        }
        else
        {
            SelectNewItem(slotIndex);
        }
    }

    private bool IsSameAsLastSelected(int slotIndex)
    {
        return _lastSelectedSlotIndex == slotIndex && _lastSelectedPage == _currentPage;
    }

    private void ConfirmSelection(int slotIndex)
    {
        if (_proposalType == EProposalType.Genre)
        {
            var currentPageData = GameDevManager.Instance.GetGenreDataByPage(_currentPage, _itemsPerPage);
            if (slotIndex < currentPageData.Count)
            {
                Debug.Log($"Genre confirmed: {currentPageData[slotIndex].GenreType}");
            }
        }
        else
        {
            var currentPageData = GameDevManager.Instance.GetContentDataByPage(_currentPage, _itemsPerPage);
            if (slotIndex < currentPageData.Count)
            {
                Debug.Log($"Content confirmed: {currentPageData[slotIndex].ContentType}");
            }
        }

        UIManager.Instance.ClosePopupUI();
    }

    private void SelectNewItem(int slotIndex)
    {
        bool selectionSuccessful = false;

        if (_proposalType == EProposalType.Genre)
        {
            var currentPageData = GameDevManager.Instance.GetGenreDataByPage(_currentPage, _itemsPerPage);
            if (slotIndex < currentPageData.Count)
            {
                GenreData selectedGenre = currentPageData[slotIndex];
                GameDevManager.Instance.SelectGenre(selectedGenre.GenreType);
                Debug.Log($"Genre selected: {selectedGenre.GenreType}");
                selectionSuccessful = true;
            }
        }
        else
        {
            var currentPageData = GameDevManager.Instance.GetContentDataByPage(_currentPage, _itemsPerPage);
            if (slotIndex < currentPageData.Count)
            {
                ContentData selectedContent = currentPageData[slotIndex];
                GameDevManager.Instance.SelectContent(selectedContent.ContentType);
                Debug.Log($"Content selected: {selectedContent.ContentType}");
                selectionSuccessful = true;
            }
        }

        if (selectionSuccessful)
        {
            UpdateLastSelection(slotIndex);
            UpdateSynergyInfo();
            EventManager.Instance.TriggerEvent(EEventType.ProposalChanged);
        }
    }

    private void UpdateLastSelection(int slotIndex)
    {
        _lastSelectedSlotIndex = slotIndex;
        _lastSelectedPage = _currentPage;
    }
    
    public override void RefreshUI()
    {
        base.RefreshUI();
        UpdateContent();
    }
}
