using UnityEngine;
using UnityEngine.UI;
using TMPro;

// TODO
// - ClickButton (Button), GoldText (TMP) 코드로 찾아서 연결
// - ClickButton 클릭하면, Gold가 1 증가하고, GoldText에 갱신

public class UI_GameScene : MonoBehaviour
{
    private Button clickButton;
    private TMP_Text goldText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        clickButton = GameObject.Find("ClickButton").GetComponent<Button>();
        goldText = GameObject.Find("GoldText").GetComponent<TMP_Text>();
        clickButton.onClick.AddListener(OnClickButton);
        UpdateGoldText();
    }

    private void OnClickButton()
    {
        //GameManager.Instance.AddGold(1);
        UpdateGoldText();
    }

    private void UpdateGoldText()
    {
        //goldText.text = GameManager.Instance.Gold.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
