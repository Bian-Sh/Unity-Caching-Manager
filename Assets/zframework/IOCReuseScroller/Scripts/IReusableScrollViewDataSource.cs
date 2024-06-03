namespace zFrame.UI
{
    /// <summary>
    /// RSVC = ReusableScrollViewController 
    /// RSVC 必须提供一个继承了 IReusableScrollviewDataSource 的数据源，可复用 Scrollview 通过此接口更新item数据
    /// 这个数据源可以是任意的脚本（包含但不限于你的各类Controller脚本）很方便的实现了控制反转，把控制权由Scrollview 给了用户，
    /// 方便同一个 RSVC 通用不同的CellItem ，SearchPanel 会用上
    /// </summary>
    public interface IReusableScrollViewDataSource
    {
        /// <summary>
        /// 为 ReusableScrollview提供的数据的实时长度
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 更新 cell item
        /// </summary>
        /// <param name="cell"></param>
        void UpdateCell(BaseCell cell);
    }
}
