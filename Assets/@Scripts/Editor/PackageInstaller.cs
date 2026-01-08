using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using System.Linq;

public class PackageInstaller
{
    [MenuItem("Tools/Install Required Packages")]
    static void InstallPackages()
    {
        var listRequest = Client.List();
        EditorApplication.update += OnListCompleted;
    }

    static void OnListCompleted()
    {
        var listRequest = Client.List();
        if (listRequest.IsCompleted == false)
            return;

        if (listRequest.Status == StatusCode.Success)
        {
            var installedPackages = listRequest.Result.Select(p => p.name).ToList();
            string[] requiredPackages = 
            { 
                "com.unity.nuget.newtonsoft-json", 
                "com.unity.textmeshpro", 
                "com.unity.services.levelplay", 
                "com.unity.purchasing" 
            };
                
            foreach (var package in requiredPackages)
            {
                if (!installedPackages.Contains(package))
                {
                    var addRequest = Client.Add(package);
                    Debug.Log($"Installing {package}...");
                }
                else
                {
                    Debug.Log($"{package} is already installed. Skipping.");
                }
            }
        }
        else
        {
            Debug.LogError("Failed to list packages: " + listRequest.Error.message);
        }

        EditorApplication.update -= OnListCompleted;
    }
}
