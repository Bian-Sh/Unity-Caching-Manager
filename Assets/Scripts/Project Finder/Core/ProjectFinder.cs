using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static EverythingWrapper;
using Debug = UnityEngine.Debug;

public class ProjectFinder : IDisposable
{
    private static int lastReplyId;
    private const uint DefaultSearchFlags = (uint)(RequestFlags.EVERYTHING_REQUEST_PATH
        | RequestFlags.EVERYTHING_REQUEST_FULL_PATH_AND_FILE_NAME
        | RequestFlags.EVERYTHING_REQUEST_DATE_MODIFIED);
    private readonly uint replyId;
    public ProjectFinder()
    {
        Interlocked.Increment(ref lastReplyId);
        this.replyId = Convert.ToUInt32(lastReplyId);
    }

    public ErrorCode LastErrorCode { get; set; }
    public Version Version => GetVersion();

    public bool IsReady() => Everything_IsDBLoaded();
    public void Reset() => Everything_Reset();
    public void Dispose() => this.Reset();
    public bool IsStarted()
    {
        Version version = GetVersion();
        return version.Major > 0;
    }

    static Process process;
    public bool StartService()
    {
        if (!IsStarted())
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Everything/Everything.exe");
            process = Process.Start(path, "-admin -startup");
            return IsStarted();
        }
        return true;
    }
    public void StopService()
    {
        if (process != null)
        {
            process.Kill();
        }
        process = null;
    }


    public async Task<List<ProjectInfo>> SearchProjectAsync()
    {
        Everything_SetReplyID(this.replyId);
        Everything_SetRegex(false);
        Everything_SetRequestFlags(DefaultSearchFlags);
        Everything_SetSearch("folder: Library child:EditorUserBuildSettings.asset|LastSceneManagerSetup.txt");

        Debug.Log("Searching...");
        var result = await Task.Run(() => Everything_Query(true));
        this.LastErrorCode = (ErrorCode)Everything_GetLastError();
        Debug.Log($"Search completed, errorcode {LastErrorCode}");

        if (result)
        {
            var numResults = Everything_GetNumResults();
            var list = new List<ProjectInfo>((int)numResults);
            for (int i = 0; i < numResults; i++)
            {
                try
                {
                    var info = CollectProjectInfo(i);
                    list.Add(info);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            Debug.Log($"{nameof(ProjectFinder)}: list count {list.Count}");
            return list;
        }
        else
        {
            return new(0);
        }
    }

    private ProjectInfo CollectProjectInfo(int index)
    {
        var info = new ProjectInfo();
        // full path
        var builder = new StringBuilder(260);
        Everything_SetReplyID(this.replyId);
        Everything_GetResultFullPathName((uint)index, builder, 260);
        var path = builder.ToString();
        path = path.Replace("/", "\\");
        info.fullPath = path.Substring(0, path.LastIndexOf("Library") - 1);
        // project name
        info.projectName = Path.GetFileName(info.fullPath);
        // modified
        Everything_GetResultDateModified((uint)index, out var date);
        if (date >= 0)
        {
            info.modified = DateTime.FromFileTime(date);
        }
        else
        {
            info.modified = File.GetLastWriteTime(info.fullPath);
        }
        // check white list

        return info;
    }
}
