using UnityEngine.UI;
using zFrame.UI;
public class ProjectInfoCell : BaseCell
{
    public Text projectname;
    public Text projectpath;
    public Text lastModified;
    public Toggle libraryDeleteRequire;
    public Button showInExplorer;
    public Button deleteButton;

    ProjectInfo projectInfo;

    protected override void Start()
    {
        base.Start();
        showInExplorer.onClick.AddListener(() =>
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select," + projectInfo.path);
        });
    }

    public void ConfigureCell(ProjectInfo info)
    {
        projectInfo = info;
        projectname.text = info.projectName;
        projectpath.text = info.path;
        lastModified.text = $"最后修改：{info.lastModified:yyyy-MM-dd}";
        libraryDeleteRequire.SetIsOnWithoutNotify(info.isLibraryDeleteRequired);
    }
}
