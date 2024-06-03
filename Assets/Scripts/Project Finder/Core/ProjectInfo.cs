using System;
public class ProjectInfo
{
    public long index;
    public string fullPath;
    public string projectName;
    public long size;
    public DateTime modified;
    // if the project is in the white list , Library Folder will not delete
    public bool isInWhiteList; 
    public override string ToString()
    {
        return $"ProjectInfo: {projectName} \n{fullPath}\nsize {size}\nlast modified {modified}";
    }
}
