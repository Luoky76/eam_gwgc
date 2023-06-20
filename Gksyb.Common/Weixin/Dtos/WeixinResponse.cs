namespace Gksyb.Common.Weixin
{
    public class WeixinResponse
    {
        /// <summary>
        /// 是否错误
        /// </summary>
        public bool IsError
        {
            get
            {
                return ErrCode != 0;
            }
        }

        /// <summary>
        /// AccessToken过期
        /// </summary>
        public bool IsAccessTokenExpires
        {
            get
            {
                return ErrCode == 40001;
            }
        }

        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrCode { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrMsg { get; set; }

        public override string ToString()
        {
            return $"{ErrCode}:{ErrMsg}";
        }
    }
}