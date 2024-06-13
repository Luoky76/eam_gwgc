using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Reflection;
using System.Text;

namespace Gksyb.Common
{
    /// <summary>
    /// 验证码帮助类
    /// </summary>
    public class VerifyCodeHelper
    {
        private static readonly List<FontFamily> _fontFamilies;

        static VerifyCodeHelper()
        {
            _fontFamilies = new List<FontFamily>();
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            var collection = new FontCollection();
            foreach (var name in names)
            {
                _fontFamilies.Add(collection.Add(assembly.GetManifestResourceStream(name)));
            }
        }

        public static byte[] GetVerifyCode(out string code, ValidateCodeType codeType = ValidateCodeType.NumberAndLetter, int length = 4, int codeW = 80, int codeH = 30)
        {
            return new VerifyCodeHelper().GetCode(out code, codeType, length, codeW, codeH);
        }

        /// <summary>
        /// 随机数
        /// </summary>
        private readonly Random random;

        private VerifyCodeHelper()
        {
            random = new Random(GuidHelper.NewShortId().GetHashCode());
        }

        /// <summary>
        /// 获取指定长度的验证码字符串
        /// </summary>
        /// <param name="length"></param>
        /// <param name="codeType"></param>
        /// <returns></returns>
        private string GetCode(int length, ValidateCodeType codeType = ValidateCodeType.NumberAndLetter)
        {
            return codeType switch
            {
                ValidateCodeType.Number => GetRandomNums(length),
                ValidateCodeType.Chinese => GetRandomChinese(length),
                _ => GetRandomNumsAndLetters(length),
            };
        }

        /// <summary>
        /// 获取验证码图片
        /// </summary>
        /// <param name="code">验证码</param>
        /// <param name="codeType">验证码类型</param>
        /// <param name="length">验证码长度</param>
        /// <param name="codeW"></param>
        /// <param name="codeH"></param>
        /// <returns></returns>
        private byte[] GetCode(out string code, ValidateCodeType codeType = ValidateCodeType.NumberAndLetter, int length = 4, int codeW = 80, int codeH = 30)
        {
            int fontSize = (int)(codeH * 0.8), y = (codeH - fontSize) / 2;
            var text = code = GetCode(length, codeType);
            Color[] color = { Color.Black, Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Brown, Color.Brown, Color.DarkBlue };

            //创建画布
            var font = _fontFamilies[random.Next(_fontFamilies.Count)].CreateFont(fontSize, FontStyle.Bold);
            using var img = new Image<Rgba32>(codeW, codeH, Color.White);
            img.Mutate(ctx =>
            {
                for (int i = 0; i < text.Length; i++)
                {
                    ctx.DrawLine(new SolidPen(color[random.Next(color.Length)], 1), new Point(random.Next(codeW), random.Next(codeH)), new Point(random.Next(codeW), random.Next(codeH)));
                    ctx.DrawText(text[i].ToString(), font, color[random.Next(color.Length)], new Point(i * 18, y));
                }
            });
            using var ms = new MemoryStream();
            try
            {
                img.SaveAsPng(ms, new PngEncoder());
                return ms.ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetRandomNums(int length)
        {
            string result = string.Empty;
            for (int i = 0; i < length; i++)
            {
                result += random.Next(0, 9).ToString();
            }
            return result;
        }

        private string GetRandomNumsAndLetters(int length)
        {
            char[] allChars = { '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'd', 'e', 'f', 'h', 'k', 'm', 'n', 'r', 'x', 'y', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'W', 'X', 'Y' };
            string result = string.Empty;
            for (int i = 0; i < length; i++)
            {
                result += allChars[random.Next(allChars.Length)];
            }
            return result;
        }

        /// <summary>
        /// 获取汉字验证码
        /// </summary>
        /// <param name="length">验证码长度</param>
        /// <returns></returns>
        private string GetRandomChinese(int length)
        {
            //汉字编码的组成元素，十六进制数
            string[] baseStrs = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f" };
            Encoding encoding = Encoding.GetEncoding("GB2312");
            string result = null;

            //每循环一次产生一个含两个元素的十六进制字节数组，并放入bytes数组中
            //汉字由四个区位码组成，1、2位作为字节数组的第一个元素，3、4位作为第二个元素
            for (int i = 0; i < length; i++)
            {
                int index1 = random.Next(11, 14);
                string str1 = baseStrs[index1];

                int index2 = index1 == 13 ? random.Next(0, 7) : random.Next(0, 16);
                string str2 = baseStrs[index2];

                int index3 = random.Next(10, 16);
                string str3 = baseStrs[index3];

                int index4 = index3 == 10 ? random.Next(1, 16) : (index3 == 15 ? random.Next(0, 15) : random.Next(0, 16));
                string str4 = baseStrs[index4];

                //定义两个字节变量存储产生的随机汉字区位码
                byte b1 = Convert.ToByte(str1 + str2, 16);
                byte b2 = Convert.ToByte(str3 + str4, 16);
                byte[] bs = { b1, b2 };

                result += encoding.GetString(bs);
            }
            return result;
        }
    }

    /// <summary>
    /// 验证码类型
    /// </summary>
    public enum ValidateCodeType
    {
        /// <summary>
        /// 纯数值
        /// </summary>
        Number,

        /// <summary>
        /// 数值与字母的组合
        /// </summary>
        NumberAndLetter,

        /// <summary>
        /// 汉字
        /// </summary>
        Chinese
    }
}