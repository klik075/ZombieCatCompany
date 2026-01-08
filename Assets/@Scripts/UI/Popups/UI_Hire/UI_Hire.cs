using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class UI_Hire : UI_Toolkit, IUI_Popup
{
    private UIDocument _uiDocument;
    private VisualElement _hirePanel;
	private Button _hireButton;
	private Button _cancelButton;

	private Sprite _image;
	private string _name;
	private int _stat1;
	private int _stat2;

	enum Buttons
	{ 
		HireButton,
		CancelButton
	}

	enum Texts
	{
		EmployeeName,
		Stat1,
		Stat2
	}

	enum Images
	{
		EmployeeImage
	}

	protected override void Awake()
	{
		base.Awake();

		// Bind UI elements using UI Toolkit version of Bind/Get
		BindButtons(typeof(Buttons));
		BindTexts(typeof(Texts));
		BindImages(typeof(Images));

		// Get buttons and add event listeners
		Get<Button>((int)Buttons.HireButton).clicked += OnHireClicked;
		Get<Button>((int)Buttons.CancelButton).clicked += OnCancelClicked;
	}

	protected override void Start()
    {
		base.Start();
	}

	public void SetInfo(Sprite employeeImage, string name, int stat1, int stat2)
	{
		_image = employeeImage;
		_name = name;
		_stat1 = stat1;
		_stat2 = stat2;

		RefreshUI();
	}

	public override void RefreshUI()
	{
		if (_hirePanel == null) 
			return;

		// Set image
		Get<Image>((int)Images.EmployeeImage).sprite = _image;

		// Set name
		Get<Label>((int)Texts.EmployeeName).text = _name;

		// Set stats with localization
		Get<Label>((int)Texts.Stat1).text = $"{LocalizationManager.Instance.GetLocalizedText("능력치1")}: {_stat1}";
		Get<Label>((int)Texts.Stat2).text = $"{LocalizationManager.Instance.GetLocalizedText("능력치2")}: {_stat2}";

		// Set button texts with localization
		Get<Button>((int)Buttons.HireButton).text = LocalizationManager.Instance.GetLocalizedText("고용");
		Get<Button>((int)Buttons.CancelButton).text = LocalizationManager.Instance.GetLocalizedText("취소");
	}

    private void OnHireClicked()
    {
        // Implement hire logic here
        Debug.Log("Hired!");
		UIManager.Instance.ClosePopupUI();
	}

    private void OnCancelClicked()
    {
		UIManager.Instance.ClosePopupUI();
	}
}