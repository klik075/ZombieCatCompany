using TMPro;
using UnityEngine;

public static class Extension
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        return Utils.GetOrAddComponent<T>(go);
    }

    public static void DestroyChildren(this GameObject go)
    {
        go.transform.DestroyChildren();
    }

    public static void DestroyChildren(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public static void SetLocalizedText(this TMP_Text tmpText, string templateID)
    {
        if (tmpText == null)
        {
            Debug.LogWarning("Extension.GetText: TMP_Text is null");
            return;
        }

        tmpText.text = LocalizationManager.Instance.GetLocalizedText(templateID);
        tmpText.font = LocalizationManager.Instance.CurrentFontAsset;
    }
}
