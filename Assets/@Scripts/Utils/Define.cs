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
}
