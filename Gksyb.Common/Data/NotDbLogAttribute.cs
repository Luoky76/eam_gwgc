namespace Chloe.Annotations
{
    /// <summary>
    /// 不写入数据库日志
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NotDbLogAttribute : Attribute
    {
    }
}