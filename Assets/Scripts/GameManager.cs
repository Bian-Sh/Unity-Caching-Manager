using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using zFramework.UI;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    private void Start()
    {
        LoadHandledProject();
        LoadWhiteList();
        _ = UIManager.ShowPageAsync<LibraryManagePage>();
    }

    /// <summary>
    /// 排除项目而避免 Library 被删除
    /// </summary>
    public static List<string> ProjectWhiteList = new List<string>();
    public static List<string> ProjectHandled = new List<string>();
    /// <summary>
    ///  Library 文件夹中需要保留的文件
    /// </summary>
    public static string[] FilesWhiteList = new string[]
    {
        "LastSceneManagerSetup.txt",
        "EditorUserBuildSettings.asset"
    };

    /// <summary>
    ///  Everything 搜索引擎的搜索
    /// </summary>
    public static string searchPattern = "Library child:EditorUserBuildSettings.asset|LastSceneManagerSetup.txt";


    #region WhiteList
    public static void AddProjectToWhiteList(string projectPath)
    {
        if (!ProjectWhiteList.Contains(projectPath))
        {
            ProjectWhiteList.Add(projectPath);
        }
    }

    public static void RemoveProjectFromWhiteList(string projectPath)
    {
        if (ProjectWhiteList.Contains(projectPath))
        {
            ProjectWhiteList.Remove(projectPath);
        }
    }

    public static void LoadWhiteList()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "WhiteList/Project.txt");
        if (File.Exists(path))
        {
            ProjectWhiteList = new List<string>(File.ReadAllLines(path, Encoding.UTF8));
        }
    }

    public static bool IsProjectInWhiteList(string projectPath)
    {
        return ProjectWhiteList.Contains(projectPath);
    }

    public static void SaveWhiteList()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "WhiteList/Project.txt");
        var dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllLines(path, ProjectWhiteList.ToArray(), Encoding.UTF8);
    }

    private void OnApplicationQuit()
    {
        SaveWhiteList(); //当且仅当程序退出时保存白名单
        SaveHandledProject();
    }

    private void SaveHandledProject()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "WhiteList/Handled.txt");
        var dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllLines(path, ProjectHandled.ToArray(), Encoding.UTF8);
    }


    private void LoadHandledProject()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "WhiteList/Handled.txt");
        if (File.Exists(path))
        {
            ProjectHandled = new List<string>(File.ReadAllLines(path, Encoding.UTF8));
        }
    }

    public static void AddProjectToHandledList(string path)
    {
        if (!ProjectHandled.Contains(path))
        {
            ProjectHandled.Add(path);
        }
    }
    internal static bool IsProjectHandled(string path)
    {
        return ProjectHandled.Contains(path);
    }
    #endregion

}
