using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using zFrame.UI;
using zFramework.UI;
using static GameManager;

public class LibraryManagePage : BasePage, IReusableScrollViewDataSource
{
    public ReusableScrollViewController controller;
    public List<ProjectInfo> datas;
    public Button loadProjects; // 使用 Everything 搜索项目
    public Button deleteLibrary;
    public Button reloadDatabase; //重新加载 Everything 数据库
    public Button selectAll;
    public Button invertSelect;
    Everything everything;
    public NotificationPanel notification;
    public ProgressPanel progressPanel;
    public GameObject state;

    public int Count => datas?.Count ?? 0;

    public override void Start()
    {
        loadProjects.onClick.AddListener(OnLoadProjects);
        deleteLibrary.onClick.AddListener(OnDeleteLibraryAsync);
        reloadDatabase.onClick.AddListener(OnReloadDatabase);
        selectAll.onClick.AddListener(OnLibraryDeleteRequireAll);
        invertSelect.onClick.AddListener(OnLibraryDeleteRequireInvert);
        everything = new Everything();
        everything.OnVersionQuery += (version) =>
        {
            Debug.Log("Everything Version : " + version);
        };
        everything.OnDatabaseLoaded += () =>
        {
            Debug.Log("Everything Database Loaded");
        };
    }
    public override void Update()
    {
        base.Update();
        //log count
        state.SetActive(datas?.Count <= 0);
    }

    public override async Task OnEnter(params object[] args)
    {
        await base.OnEnter(args);
        //can not interacte
        canvasGroup.blocksRaycasts = false;
        Debug.Log("Starting Everything Engine Please wait,....");
        await everything.StartSearchEngineAsync();
        Debug.Log("Everything Engine Started");
        datas = await everything.QueryProjectInfosAsync();
        LoadScrollData(false);
        canvasGroup.blocksRaycasts = true;
    }

    public async override Task OnExit()
    {
        await base.OnExit();
        everything.Reset();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        everything?.Dispose();
    }

    #region Button Click Event

    private void OnLibraryDeleteRequireInvert()
    {
        foreach (var item in datas)
        {
            item.isLibraryDeleteRequired = !item.isLibraryDeleteRequired;
            //Update WhiteList File 
            Action<string> action = item.isLibraryDeleteRequired ? RemoveProjectFromWhiteList : AddProjectToWhiteList;
            action(item.path);
        }
        LoadScrollData();
    }

    private void OnLibraryDeleteRequireAll()
    {
        foreach (var item in datas)
        {
            item.isLibraryDeleteRequired = true;
            //Update WhiteList File 
            RemoveProjectFromWhiteList(item.path);
        }
        LoadScrollData();
    }

    private async void OnReloadDatabase()
    {
        canvasGroup.blocksRaycasts = false;
        var result = await everything.UpdateAllFolderIndexesAsync();
        canvasGroup.blocksRaycasts = true;
        //TODO: 弹窗提示重新加载数据库成功
        if (result)
        {
            Debug.Log("Reload Database Success");
        }
        else
        {
            Debug.Log("Reload Database Failed");
        }
    }

    private async void OnLoadProjects()
    {
        canvasGroup.blocksRaycasts = false;
        datas = await everything.QueryProjectInfosAsync();
        LoadScrollData(true);
        canvasGroup.blocksRaycasts = true;
    }
    public async void OnDeleteLibraryAsync()
    {
        // 用户确认删除
        var result = await notification.ShowAsync("Delete Library", "Are you sure to delete the Library folder of the selected project?");
        if (result == 0) //确认
        {
            _ = progressPanel.ShowAsync("", "");

            for (int i = 0; i < datas.Count; i++)
            {
                var data = datas[i];
                if (data.isLibraryDeleteRequired)
                {
                    if (progressPanel.IsCanceled)
                    {
                        break;
                    }
                    progressPanel.UpdateProgress(data.path, $"{i + 1}/{datas.Count}");
                    await Task.Run(() => OnProjectLibraryDelete(data, false));
                    AddProjectToHandledList(data.path);
                }
            }
            await progressPanel.HideAsync();
            LoadScrollData(false);
        }
        else if (result == 1) //取消
        {
            Debug.Log("Cancel Delete Library");
        }
    }
    #endregion

    public void UpdateCell(BaseCell cell)
    {
        if (cell is ProjectInfoCell projectInfoCell)
        {
            var data = datas[cell.dataIndex];
            projectInfoCell.ConfigureCell(data);
            projectInfoCell.deleteButton.onClick.RemoveAllListeners();
            projectInfoCell.deleteButton.onClick.AddListener(() =>
            {
                OnProjectLibraryDelete(data, true);
            });
            projectInfoCell.libraryDeleteRequire.onValueChanged.RemoveAllListeners();
            projectInfoCell.libraryDeleteRequire.onValueChanged.AddListener((isOn) =>
            {
                OnProjectLibraryMarked(data, isOn);
            });
        }
    }

    private void OnProjectLibraryMarked(ProjectInfo data, bool isOn)
    {
        data.isLibraryDeleteRequired = isOn;
        //Update WhiteList File 
        Action<string> action = isOn ? RemoveProjectFromWhiteList : AddProjectToWhiteList;
        action(data.path);
    }

    //删除 Library 文件夹
    private void OnProjectLibraryDelete(ProjectInfo data, bool refreshData)
    {
        if (data != null && data.isLibraryDeleteRequired)
        {
            var libraryPath = Path.Combine(data.path, "Library");
            //删除除了 GameManager.FilesWhiteList  之外的所有文件及文件夹
            var temp = Path.Combine(data.path, "Library_Temp");
            if (!Directory.Exists(temp))
            {
                Directory.CreateDirectory(temp);
            }
            foreach (var file in FilesWhiteList)
            {
                var filePath = Path.Combine(libraryPath, file);
                if (File.Exists(filePath))
                {
                    File.Move(filePath, Path.Combine(temp, file));
                }
            }
            Directory.Delete(libraryPath, true);
            Directory.Move(temp, libraryPath);
            // update datas
            datas.Remove(data);
            if (refreshData)
            {
                LoadScrollData();
            }
        }
    }

    public void LoadScrollData(bool restorePos = true)
    {
        var pos = restorePos ? controller.ScrollPosition : Vector2.one;
        controller.DataSource = this;
        controller.ScrollPosition = pos;
    }
}
