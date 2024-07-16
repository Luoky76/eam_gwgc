using Microsoft.AspNetCore.Http;

namespace Gksyb.Common.Mvc.Dtos
{
    /// <summary>
    /// 文件保存请求
    /// </summary>
    public class FormFileRequest
    {
        /// <summary>
        ///web路径
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 硬盘路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 映射路径
        /// </summary>
        public string MapPath { get; set; }

        /// <summary>
        /// 跳过文件哈希判断
        /// </summary>
        public bool IgnoreHash { get; set; }

        /// <summary>
        /// 上传文件
        /// </summary>
        public IFormFile FormFile { get; set; }
    }
}