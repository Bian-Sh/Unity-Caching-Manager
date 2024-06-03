using System.Threading.Tasks;
using UnityEngine;

namespace zFramework.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BasePage : MonoBehaviour, IPageBehaviours
    {
        [HideInInspector]
        public CanvasGroup canvasGroup;
        #region  Mono Function
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void Update() { }
        public virtual void Awake() => canvasGroup = GetComponent<CanvasGroup>();
        public virtual void Start() { }
        public virtual void OnDestroy() { }
        #endregion
        /// <summary>
        /// 当前页面是否为弹窗模式
        /// </summary>
        public bool isPopUpStyle = false;
        /// <summary>
        /// 界面停留(禁用交互)
        /// </summary>
        public virtual void OnPause()
        {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = false;
        }
        /// <summary>
        /// 界面继续(可以交互)
        /// </summary>
        public virtual void OnResume(params object[] args)
        {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// 单纯的隐藏页面
        /// </summary>
        public void Hide()
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// 界面被显示
        /// </summary>
        public virtual async Task OnEnter(params object[] args)
        {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            await Task.Yield();
        }

        /// <summary>
        /// 界面关闭
        /// </summary>
        public virtual async Task OnExit()
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            await Task.Yield();
        }
    }
}
