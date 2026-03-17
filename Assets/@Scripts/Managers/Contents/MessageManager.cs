using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

/// <summary>
/// UI_ChatPopup과 UI_MessagePopup의 메시지 스크립트를 관리하는 매니저
/// 메시지 종류에 따라 정해진 대사/스크립트를 반환
/// </summary>
public class MessageManager : Singleton<MessageManager>
{
    // 메시지 데이터 저장소
    private Dictionary<EMessageType, MessageScript> _messageScripts = new Dictionary<EMessageType, MessageScript>();
    //private Dictionary<EChatType, List<ChatScript>> _chatScripts = new Dictionary<EChatType, List<ChatScript>>();

    private void Awake()
    {
        InitializeMessageScripts();
        //InitializeChatScripts();
    }

    #region Message Scripts (UI_MessagePopup용)

    /// <summary>
    /// 메시지 타입에 해당하는 스크립트 반환
    /// </summary>
    public MessageScript GetMessageScript(EMessageType messageType)
    {
        if (_messageScripts.TryGetValue(messageType, out MessageScript messageScript))
            return messageScript;

        Debug.LogWarning($"MessageManager: No script found for message type {messageType}");
        return GetDefaultMessageScript();
    }

    /// <summary>
    /// 커스텀 메시지 스크립트 생성
    /// </summary>
    public MessageScript CreateCustomMessage(string[] contents)
    {
        var customScript = new MessageScript
        {
            Contents = contents,
        };

        return customScript;
    }

    #endregion

    #region Chat Scripts (UI_ChatPopup용)

    /// <summary>
    /// 채팅 타입에 해당하는 랜덤 대사 반환
    /// </summary>
    //public ChatScript GetRandomChatScript(EChatType chatType)
    //{
    //    if (_chatScripts.TryGetValue(chatType, out List<ChatScript> scripts) && scripts.Count > 0)
    //    {
    //        var randomScript = scripts[UnityEngine.Random.Range(0, scripts.Count)];
                
    //        return randomScript;
    //    }

    //    Debug.LogWarning($"MessageManager: No chat script found for type {chatType}");
    //    return GetDefaultChatScript();
    //}

    /// <summary>
    /// 특정 멤버의 대사 반환
    /// </summary>
    //public ChatScript GetMemberChat(EChatType chatType, string memberName, string targetName = "")
    //{
    //    var baseScript = GetRandomChatScript(chatType);
        
    //    return new ChatScript
    //    {
    //        SpeakerName = memberName,
    //        Message = baseScript.Message.Replace("{MemberName}", memberName).Replace("{TargetName}", targetName),
    //        ChatType = chatType
    //    };
    //}

    /// <summary>
    /// 시스템 메시지 생성
    /// </summary>
    public ChatScript CreateSystemMessage(string message)
    {
        var systemScript = new ChatScript
        {
            SpeakerName = "시스템",
            Message = message,
            ChatType = EChatType.System
        };

        return systemScript;
    }

    #endregion

    #region Data Initialization

