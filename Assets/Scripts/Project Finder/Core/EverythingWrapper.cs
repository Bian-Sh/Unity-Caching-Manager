using System;
using System.Runtime.InteropServices;
using System.Text;

internal class EverythingWrapper
{
#if PLATFORM_ARCH_64
    private const string EverythingDLL = "Everything64.dll";
#else
    private const string EverythingDLL = "Everything32.dll";
#endif

    private const int EVERYTHING_OK = 0;
    private const int EVERYTHING_ERROR_MEMORY = 1;
    private const int EVERYTHING_ERROR_IPC = 2;
    private const int EVERYTHING_ERROR_REGISTERCLASSEX = 3;
    private const int EVERYTHING_ERROR_CREATEWINDOW = 4;
    private const int EVERYTHING_ERROR_CREATETHREAD = 5;
    private const int EVERYTHING_ERROR_INVALIDINDEX = 6;
    private const int EVERYTHING_ERROR_INVALIDCALL = 7;

    public enum FileInfoIndex
    {
        FileSize = 1,
        FolderSize,
        DateCreated,
        DateModified,
        DateAccessed,
        Attributes
    }

    #region Version
    [DllImport(EverythingDLL)]
    public static extern bool Everything_IsDBLoaded();
    [DllImport(EverythingDLL)]
    public static extern UInt32 Everything_GetMajorVersion();
    [DllImport(EverythingDLL)]
    public static extern UInt32 Everything_GetMinorVersion();
    [DllImport(EverythingDLL)]
    public static extern UInt32 Everything_GetRevision();
    [DllImport(EverythingDLL)]
    public static extern UInt32 Everything_GetBuildNumber();
    public static Version GetVersion()
    {
        var major = Everything_GetMajorVersion();
        var minor = Everything_GetMinorVersion();
        var build = Everything_GetBuildNumber();
        var revision = Everything_GetRevision();
        return new Version(Convert.ToInt32(major), Convert.ToInt32(minor), Convert.ToInt32(build), Convert.ToInt32(revision));
    }
    #endregion

    #region Settings
    [DllImport(EverythingDLL)]
    public static extern void Everything_SetRegex(bool bEnable);
    [DllImport(EverythingDLL)]
    public static extern void Everything_SetReplyID(UInt32 nId);
    [DllImport(EverythingDLL)]
    public static extern int Everything_SetSearch(string lpSearchString);
    [DllImport(EverythingDLL)]
    public static extern void Everything_SetRequestFlags(UInt32 dwRequestFlags);
    #endregion


    [DllImport(EverythingDLL)]
    public static extern void Everything_Reset();
    [DllImport(EverythingDLL)]
    public static extern int Everything_GetLastError();

    [DllImport(EverythingDLL)]
    public static extern bool Everything_Query(bool bWait);
    [DllImport(EverythingDLL)]
    public static extern UInt32 Everything_GetNumResults();
    [DllImport(EverythingDLL, CharSet = CharSet.Unicode)]
    public static extern void Everything_GetResultFullPathName(UInt32 nIndex, StringBuilder lpString, UInt32 nMaxCount);
    [DllImport(EverythingDLL)]
    public static extern bool Everything_GetResultDateModified(UInt32 nIndex, out long lpFileTime);
    [DllImport(EverythingDLL)]// refresh db,only save when everything(app) exit
    public static extern void Everything_UpdateAllFolderIndexes();
}
