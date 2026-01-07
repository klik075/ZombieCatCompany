using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveManagerEditor : EditorWindow
{
#if UNITY_EDITOR

    [MenuItem("Tools/DeleteSaveFile %#L")] // Ctrl+Shift+L
    public static void DeleteSaveFile()
    {
        if (File.Exists(SaveManager.SavePath))
        {
            File.Delete(SaveManager.SavePath);
            Debug.Log($"Save File Deleted : {SaveManager.SavePath}");
        }
        else
        {
            Debug.Log("No Save File Found");
        }
    }
#endif
}
