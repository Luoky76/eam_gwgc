using System.Net;
using System.Net.Mail;

namespace Gksyb.Common
{
    /// <summary>
    /// 邮件帮助类
    /// </summary>
    public class MailHelper
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="server">邮件服务器地址</param>
        /// <param name="userName">发件人</param>
        /// <param name="password">密码</param>
        /// <param name="displayName">发件人显示名称</param>
        /// <returns></returns>
        public static MailHelper GetInstance(string server, string userName, string password, string displayName)
        {
            return new MailHelper(server, userName, password, displayName);
        }

        /// <summary>
        /// 邮件帮助类
        /// </summary>
        /// <param name="server">邮件服务器地址</param>
        /// <param name="userName">发件人</param>
        /// <param name="password">密码</param>
        /// <param name="displayName">发件人显示名称</param>
        public MailHelper(string server, string userName, string password, string displayName)
        {
            MailServer = server;
            MailUserName = userName;
            MailPassword = password;
            DisplayName = displayName;
        }

        /// <summary>
        /// 邮件服务器地址
        /// </summary>
        public string MailServer { get; set; }

        /// <summary>
        /// 发件人
        /// </summary>
        public string MailUserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string MailPassword { get; set; }

        /// <summary>
        /// 发件人显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="to">接收人</param>
        /// <param name="subject">主题</param>
        /// <param name="body">内容</param>
        /// <param name="attachments">附件</param>
        /// <param name="cc">抄送</param>
        /// <param name="encoding">编码</param>
        /// <param name="isBodyHtml">内容是否是html</param>
        /// <param name="enableSsl">启用ssl</param>
        public void Send(List<MailAddress> to, string subject, string body, List<Attachment> attachments, List<MailAddress> cc = null, Encoding encoding = null, bool isBodyHtml = true, bool enableSsl = true)
        {
            cc ??= new List<MailAddress>();
            encoding ??= Encoding.UTF8;
            var message = new MailMessage
            {
                From = new MailAddress(MailUserName, DisplayName),
                Subject = subject,
                SubjectEncoding = encoding,
                Body = body,
                BodyEncoding = encoding,
                IsBodyHtml = isBodyHtml,
            };
            (to ?? new List<MailAddress>()).ForEach(c => message.To.Add(c));
            (cc ?? new List<MailAddress>()).ForEach(c => message.CC.Add(c));
            (attachments ?? new List<Attachment>()).ForEach(c => message.Attachments.Add(c));
            using var client = new SmtpClient(MailServer)
            {
                Credentials = new NetworkCredential(MailUserName, MailPassword),
                EnableSsl = enableSsl
            };
            client.Send(message);
        }
    }
}