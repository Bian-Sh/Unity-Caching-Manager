using System.Collections.Generic;
using UnityEngine;
using zFrame.UI;

//Dummy Data model for demostraion
public struct ContactInfo
{
    public string Name;
    public string Gender;
    public string id;
}

/// <summary>
/// ReusableScrollViewController （RSVC）的数据载体演示脚本 
/// 可以是你的任意控制器啥的，控制反转按需配置自己的个性化 cell Item 
/// </summary>
public class ReusableScrollerDemo : MonoBehaviour, IReusableScrollViewDataSource
{
    [SerializeField]
    ReusableScrollViewController reusableScroll;

    [SerializeField]
    private int _dataLength;

    //Dummy ConfigData List
    private List<ContactInfo> _contactList = new List<ContactInfo>();
    public int Count => _contactList.Count;

    private void Start() //数据初始化务必放在 低于Awake 启动的Mono方法中
    {
        InitData();
        reusableScroll.DataSource = this;
    }

    //Initialising _contactList with dummy ConfigData 
    private void InitData()
    {
        if (_contactList != null) _contactList.Clear();

        string[] genders = { "Male", "Female" };
        for (int i = 0; i < _dataLength; i++)
        {
            ContactInfo obj = new ContactInfo();
            obj.Name = i + "_Name";
            obj.Gender = genders[Random.Range(0, 2)];
            obj.id = "item : " + i;
            _contactList.Add(obj);
        }
    }

    public void UpdateCell(BaseCell cell)
    {
        var item = cell as DemoCell;
        item.ConfigureCell(_contactList[cell.dataIndex]);
    }
}


