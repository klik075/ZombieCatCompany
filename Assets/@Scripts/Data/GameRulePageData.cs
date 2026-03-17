using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameRulePageData
{
    public int PageId;           // 페이지 번호 (0부터 시작)
    public string Content;       // 본문 내용
}

[Serializable]
public class GameRulePageDataLoader : IDataLoader<int, GameRulePageData>
{
    public List<GameRulePageData> pages = new List<GameRulePageData>();

    public Dictionary<int, GameRulePageData> MakeDict()
    {
        Dictionary<int, GameRulePageData> dict = new Dictionary<int, GameRulePageData>();
        foreach (var page in pages)
        {
            if (dict.ContainsKey(page.PageId))
            {
                Debug.LogError($"[GameRuleDataLoader] Duplicate pageId: {page.PageId}");
                continue;
            }
            dict.Add(page.PageId, page);
        }
        return dict;
    }

    public bool Validate()
    {
        if (pages == null || pages.Count == 0)
        {
            Debug.LogError("[GameRuleDataLoader] No pages found!");
            return false;
        }

        // 페이지 ID가 0부터 연속적인지 확인
        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i].PageId != i)
            {
                Debug.LogWarning($"[GameRuleDataLoader] Page ID {i} expected, but found {pages[i].PageId}");
            }
        }

        return true;
    }
}
