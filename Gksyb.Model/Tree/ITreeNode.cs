namespace Gksyb.Model.Tree
{
    public interface ITreeable
    {
        /// <summary>
        /// 节点
        /// </summary>
        string ID { get; set; }

        /// <summary>
        /// 父节点
        /// </summary>
        string PARENTID { get; set; }

        /// <summary>
        /// 树节点（用于快速搜索）
        /// </summary>
        string TREENODE { get; set; }
    }
}