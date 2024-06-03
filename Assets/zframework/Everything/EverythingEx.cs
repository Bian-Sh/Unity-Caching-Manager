using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;
using static EverythingWrapper;
public class Everything : IDisposable
{
    public event Action<Version> OnVersionQuery;
    public event Action OnDatabaseLoaded;

    public enum ErrorCode
    {
        Ok = 0,
        Memory,
        Ipc,
        RegisterClassEX,
        CreateWindow,
        CreateThread,
        InvalidIndex,
        Invalidcall
    }

    public Everything()
    {
        //��Ҫ�����̵߳���
        if (Thread.CurrentThread.ManagedThreadId != 1)
        {
            throw new Exception("Everything must be created in the main thread");
        }
        context = SynchronizationContext.Current;
        isCustomProcess = false;
    }

    // ���� Everything �������棺
    // 1. ���� Process��������ڣ����Ϊ Custom ���� Self
    // 2. ��ѯ Version ������ OnVersionQuery �¼�
    // 3. ��ѯ�Ƿ��Ѿ��������ݿ⣬���û�У��첽�ȴ��������
    // 4. ���ݿ������ɺ󣬴��� OnDatabaseLoaded �¼�
    // 5. ���ж������쳣������ true�����򷵻� false
    public async Task<bool> StartSearchEngineAsync()
    {
        try
        {
            process = Process.GetProcessesByName("Everything").Length > 0 ? Process.GetProcessesByName("Everything")[0] : null;
            isCustomProcess = process != null;
            if (process == null)
            {
                var path = Path.Combine(Application.streamingAssetsPath, "Everything/Everything.exe");                    // args : -admin -startup
                process = Process.Start(path, "-admin -startup");
            }
            await Task.Delay(1000);

            Version version = GetVersion();
            context.Post(_ => OnVersionQuery?.Invoke(version), null);
            if (!Everything_IsDBLoaded())
            {
                await Task.Run(() =>
                {
                    while (!Everything_IsDBLoaded())
                    {
                        Thread.Sleep(100);
                    }
                });
                context.Post(_ => OnDatabaseLoaded?.Invoke(), null);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
        }
        return true;
    }
    public async Task<bool> UpdateAllFolderIndexesAsync()
    {
        if (process == null || process.HasExited)
        {
            return false;
        }
        return await Task.Run(() => Everything_UpdateAllFolderIndexes());
    }


    public async Task<List<ProjectInfo>> QueryProjectInfosAsync()
    {
        Everything_SetRegex(false);
        Everything_SetSearch("folders: Library child:EditorUserBuildSettings.asset|LastSceneManagerSetup.txt");
        Everything_SetRequestFlags(EVERYTHING_REQUEST_PATH | EVERYTHING_REQUEST_DATE_MODIFIED | EVERYTHING_REQUEST_FULL_PATH_AND_FILE_NAME);
        var result = await Task.Run(() => Everything_Query(true));
        if (!result)
        {
            ErrorCode errorcode = (ErrorCode)Everything_GetLastError();
            Debug.Log($"Everything_GetLastError: {errorcode}");
            return new(0);
        }
        uint count = Everything_GetNumResults();
        List<ProjectInfo> items = new((int)count);
        // log count
        Debug.Log($"Everything_GetNumResults: {count}");
        for (uint i = 0; i < count; i++)
        {
            var info = new ProjectInfo() { index = i };
            CollectProjectInformation(info);
            items.Add(info);
        }
        // remove the project that is in the handled list
        for (int i = 0; i < items.Count; i++)
        {
            var project = items[i];
            if (GameManager.IsProjectHandled(project.path))
            {
                items.RemoveAt(i);
                i--;
            }
        }

        return items;
    }

    public static void CollectProjectInformation(ProjectInfo item)
    {
        try
        {
            StringBuilder sb = new StringBuilder(500);
            Everything_GetResultFullPathName(item.index, sb, (uint)sb.Capacity);
            string path = sb.ToString();
            path = path.Replace("/", "\\");
            path = path.Substring(0, path.LastIndexOf("Library") - 1);
            item.projectName = Path.GetFileName(path);
            item.path = path;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        Everything_GetResultDateModified(item.index, out var fileTime);
        item.lastModified = DateTime.FromFileTime(fileTime);
        // Check if Library folder is required to be deleted
        // �����Ŀ�ڰ������У�����Ҫɾ�� Library �ļ���
        item.isLibraryDeleteRequired = !GameManager.IsProjectInWhiteList(item.path);
    }

    public void Dispose()
    {
        if (!isCustomProcess)
        {
            Everything_Exit();
        }
        process?.Dispose();
        process = null;
    }

    internal void Reset()
    {
        Everything_Reset();
    }

    // process of everything,if null, will start a new process
    // if is opened by this class, will close it when dispose,otherwise will not close it
    Process process;
    SynchronizationContext context;
    bool isCustomProcess;// Everything ���û������ģ�
}

[Serializable]
public class ProjectInfo
{
    public uint index;
    public string projectName;
    public string path;
    public bool isLibraryDeleteRequired = true;
    public DateTime lastModified;
    public override string ToString()
    {
        return $"{projectName} - {path} -  {lastModified:yyyy-MM-dd}";
    }
}

public static class EverythingWrapper
{
#if PLATFORM_ARCH_64
    public const string EverythingDLL = "Everything64.dll";
#elif PLATFORM_ARCH_32
        public const string EverythingDLL = "Everything32.dll";
#endif
    public const int EVERYTHING_REQUEST_PATH = 0x00000002;
    public const int EVERYTHING_REQUEST_DATE_MODIFIED = 0x00000040;
    public const int EVERYTHING_REQUEST_FULL_PATH_AND_FILE_NAME = 0x00000004;

    #region Version
    public static Version GetVersion()
    {
        var major = Everything_GetMajorVersion();
        var minor = Everything_GetMinorVersion();
        var revision = Everything_GetRevision();
        var build = Everything_GetBuildNumber();
        return new Version((int)major, (int)minor, (int)build, (int)revision);
    }
    [DllImport(EverythingDLL)]
    public static extern uint Everything_GetMajorVersion();
    [DllImport(EverythingDLL)]
    public static extern uint Everything_GetMinorVersion();
    [DllImport(EverythingDLL)]
    public static extern uint Everything_GetRevision();
    [DllImport(EverythingDLL)]
    public static extern uint Everything_GetBuildNumber();
    #endregion

    #region Lifecycle
    [DllImport(EverythingDLL)]
    public static extern bool Everything_IsDBLoaded();
    [DllImport(EverythingDLL)]
    public static extern bool Everything_UpdateAllFolderIndexes();
    //rebuilt the database
    [DllImport(EverythingDLL)]
    public static extern bool Everything_RebuildDB();
    [DllImport(EverythingDLL)]
    public static extern void Everything_Exit();
    #endregion


    #region Settings
    [DllImport(EverythingDLL)]
    public static extern void Everything_SetRequestFlags(uint dwRequestFlags);
    [DllImport(EverythingDLL)]
    public static extern void Everything_SetRegex(bool bSetRegex);
    [DllImport(EverythingDLL)]
    public static extern bool Everything_GetRegex();
    #endregion

    #region Search
    [DllImport(EverythingDLL, CharSet = CharSet.Unicode)]
    public static extern void Everything_SetSearch(string lpSearchString);
    [DllImport(EverythingDLL)]
    public static extern bool Everything_Query(bool bWait);
    #endregion

    #region Result
    [DllImport(EverythingDLL, CharSet = CharSet.Unicode)]
    public static extern void Everything_GetResultFullPathName(uint nIndex, StringBuilder lpString, uint nMaxCount);
    [DllImport(EverythingDLL)]
    public static extern bool Everything_GetResultDateModified(uint nIndex, out long lpFileTime);
    [DllImport(EverythingDLL)]
    public static extern uint Everything_GetNumResults();
    [DllImport(EverythingDLL)]
    public static extern uint Everything_GetLastError();
    [DllImport(EverythingDLL)] //reset
    public static extern void Everything_Reset();
    #endregion
}
