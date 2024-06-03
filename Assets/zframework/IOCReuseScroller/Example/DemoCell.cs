using UnityEngine;
using UnityEngine.UI;
using zFrame.UI;

public class DemoCell : BaseCell
{
    //UI
    public Text nameLabel;
    public Text genderLabel;
    public Text idLabel;
    public Button ply;

    //Model
    private ContactInfo _contactInfo;

   protected override void Start()
    {
        base.Start();
        GetComponent<Button>().onClick.AddListener(ButtonListener); //演示事件注册
    }

    //必须有这个方法或者类似于这样的方法，由继承自IReusableScrollViewController 的脚本驱动以刷新cell
    public void ConfigureCell(ContactInfo contactInfo)
    {
        _contactInfo = contactInfo;
        nameLabel.text = contactInfo.Name;
        genderLabel.text = contactInfo.Gender;
        idLabel.text = contactInfo.id;
    }

    
    private void ButtonListener()
    {
        Debug.Log("Index : " + dataIndex +  ", Name : " + _contactInfo.Name  + ", Gender : " + _contactInfo.Gender);
    }

    public enum MonitorSearchCellEvent
    {
        Play,
        Pined,
    }
}
