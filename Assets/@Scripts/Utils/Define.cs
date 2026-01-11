using UnityEngine;

public static class Define
{
    public enum EScene
    {
        Unknown,
        LoadingScene,
        DevScene,
    }

    public enum EEventType
    {
        None,
        GoldChanged,
        LanguageChanged,

        //LeftPanel
        UI_LeftPanelStateChanged,

        UI_MenuButtonClicked,
        UI_SystemButtonClicked,

        //Popup
        UI_PopupClosed,//
        UI_PopupOpened,
    }

    public enum ESound
    {
        Bgm,
        Effect,

        MaxCount
    }

    public enum ELanguage
    {
        KOR,
        ENG,
    }

	public enum EAnimation
	{
		b_wait,
		b_walk,
		f_wait,
		f_walk
	}

	public enum ECatState
	{
		Idle,
		Move,
		Work
	}
    public enum ERoleType
    {
        Boss,//사장
        Planner,//기획자
        Designer,//디자이너
        SoundWriter,//사운드 작가
        Merchant,//상인
    }
    public enum EMemberStateType
    {
        Full,//배부름
        Hunger1,//약간 배고픔
        Hunger2,//배고픔
        Starvation,//굶주림
        Soon,//곧 죽음
    }
    public enum EAbilityType
    {
        Programming,//프로그래밍
        Scenario,//시나리오
        Graphics,//그래픽
        Sound,//사운드
        Power//전투력
    }
}
