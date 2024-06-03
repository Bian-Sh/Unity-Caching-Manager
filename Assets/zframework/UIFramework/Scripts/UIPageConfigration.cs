using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace zFramework.UI
{
    using System;
#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(UIPageConfigration))]
    public class UIPageConfigrationEditor : Editor
    {
        UIPageConfigration pageConfigration;
        private void OnEnable()
        {
            pageConfigration = target as UIPageConfigration;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("生成Page名称"))
            {
                foreach (var item in pageConfigration.pageInfos)
                {
                    if (null != item.prefab)
                    {
                        item.name = item.prefab.name;
                    }
                    else
                    {
                        Debug.LogError("请先指定 Page预制体！");
                    }
                }
                EditorUtility.SetDirty(this);
            }
        }
    }
#endif

    /// <summary>
    /// 用于保存UI页面
    /// </summary>
    [CreateAssetMenu(fileName = "PageConfigriation", menuName = "Configration/Create PageConfigration", order = 1000)]
    public class UIPageConfigration : ScriptableObject
    {
        [System.Serializable]
        public class PageInfo
        {
            public string name;
            public GameObject prefab;
            [SerializeField]
            private string description;
        }
        public List<PageInfo> pageInfos;
        private List<BasePage> loadedPages = new List<BasePage>(); // 加载过了的页面

        /// <summary>
        /// 获取 指定类型的页面
        /// </summary>
        /// <typeparam name="T">页面类型</typeparam>
        /// <returns></returns>
        public T GetPage<T>() where T : BasePage
        {
            BasePage page = null;
            Predicate<PageInfo> predicate = v =>
            {
                page = v.prefab.GetComponent<BasePage>();
                return (null != page) && page.GetType() == typeof(T);
            };

            //   page = loadedPages.Find(v => v is T);
            page = loadedPages.Find(v => v.GetType() == typeof(T));
            if (null == page)
            {
                Debug.Log("请求的页面未加载过 ，加载。。。" + typeof(T).ToString());
                PageInfo pageInfo = pageInfos.Find(predicate);
                if (null != pageInfo)
                {
                    GameObject go = GameObject.Instantiate(pageInfo.prefab) as GameObject;
                    go.name = typeof(T).ToString();
                    page = go.GetComponent<BasePage>();
                    loadedPages.Add(page);
                }
                else
                {
                    Debug.LogWarning("请求的页面未找到配置数据 ：" + typeof(T).ToString());
                }
            }
            else
            {

            }
            return page as T;
        }

        /// <summary>
        /// 回收页面
        /// </summary>
        /// <param name="page">指定的页面</param>
        public void RecyclePage(BasePage page)
        {
            if (null != page && !loadedPages.Contains(page))
            {
                page.Hide();
                loadedPages.Add(page);
            }
        }

        internal void Init()
        {
            loadedPages.Clear();
        }
    }
}
