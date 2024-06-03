using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace zFramework.UI
{
    public class UIManager :MonoBehaviour
    {
        public UIPageConfigration uIPageConfiguration;
        static UIManager Instance;

        protected  void Awake()
        {
            Instance = this;
            uIPageConfiguration.Init(); //非常需要清空这个数据，鬼知道什么时候加载了
            if (panelStack == null)
            {
                panelStack = new Stack<BasePage>();
            }
            billBoard = transform.Find("BillBoard");
            recycleBin = transform.Find("RecycleBin");
        }

        #region PanelRawData
        private static Stack<BasePage> panelStack;
        /// <summary>
        /// 当前页面
        /// </summary>
        public static BasePage CurrentPage { get; private set; }
        #endregion

        #region UIFramework Component
        [HideInInspector]
        public Transform billBoard, recycleBin;
        public static Transform BillBoard => Instance.billBoard;
        public static Transform RecycleBin => Instance.recycleBin;
        #endregion
        #region  UIFrameBehaviours
        /// <summary>
        /// 显示指定类型的页面
        /// </summary>
        /// <param name="panelType">页面类型</param>
        public static async Task<T> ShowPageAsync<T>(params object[] args) where T : BasePage
        {
            T page = Instance.uIPageConfiguration.GetPage<T>();
            await PushPanelAsync(page,args);
            return page;
        }

        #endregion

        /// <summary>
        /// 入栈 ,页面显示的第一步，就是数据的入栈
        /// </summary>
        private static async Task PushPanelAsync(BasePage target, params object[] args)
        {
            if (null == target)
            {
                Debug.LogError("入栈页面不得为空！");
                return;
            }
            CurrentPage = target;       //缓存当前游戏对象

            if (panelStack.Count > 0)
            {
                BasePage topPanel = panelStack.Peek();//只取出栈顶，不删除
                if (target.isPopUpStyle) //如果是弹窗，则之前的页面可以共存 
                {                                          //即：页面保持渲染，只是唤起 Pause() 
                    topPanel.OnPause();
                }
                else
                {
                    //否则页面互斥，只能显示最新压入的页面 ,即：页面移入回收区停止渲染，唤起 OnExit() 
                    // 当执行 toppanel 的 onexit 之前，currentPage 已经不是 toppanel, 这点很重要，如此便可以在 OnExit 时确认 是谁替代了自己的栈顶位置！
                    // 从而可以根据引起 toppanel 退出的 currentpage 不同而执行不通的操作。
                    await topPanel.OnExit(); //约定：OnExit 是 page 完成交换后才执行，而不是执行完才发生交换
                    topPanel.transform.SetParent(RecycleBin, false);
                }
            }

            panelStack.Push(CurrentPage);    //将页面压栈
            CurrentPage.transform.SetParent(BillBoard, false); //加载到画布渲染
            CurrentPage.transform.SetAsLastSibling(); //设为渲染的最上层
            await CurrentPage.OnEnter(args); //使用UniTask 实现“异步”调用，保证动画的有效执行(如果有的话)
        }

        /// <summary>
        /// 出栈
        /// </summary>
        public static async Task ClosePageAsync(params object[] args)
        {
            if (panelStack.Count <= 1)
            {
                return;
            }
            var topPanel = panelStack.Pop();//将页面从栈顶移除
             CurrentPage = panelStack.Peek(); //将当前页面指定为栈顶页面，这个逻辑顺序可以让你在 OnExit 时就知道接下来是谁的栈顶
            await topPanel.OnExit(); //约定：OnExit 是 page 完成交换后才执行，而不是执行完才发生交换
            topPanel.transform.SetParent(RecycleBin, false); //移出画布避免渲染
            if (topPanel.isPopUpStyle)
            {
                CurrentPage.OnResume(args);
            }
            else
            {
                CurrentPage.transform.SetParent(BillBoard, false);
                CurrentPage.transform.SetAsLastSibling();
                await CurrentPage.OnEnter(args);
            }
        }

#if UNITY_EDITOR
        void Reset()
        {

            //如果不存在billboard，则创建一个，设置为自身子物体
            billBoard = BuildUIFoundation("BillBoard");
            //如果不存在recycleBin，则创建一个，设置为自身子物体并隐藏
            recycleBin = BuildUIFoundation("RecycleBin", false);
            if (!uIPageConfiguration)
            {
                //从编辑器获取uIPageConfiguration
                var results = AssetDatabase.FindAssets("PageConfigriation", new string[] { "Assets/Resources/UI" });
                if (results.Length != 0)
                {
                    var pc = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(results[0]));
                    if (pc is UIPageConfigration uc)
                    {
                        uIPageConfiguration = uc;
                    }
                }
            }
            EditorUtility.SetDirty(gameObject);
        }

        /// <summary>
        /// 构建 UI 框架基础架构成员
        /// </summary>
        /// <param name="name">成员名</param>
        /// <param name="active">初始状态</param>
        private RectTransform BuildUIFoundation(string name, bool active = true)
        {
            var rect = (RectTransform)transform.Find(name) ?? new GameObject(name).AddComponent<RectTransform>();
            rect.SetParent(transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one * 0.5f;
            rect.sizeDelta = Vector2.zero;
            rect.gameObject.SetActive(active);
            return rect;
        }
#endif
    }
}
