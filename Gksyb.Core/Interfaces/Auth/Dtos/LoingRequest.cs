using Gksyb.Core.Auth;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Gksyb.Model.Dtos
{
    public class LoginRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [ModelEncrypt]
        [Required(ErrorMessage = "用户名不能为空")]
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [ModelEncrypt]
        public string Password { get; set; }

        /// <summary>
        /// 验证码
        /// </summary>
        public string Verifycode { get; set; }

        /// <summary>
        /// 菜单应用名
        /// </summary>
        public string MenuAppname { get; set; }

        /// <summary>
        /// 角色应用名
        /// </summary>
        public string RoleAppname { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 用户代理
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// 设备唯一识别码
        /// </summary>
        public string IMEI { get; set; }

        /// <summary>
        ///来源
        /// </summary>
        [JsonIgnore]
        public string Source { get; set; }

        /// <summary>
        ///输入的密码
        /// </summary>
        [JsonIgnore]
        public string InputPassword { get; set; }

        /// <summary>
        /// 密码处理
        /// </summary>
        public LoginRequest PasswordHandle()
        {
            if (string.IsNullOrWhiteSpace(Password)) return this;
            InputPassword = Password;
            Password = UserSession.Encrypt(InputPassword);
            return this;
        }
    }
}