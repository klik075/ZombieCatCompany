using TMPro;
using UnityEngine;
using static Define;

public class LocalizationManager : Singleton<LocalizationManager>
{
    private ELanguage _currentLanguage = ELanguage.KOR;
    public ELanguage CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            _currentLanguage = value;
            _currentFontAsset = null;
            EventManager.Instance.TriggerEvent(EEventType.LanguageChanged);
        }
    }

    private TMP_FontAsset _currentFontAsset;
    public TMP_FontAsset CurrentFontAsset
    {
        get
        {
            if (_currentFontAsset == null)
                _currentFontAsset = DataManager.Instance.LocalizationConfig.GetFontAsset(_currentLanguage);

            return _currentFontAsset;
        }
    }

    public string GetLocalizedText(string templateID)
    {
        if (DataManager.Instance.TextDict.TryGetValue(templateID, out TextData textData))
        {
            return _currentLanguage switch
            {
                ELanguage.KOR => textData.KOR,
                ELanguage.ENG => textData.ENG,
                _ => textData.ENG
            };
        }

        Debug.LogWarning($"LocalizationManager: Text not found for TemplateID: {templateID}");
        return string.Empty;
    }
}