    private void InitializeMessageScripts()
    {
        _messageScripts[EMessageType.MoneyLow] = new MessageScript
        {
            Contents = new string[] { "@자금이 부족하다냥" }
        };

        _messageScripts[EMessageType.AlreadyRecruiting] = new MessageScript
        {
            Contents = new string[] { "@이미 모집 중이다냥" }
        };

        _messageScripts[EMessageType.StartRecruiting] = new MessageScript
        {
            Contents = new string[] { "@구인 사이트에 광고를 게재했다냥.\r\n곧 있으면 결과가 나오겠지냥." }
        };
        _messageScripts[EMessageType.CompleteRecruiting] = new MessageScript
        {
            Contents = new string[] { "@모집했던 결과가 나왔다냥.\r\n고양이 ","마리가 지원했다냥.\r\n누구를 채용할까냥?" }
        };
        _messageScripts[EMessageType.MembersFull] = new MessageScript
        {
            Contents = new string[] { "@자리가 없다냥.\r\n누구를 해고할까냥?" }
        };
        _messageScripts[EMessageType.MemberFired] = new MessageScript
        {
            Contents = new string[] { "", "(을)를\r\n해고하시겠습니까?" }
        };
        _messageScripts[EMessageType.MemberSwapped] = new MessageScript
        {
            Contents = new string[] { "", "(을)를 해고하고\r\n","(을)를 신규 고용했다냥." }
        };
        _messageScripts[EMessageType.MemberHired] = new MessageScript
        {
            Contents = new string[] { "", "(을)를 감염시켰다냥.\r\n기존 능력치의 일부가 전투력으로 전환됐다냥." }
        };
        _messageScripts[EMessageType.MemberFiredConfirm] = new MessageScript
        {
            Contents = new string[] { "", "(을)를 해고했다냥." }
        };
        _messageScripts[EMessageType.SelectPlanner] = new MessageScript
        {
            Contents = new string[] { "", "번째 작품은 ","에 ","인 게임이다냥.","\r\n누구에 기획을 맡길까냥?" }
        };
        _messageScripts[EMessageType.SelectDesigner] = new MessageScript
        {
            Contents = new string[] { "누구에게 디자인을 맡길까냥 ?" }
        };
        _messageScripts[EMessageType.SelectSoundWriter] = new MessageScript
        {
            Contents = new string[] { "마지막으로 누구에게 사운드를 맡길까냥?" }
        };
        _messageScripts[EMessageType.StartDebugging] = new MessageScript
        {
            Contents = new string[] { "버그 제거를 시작하자냥!" }
        };
        _messageScripts[EMessageType.CompleteGameDev] = new MessageScript
        {
            Contents = new string[] { "","번째 게임이 완성되었다냥." }
        };
        _messageScripts[EMessageType.NoRecruiting] = new MessageScript
        {
            Contents = new string[] { "지금은 모집할 수 없다냥." }
        };
        _messageScripts[EMessageType.NoFire] = new MessageScript
        {
            Contents = new string[] { "지금은 해고할 수 없다냥." }
        };
        _messageScripts[EMessageType.NoDev] = new MessageScript
        {
            Contents = new string[] { "지금은 개발할 수 없다냥." }
        };
        _messageScripts[EMessageType.MerchantHello] = new MessageScript
        {
            Contents = new string[] { "반갑다냥.\r\n통조림을 구매할 거냥?" }
        };
        _messageScripts[EMessageType.PurchaseMessage] = new MessageScript
        {
            Contents = new string[] { "통조림을 구매할까?" }
        };
        _messageScripts[EMessageType.MerchantBye] = new MessageScript
        {
            Contents = new string[] { "다음에 또 오겠다냥." }
        };
        _messageScripts[EMessageType.TryEnhanceFence] = new MessageScript
        {
            Contents = new string[] { "펜스를 강화할까?\r\n성공 확률 ","\r\n비용 " }
        };
        _messageScripts[EMessageType.EnhanceSuccess] = new MessageScript
        {
            Contents = new string[] { "성공적으로 강화했다냥." }
        };
        _messageScripts[EMessageType.EnhanceFail] = new MessageScript
        {
            Contents = new string[] { "강화에 실패했다냥.\r\n내구도가 떨어졌다냥." }
        };
        _messageScripts[EMessageType.TryRepairFence] = new MessageScript
        {
            Contents = new string[] { "펜스의 체력과 내구도를 수리할까?\r\n비용 " }
        };
        _messageScripts[EMessageType.RepairSuccess] = new MessageScript
        {
            Contents = new string[] { "펜스를 성공적으로 수리했다냥." }
        };
        _messageScripts[EMessageType.SelectDispatch] = new MessageScript
        {
            Contents = new string[] { "새로운 파견을 보낼까?" }
        };
        _messageScripts[EMessageType.DispatchMemberSelected] = new MessageScript
        {
            Contents = new string[] { "내가 갈 순 없지 않냥.\r\n","! 믿는다냥!" }
        };
        _messageScripts[EMessageType.FoodRationing] = new MessageScript
        {
            Contents = new string[] { "식량 배급 시간이다냥.." }
        };
        _messageScripts[EMessageType.FoodShortage] = new MessageScript
        {
            Contents = new string[] { "식량이 부족하다냥.\r\n누군가는 굶어야겠지냥.." }
        };
        _messageScripts[EMessageType.DispatchNotSent] = new MessageScript
        {
            Contents = new string[] { "파견을 보내지 않았다냥.." }
        };
        _messageScripts[EMessageType.EndingStarvation] = new MessageScript
        {
            Contents = new string[] { "자금이 부족하다냥.\r\n여기까지인 것 같다냥..." }
        };
        _messageScripts[EMessageType.EndingExposed] = new MessageScript
        {
            Contents = new string[] { "정체가 발각되었다냥.\r\n여기까지인 것 같다냥..." }
        };
        _messageScripts[EMessageType.EndingSerum] = new MessageScript
        {
            Contents = new string[] { "혈청을 구했다냥.\r\n나는 자유다냥!" }
        };
    }

