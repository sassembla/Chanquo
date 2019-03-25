using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class UnityPackageGenerator
{
    [MenuItem("Window/Autoya/Update UnityPackage")]
    public static void UnityPackage()
    {
        var assetPaths = new List<string>();

        var frameworkPath = "Assets/Chanquo";
        CollectPathRecursive(frameworkPath, assetPaths);

        AssetDatabase.ExportPackage(assetPaths.ToArray(), "Chanquo.unitypackage", ExportPackageOptions.IncludeDependencies);
    }

    private static void CollectPathRecursive(string path, List<string> collectedPaths)
    {
        var filePaths = Directory.GetFiles(path);
        foreach (var filePath in filePaths)
        {
            collectedPaths.Add(filePath);
        }

        var modulePaths = Directory.GetDirectories(path);
        foreach (var folderPath in modulePaths)
        {
            CollectPathRecursive(folderPath, collectedPaths);
        }
    }

    static UnityPackageGenerator()
    {
        // create unitypackage if compiled.
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            UnityPackage();
        }
    }
}