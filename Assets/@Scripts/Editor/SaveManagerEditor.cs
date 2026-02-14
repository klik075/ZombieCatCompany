using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveManagerEditor : EditorWindow
{
#if UNITY_EDITOR

    [MenuItem("Tools/DeleteGameData")]
    public static void DeleteGameData()
    {
        if (File.Exists(SaveManager.GameDataPath))
        {
            File.Delete(SaveManager.GameDataPath);
            Debug.Log($"Save File Deleted : {SaveManager.GameDataPath}");
        }
        else
        {
            Debug.Log("No Game Data File Found");
        }
    }
    // Delete User Data
    [MenuItem("Tools/DeleteUserData")]
    public static void DeleteUserData()
    {
        if (File.Exists(SaveManager.UserDataPath))
        {
            File.Delete(SaveManager.UserDataPath);
            Debug.Log($"User Data File Deleted : {SaveManager.UserDataPath}");
            DeleteGameData();
        }
        else
        {
            Debug.Log("No User Data File Found");
        }
    }
#endif
}
