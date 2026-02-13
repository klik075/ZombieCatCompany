using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveManagerEditor : EditorWindow
{
#if UNITY_EDITOR

    [MenuItem("Tools/DeleteSaveFile %#L")] // Ctrl+Shift+L
    public static void DeleteSaveFile()
    {
        if (File.Exists(SaveManager.GameDataPath))
        {
            File.Delete(SaveManager.GameDataPath);
            Debug.Log($"Save File Deleted : {SaveManager.GameDataPath}");
        }
        else
        {
            Debug.Log("No Save File Found");
        }
    }
#endif
}
