using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_ChatPopup : UI_UGUI, IUI_Popup, IClickableUI
{
    enum GameObjects
    {

    }
    enum Buttons
    {
        //BG
        Click,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //Content
        ContentText,

        //SubBottom
        RoleText,
    }
    enum Images
    {
        //SubBottom
        MemberImage,
    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));
    }
    public void SetInfo(int employeeId, string[] scripts, string[] insertScripts = null, Action action = null)//클릭 시 사용할 메서드 등록해야 함.
    {
        DataManager.Instance.MemberDict.TryGetValue(employeeId, out MemberData memberData);

        if(memberData == null)
            return;

        Button clickButton = GetButton((int)Buttons.Click);
        clickButton.onClick.RemoveAllListeners();

        if (action != null)
            clickButton.onClick.AddListener(() => OnClickButton(Buttons.Click, action));
        else
            clickButton.onClick.AddListener(() => ClosePopup());

        GetText((int)Texts.RoleText).text = MemberData.RoleToString(memberData.Role);
        GetImage((int)Images.MemberImage).sprite = ResourceManager.Instance.Get<Sprite>(memberData.ZombieImagePath);

        GetText((int)Texts.ContentText).text = GetContentText(scripts, insertScripts);
    }
    public string GetContentText(string[] script, string[] insertScript)
    {
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < script.Length; i++)
        {
            stringBuilder.Append(script[i]);
            if (insertScript != null && i < insertScript.Length)
            {
                stringBuilder.Append(insertScript[i]);
            }
        }

        return stringBuilder.ToString();
    }
    public void ClosePopup()
    {
        UIManager.Instance.ClosePopupUI();
    }
    private void OnClickButton(Buttons click, Action action)
    {
        ClosePopup();
        action?.Invoke();
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
