using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UI_GameHUD : UI_Toolkit, IUI_Scene
{
    enum Buttons
    {
        Button1,
        Button2,
        Button3
    }

	protected override void Awake()
	{
		base.Awake();

		BindButtons(typeof(Buttons));
		Get<Button>((int)Buttons.Button1).clicked += OnButton1Clicked;
		Get<Button>((int)Buttons.Button2).clicked += OnButton2Clicked;
		Get<Button>((int)Buttons.Button3).clicked += OnButton3Clicked;
	}

    protected override void Start()
    {
        base.Start();
        
    }

    public override void RefreshUI()
    {
        //Get<Button>((int)Buttons.Button1).text = LocalizationManager.Instance.GetLocalizedText("버튼1");
        //Get<Button>((int)Buttons.Button2).text = LocalizationManager.Instance.GetLocalizedText("버튼2");
        //Get<Button>((int)Buttons.Button3).text = LocalizationManager.Instance.GetLocalizedText("버튼3");
    }

    private void OnButton1Clicked()
    {
        Debug.Log("Button 1 Clicked");
		UIManager.Instance.ShowPopupUI<UI_Hire>();
    }

    private void OnButton2Clicked()
    {
        Debug.Log("Button 2 Clicked");
	}

    private void OnButton3Clicked()
    {
        Debug.Log("Button 3 Clicked");
	}
}