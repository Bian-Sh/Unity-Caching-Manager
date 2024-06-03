using System.Threading.Tasks;
namespace zFramework.UI
{
    /// <summary>
    /// 所有页面的行为基准
    /// </summary>
    public interface IPageBehaviours 
    {
        /// <summary>
        /// 界面被显示
        /// </summary>
        Task OnEnter(params object[] args);
        /// <summary>
        /// 界面停留(禁用交互)
        /// </summary>
        void OnPause();
        /// <summary>
        /// 界面继续(可以交互)
        /// </summary>
        void OnResume(params object[] args);
        /// <summary>
        /// 界面关闭
        /// </summary>
        Task OnExit();
    }
}
