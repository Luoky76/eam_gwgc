namespace Gksyb.Model.Tree
{
    public interface ITreeable<T> where T : IEquatable<T>
    {
        /// <summary>
        /// 节点
        /// </summary>
        T ID { get; set; }

        /// <summary>
        /// 父节点
        /// </summary>
        T PARENTID { get; set; }

        /// <summary>
        /// 树节点（用于快速搜索）
        /// </summary>
        string TREENODE { get; set; }
    }
}