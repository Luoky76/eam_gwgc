using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Gksyb.Common
{
    public class PasswordHelper
    {
        private const string _BANLIST = "QWERTY|QWEASD|ADMIN|PASSWORD|P@SSWORD|PASSWD|ILOVEYOU|5201314";

        private static readonly List<string> _BanList = _BANLIST.Split('|').ToList();

        /// <summary>
        /// 错误提示信息
        /// </summary>
        private const string errorMsg = "密码必须8个以上字符且包含数字、大小写字母、特殊字符中的三种。密码不能包含账号及4个以上连续或重复字符。密码不能包含常见密码。";

        /// <summary>
        /// 导向提示
        /// </summary>
        private static readonly string directionMsg = $"对不起，您的密码被判定为弱密码<br/><span style='color:red'>{ErrorMsg}</span><br/>请修改";

        public static string ErrorMsg => errorMsg;

        public static string DirectionMsg { get => directionMsg; }

        /// <summary>
        /// 密码规则验证 8个以上字符 包含数字、字母  不包含4个以上连续或重复字符
        /// </summary>
        /// <returns></returns>
        public static bool IsStrong(string password, string username)
        {
            var upperPassword = password.ToUpper();
            if (upperPassword.Contains(username.ToUpper())) return false;//不能包含账号
            if (_BanList.Any(c => upperPassword.Contains(c))) return false;//不能包含常见密码
            if (password.Length < 8) return false;//至少8个字符
            if (Regex.IsMatch(password, @"([0-9a-zA-Z])\1{3}")) return false; //不包含4个以上重复字符
            var strong = 0;
            if (Regex.IsMatch(password, @"(?=.*[0-9])")) strong++;//包含数字
            if (Regex.IsMatch(password, @"(?=.*[a-z])")) strong++;//包含小写字母
            if (Regex.IsMatch(password, @"(?=.*[A-Z])")) strong++;//包含大写字母
            if (Regex.IsMatch(password, @"(?=.*[^0-9a-zA-Z])")) strong++;//包含特殊字符
            if (strong < 3) return false;
            //不包含4个以上连续字符
            var count = 4;
            var chars = password.ToCharArray().Select(c => Convert.ToInt32(c)).ToArray();
            for (var i = chars.Length - 1; i >= 0; i--)
            {
                if ((i - count) >= 0)
                {
                    var isBreak = false;
                    var direction = 0;
                    for (var j = 1; j < count; j++)
                    {
                        var ch = chars[i - j];
                        if (ch < 48 || (ch > 57 && ch < 65) || (ch > 90 && ch < 97) || ch > 122)
                        {
                            isBreak = true;
                            break;
                        }
                        var diff = chars[i] - ch;
                        if (direction == 0)
                        {
                            direction = diff > 0 ? 1 : -1;
                        }
                        if (diff != j * direction)
                        {
                            isBreak = true;
                            break;
                        }
                    }
                    if (!isBreak) return false;
                }
            }
            return true;
        }

        private const string Lowercase = "abcdefghijklmnpqrstuvwxyz";
        private const string Uppercase = "ABCDEFGHIJKLMNPQRSTUVWXYZ";
        private const string Digits = "1234567890";
        private const string SpecialChars = "!@#$%&*";
        private static readonly string AllChars = $"{Digits}{Lowercase}{SpecialChars}{Uppercase}";

        /// <summary>
        /// 生成指定长度的随机密码
        /// </summary>
        /// <param name="length">密码长度默认8</param>
        /// <returns>随机密码</returns>
        public static string Generate(int length = 8)
        {
            MessageException.ThrowIf(length < 4, "长度必须大于4");
            var password = new char[length];
            password[0] = Lowercase[RandomNumberGenerator.GetInt32(Lowercase.Length)];
            password[1] = Uppercase[RandomNumberGenerator.GetInt32(Uppercase.Length)];
            password[2] = Digits[RandomNumberGenerator.GetInt32(Digits.Length)];
            password[3] = SpecialChars[RandomNumberGenerator.GetInt32(SpecialChars.Length)];
            for (int i = length - 1; i >= 4; i--)
            {
                password[i] = AllChars[RandomNumberGenerator.GetInt32(AllChars.Length)];
            }
            return new string(password.OrderBy(s => RandomNumberGenerator.GetInt32(length)).ToArray());
        }
    }
}