    //private void InitializeChatScripts()
    //{
    //    // 멤버 간 대화
    //    _chatScripts[EChatType.MemberToMember] = new List<ChatScript>
    //    {
    //        new ChatScript { SpeakerName = "", Message = "오늘 작업은 어떻게 진행되고 있나요냥?", ChatType = EChatType.MemberToMember },
    //        new ChatScript { SpeakerName = "", Message = "점심은 뭘 먹을까요냥?", ChatType = EChatType.MemberToMember },
    //        new ChatScript { SpeakerName = "", Message = "이번 프로젝트 재미있네요냥!", ChatType = EChatType.MemberToMember },
    //        new ChatScript { SpeakerName = "", Message = "같이 열심히 해봐요냥!", ChatType = EChatType.MemberToMember }
    //    };

    //    // 업무 완료
    //    _chatScripts[EChatType.WorkComplete] = new List<ChatScript>
    //    {
    //        new ChatScript { SpeakerName = "", Message = "업무 완료했습니다냥!", ChatType = EChatType.WorkComplete },
    //        new ChatScript { SpeakerName = "", Message = "작업이 끝났어요냥!", ChatType = EChatType.WorkComplete },
    //        new ChatScript { SpeakerName = "", Message = "드디어 완성했습니다냥!", ChatType = EChatType.WorkComplete }
    //    };

    //    // 배고픔 불만
    //    _chatScripts[EChatType.ComplaintHungry] = new List<ChatScript>
    //    {
    //        new ChatScript { SpeakerName = "", Message = "배가 고파요냥... 사료 좀 주세요냥...", ChatType = EChatType.ComplaintHungry },
    //        new ChatScript { SpeakerName = "", Message = "언제 밥을 먹을 수 있나요냥?", ChatType = EChatType.ComplaintHungry },
    //        new ChatScript { SpeakerName = "", Message = "굶어 죽을 것 같아요냥...", ChatType = EChatType.ComplaintHungry }
    //    };

    //    // 기쁨 표현
    //    _chatScripts[EChatType.Happy] = new List<ChatScript>
    //    {
    //        new ChatScript { SpeakerName = "", Message = "오늘 기분이 좋네요냥!", ChatType = EChatType.Happy },
    //        new ChatScript { SpeakerName = "", Message = "야호! 신나요냥!", ChatType = EChatType.Happy },
    //        new ChatScript { SpeakerName = "", Message = "행복합니다냥~", ChatType = EChatType.Happy }
    //    };

    //    // 화남 표현
    //    _chatScripts[EChatType.Angry] = new List<ChatScript>
    //    {
    //        new ChatScript { SpeakerName = "", Message = "화가 나요냥!", ChatType = EChatType.Angry },
    //        new ChatScript { SpeakerName = "", Message = "이건 너무하잖아요냥!", ChatType = EChatType.Angry },
    //        new ChatScript { SpeakerName = "", Message = "참을 수 없어요냥!", ChatType = EChatType.Angry }
    //    };

    //    // 랜덤 대화
    //    _chatScripts[EChatType.RandomChat] = new List<ChatScript>
    //    {
    //        new ChatScript { SpeakerName = "", Message = "날씨가 좋네요냥!", ChatType = EChatType.RandomChat },
    //        new ChatScript { SpeakerName = "", Message = "오늘도 화이팅이에요냥!", ChatType = EChatType.RandomChat },
    //        new ChatScript { SpeakerName = "", Message = "게임 개발이 재미있어요냥!", ChatType = EChatType.RandomChat },
    //        new ChatScript { SpeakerName = "", Message = "우리 회사 최고예요냥!", ChatType = EChatType.RandomChat }
    //    };
    //}

    #endregion

    #region Helper Methods

    /// <summary>
    /// 기본 메시지 스크립트 반환
    /// </summary>
    private MessageScript GetDefaultMessageScript()
    {
        return new MessageScript
        {
            Contents = new string[] { "메시지를 불러올 수 없다냥!" }
        };
    }

    /// <summary>
    /// 기본 채팅 스크립트 반환
    /// </summary>
    private ChatScript GetDefaultChatScript()
    {
        return new ChatScript
        {
            SpeakerName = "???",
            Message = "...",
            ChatType = EChatType.System
        };
    }

    /// <summary>
    /// 특정 타입의 채팅 스크립트 추가
    /// </summary>
    //public void AddChatScript(EChatType chatType, ChatScript script)
    //{
    //    if (!_chatScripts.ContainsKey(chatType))
    //    {
    //        _chatScripts[chatType] = new List<ChatScript>();
    //    }

    //    _chatScripts[chatType].Add(script);
    //}

    /// <summary>
    /// 메시지 스크립트 추가/수정
    /// </summary>
    public void SetMessageScript(EMessageType messageType, MessageScript script)
    {
        _messageScripts[messageType] = script;
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// 모든 메시지 스크립트 로그 출력 (에디터 전용)
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void LogAllMessageScripts()
    {
        Debug.Log("=== MessageManager: All Message Scripts ===");
        foreach (var kvp in _messageScripts)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value.Contents}");
        }
    }

    /// <summary>
    /// 모든 채팅 스크립트 로그 출력 (에디터 전용)
    /// </summary>
    //[System.Diagnostics.Conditional("UNITY_EDITOR")]
    //public void LogAllChatScripts()
    //{
    //    Debug.Log("=== MessageManager: All Chat Scripts ===");
    //    foreach (var kvp in _chatScripts)
    //    {
    //        Debug.Log($"{kvp.Key}: {kvp.Value.Count} scripts available");
    //        foreach (var script in kvp.Value)
    //        {
    //            Debug.Log($"  - {script.Message}");
    //        }
    //    }
    //}

    #endregion
}

#region Data Classes

/// <summary>
/// UI_MessagePopup용 메시지 스크립트 데이터
/// </summary>
[System.Serializable]
public class MessageScript
{
    public string[] Contents;    // 메시지 내용
    
    public MessageScript() { }
    
    public MessageScript(string[] contents)
    {
        Contents = contents;
    }
}

/// <summary>
/// UI_ChatPopup용 채팅 스크립트 데이터
/// </summary>
[System.Serializable]
public class ChatScript
{
    public string SpeakerName; // 발화자 이름
    public string Message;     // 메시지 내용
    public EChatType ChatType; // 채팅 타입
    
    public ChatScript() { }
    
    public ChatScript(string speakerName, string message, EChatType chatType = EChatType.RandomChat)
    {
        SpeakerName = speakerName;
        Message = message;
        ChatType = chatType;
    }
}

#endregion
