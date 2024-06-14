using Org.BouncyCastle.Asn1.GM;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;
using System.Security.Cryptography;
using System.Text;

namespace Gksyb.Common
{
    /// <summary>
    /// 密码学帮助类
    /// </summary>
    public sealed class CryptographyHelper
    {
        #region ========加密========

        /// <summary>
        /// 前端加密
        /// </summary>
        /// <returns></returns>
        public static string EncryptFront(string text)
        {
            text = ToBase64(text);
            var length = text.Length;
            if (length < 2) return text;
            var random = new Random(GuidHelper.NewShortId().GetHashCode());
            var value = text.Substring(random.Next(0, length - 1), 1);
            var index = (text.Length + 1) / 2;
            text = $"{text[index..]}{value}{text[..index]}";
            return text;
        }

        /// <summary>
        /// 前端解密
        /// </summary>
        /// <returns></returns>
        public static string DecryptFront(string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text)) return text;
                var index = (text.Length + 1) / 2;
                if (index > 0) text = $"{text[index..]}{text[..(index - 1)]}";
                return FromBase64(text);
            }
            catch
            {
                throw new MessageException("解析参数失败");
            }
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <returns></returns>
        public static string Encrypt(string text)
        {
            return EncryptDes64(text);
        }

        /// <summary>
        /// des64加密
        /// </summary>
        /// <returns></returns>
        public static string EncryptDes64(string text)
        {
            return EncryptDes64(text, "GKSYYB");
        }

        /// <summary>
        /// des64加密
        /// </summary>
        /// <returns></returns>
        public static string EncryptDes64(string text, string key, Encoding encoding = null, Action<DES> action = null)
        {
            encoding ??= Encoding.UTF8;
            using var des = DES.Create();
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.Zeros;
            des.Key = encoding.GetByteCount(key) <= 8 ? encoding.GetBytes(key.PadRight(8 - encoding.GetByteCount(key) + key.Length, '\0'))
                : GetMd5(encoding.GetBytes(key)).Skip(8).Take(8).ToArray();
            des.IV = des.Key;
            action?.Invoke(des);
            var inputByteArray = encoding.GetBytes(text);
            using ICryptoTransform icr = des.CreateEncryptor();
            return Convert.ToBase64String(icr.TransformFinalBlock(inputByteArray, 0, inputByteArray.Length));
        }

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string EncryptDes(string text)
        {
            return EncryptDes(text, "gksyyb");
        }

        /// <summary>
        /// DES加密
        /// </summary>
        /// <returns></returns>
        public static string EncryptDes(string text, string key, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            using var des = DES.Create();
            des.Padding = PaddingMode.PKCS7;
            des.Key = GetMd5(encoding.GetBytes(key)).Take(8).ToArray();
            des.IV = des.Key;
            var inputByteArray = encoding.GetBytes(text);
            using ICryptoTransform icr = des.CreateEncryptor();
            return Convert.ToHexString(icr.TransformFinalBlock(inputByteArray, 0, inputByteArray.Length));
        }

        /// <summary>
        /// AES加密
        /// </summary>
        /// <returns></returns>
        public static string EncryptAES(string text, string key, string iv = null, Encoding encoding = null, CipherMode? mode = null, PaddingMode? padding = null, Action<Aes> action = null)
        {
            encoding ??= Encoding.UTF8;
            var keys = Encoding.ASCII.GetBytes(GetMd5(key));
            var ivs = (iv == null ? keys.Skip(8).Take(16).ToArray() : GetMd5(encoding.GetBytes(iv)));
            return Convert.ToBase64String(EncryptAES(encoding.GetBytes(text), keys, ivs, mode, padding, action));
        }

        /// <summary>
        /// AES加密
        /// </summary>
        /// <returns></returns>
        public static byte[] EncryptAES(byte[] text, byte[] key, byte[] iv = null, CipherMode? mode = null, PaddingMode? padding = null, Action<Aes> action = null)
        {
            using var aes = Aes.Create();
            aes.Mode = mode ?? CipherMode.CBC;
            aes.Padding = padding ?? PaddingMode.PKCS7;
            aes.Key = key;
            if (iv == null)
            {
                iv = new byte[aes.IV.Length];
                Array.Copy(key, iv, iv.Length);
            }
            aes.IV = iv;
            action?.Invoke(aes);
            using var transform = aes.CreateEncryptor();
            return transform.TransformFinalBlock(text, 0, text.Length);
        }

        #endregion ========加密========

        #region ========解密========

        /// <summary>
        /// 解密
        /// </summary>
        /// <returns></returns>
        public static string Decrypt(string text)
        {
            return DecryptDes64(text);
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <returns></returns>
        public static string DecryptDes(string text)
        {
            return DecryptDes(text, "gksyyb");
        }

        /// <summary>
        /// des64解密
        /// </summary>
        /// <returns></returns>
        public static string DecryptDes64(string text)
        {
            return DecryptDes64(text, "GKSYYB");
        }

        /// <summary>
        /// des64解密
        /// </summary>
        /// <returns></returns>
        public static string DecryptDes64(string text, string key, Encoding encoding = null, Action<DES> action = null)
        {
            encoding ??= Encoding.UTF8;
            using var des = DES.Create();
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.Zeros;
            des.Key = encoding.GetByteCount(key) <= 8 ? encoding.GetBytes(key.PadRight(8 - encoding.GetByteCount(key) + key.Length, '\0'))
                : GetMd5(encoding.GetBytes(key)).Skip(8).Take(8).ToArray();
            des.IV = des.Key;
            action?.Invoke(des);
            var inputByteArray = Convert.FromBase64String(text);
            using ICryptoTransform icr = des.CreateDecryptor();
            return encoding.GetString(icr.TransformFinalBlock(inputByteArray, 0, inputByteArray.Length)).TrimEnd('\0');
        }

        /// <summary>
        /// des解密
        /// </summary>
        /// <returns></returns>
        public static string DecryptDes(string text, string key, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            using var des = DES.Create();
            des.Padding = PaddingMode.PKCS7;
            des.Key = GetMd5(encoding.GetBytes(key)).Take(8).ToArray();
            des.IV = des.Key;
            var inputByteArray = Convert.FromHexString(text);
            using ICryptoTransform icr = des.CreateDecryptor();
            return encoding.GetString(icr.TransformFinalBlock(inputByteArray, 0, inputByteArray.Length)).TrimEnd('\0');
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <returns></returns>
        public static string DecryptAES(string text, string key, string iv = null, Encoding encoding = null, CipherMode? mode = null, PaddingMode? padding = null, Action<Aes> action = null)
        {
            encoding ??= Encoding.UTF8;
            var keys = Encoding.ASCII.GetBytes(GetMd5(key));
            var ivs = (iv == null ? keys.Skip(8).Take(16).ToArray() : GetMd5(encoding.GetBytes(iv)));
            return encoding.GetString(DecryptAES(Convert.FromBase64String(text), keys, ivs, mode, padding, action));
        }

        /// <summary>
        /// AES解密
        /// </summary>
        public static byte[] DecryptAES(byte[] text, byte[] key, byte[] iv = null, CipherMode? mode = null, PaddingMode? padding = null, Action<Aes> action = null)
        {
            using var aes = Aes.Create();
            aes.Mode = mode ?? CipherMode.CBC;
            aes.Padding = padding ?? PaddingMode.PKCS7;
            aes.Key = key;
            if (iv == null)
            {
                iv = new byte[aes.IV.Length];
                Array.Copy(key, iv, iv.Length);
            }
            aes.IV = iv;
            action?.Invoke(aes);
            using var transform = aes.CreateDecryptor();
            return transform.TransformFinalBlock(text, 0, text.Length);
        }

        /// <summary>
        /// AesGcm解密 new AesCcm(key)
        /// </summary>
        /// <returns></returns>
        public static string AesGcmDecrypt(string associatedData, string nonce, string ciphertext, string key, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var gcmBlockCipher = new GcmBlockCipher(new AesEngine());
            var aeadParameters = new AeadParameters(new KeyParameter(encoding.GetBytes(key)), 128, encoding.GetBytes(nonce), encoding.GetBytes(associatedData));
            gcmBlockCipher.Init(false, aeadParameters);
            var data = Convert.FromBase64String(ciphertext);
            var plaintext = new byte[gcmBlockCipher.GetOutputSize(data.Length)];
            var length = gcmBlockCipher.ProcessBytes(data, 0, data.Length, plaintext, 0);
            gcmBlockCipher.DoFinal(plaintext, length);
            return Encoding.UTF8.GetString(plaintext);
        }

        #endregion ========解密========

        #region 哈希值

        /// <summary>
        /// 获取字符串的MD5哈希值，默认编码为<see cref="Encoding.UTF8"/>
        /// </summary>
        public static string GetMd5(string value, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            byte[] bytes = encoding.GetBytes(value);
            return Convert.ToHexString(GetMd5(bytes));
        }

        /// <summary>
        /// 获取字节数组的MD5哈希值
        /// </summary>
        public static byte[] GetMd5(byte[] bytes) => MD5.HashData(bytes);

        /// <summary>
        /// 获取字符串的SHA1哈希值，默认编码为<see cref="Encoding.UTF8"/>
        /// </summary>
        public static string GetSha1(string value, Encoding encoding = null)
        {
            return Convert.ToHexString(SHA1.HashData((encoding ?? Encoding.UTF8).GetBytes(value)));
        }

        /// <summary>
        /// 获取字符串的Sha256哈希值，默认编码为<see cref="Encoding.UTF8"/>
        /// </summary>
        public static string GetSha256(string value, Encoding encoding = null)
        {
            return Convert.ToHexString(SHA256.HashData((encoding ?? Encoding.UTF8).GetBytes(value)));
        }

        /// <summary>
        /// 获取字符串的Sha512哈希值，默认编码为<see cref="Encoding.UTF8"/>
        /// </summary>
        public static string GetSha512(string value, Encoding encoding = null)
        {
            return Convert.ToHexString(SHA512.HashData((encoding ?? Encoding.UTF8).GetBytes(value)));
        }

        /// <summary>
        /// 获取字符串的SM3哈希值，默认编码为<see cref="Encoding.UTF8"/>
        /// </summary>
        public static string GetSM3(string value, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return Convert.ToHexString(GetSM3(encoding.GetBytes(value)));
        }

        /// <summary>
        /// 获取SM3哈希值
        /// </summary>
        public static byte[] GetSM3(byte[] bytes)
        {
            var sm3 = new SM3Digest();
            sm3.BlockUpdate(bytes, 0, bytes.Length);
            var hashBytes = new byte[sm3.GetDigestSize()];
            sm3.DoFinal(hashBytes, 0);
            return hashBytes;
        }

        /// <summary>
        /// base64编码
        /// </summary>
        /// <returns></returns>
        public static string ToBase64(string text, Encoding encoding = null)
        {
            return Convert.ToBase64String((encoding ?? Encoding.UTF8).GetBytes(text));
        }

        /// <summary>
        /// base64解码
        /// </summary>
        /// <returns></returns>
        public static string FromBase64(string text, Encoding encoding = null)
        {
            return (encoding ?? Encoding.UTF8).GetString(Convert.FromBase64String(text));
        }

        #endregion 哈希值

        #region RSA

        /// <summary>
        /// RSA 私钥签名
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="encoding">编码</param>
        /// <param name="hashAlgorithm">哈希算法</param>
        /// <param name="padding">填充模式</param>
        /// <returns></returns>
        public static string RSASign(string text, string privateKey, Encoding encoding = null, HashAlgorithmName? hashAlgorithm = null, RSASignaturePadding padding = null)
        {
            encoding ??= Encoding.UTF8;
            if (hashAlgorithm == null) hashAlgorithm = HashAlgorithmName.SHA256;
            if (padding == null) padding = RSASignaturePadding.Pkcs1;
            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out int _);
            return Convert.ToBase64String(rsa.SignData(encoding.GetBytes(text), hashAlgorithm.Value, padding));
        }

        /// <summary>
        /// RSA 公钥验签
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="signedString">签名</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="encoding">编码</param>
        /// <param name="hashAlgorithm">哈希算法</param>
        /// <param name="padding">填充模式</param>
        public static bool RSAVerify(string text, string signedString, string publicKey, Encoding encoding = null, HashAlgorithmName? hashAlgorithm = null, RSASignaturePadding padding = null)
        {
            encoding ??= Encoding.UTF8;
            if (hashAlgorithm == null) hashAlgorithm = HashAlgorithmName.SHA256;
            if (padding == null) padding = RSASignaturePadding.Pkcs1;
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out int _);
            return rsa.VerifyData(encoding.GetBytes(text), Convert.FromBase64String(signedString), hashAlgorithm.Value, padding);
        }

        /// <summary>
        /// RSA 公钥加密
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="encoding">编码 默认UTF-8</param>
        /// <param name="padding">填充模式</param>
        /// <returns></returns>
        public static string RSAEncrypt(string text, string publicKey, Encoding encoding = null, RSAEncryptionPadding padding = null)
        {
            encoding ??= Encoding.UTF8;
            if (padding == null) padding = RSAEncryptionPadding.Pkcs1;
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out int _);
            return Convert.ToBase64String(rsa.Encrypt(encoding.GetBytes(text), padding));
        }

        /// <summary>
        /// RSA 私钥解密
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="encoding">编码 默认UTF-8</param>
        /// <param name="padding">填充模式</param>
        /// <returns></returns>
        public static string RSADecrypt(string text, string privateKey, Encoding encoding = null, RSAEncryptionPadding padding = null)
        {
            encoding ??= Encoding.UTF8;
            if (padding == null) padding = RSAEncryptionPadding.Pkcs1;
            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out int _);
            return encoding.GetString(rsa.Decrypt(Convert.FromBase64String(text), padding));
        }

        #endregion RSA

        #region SM2

        private static readonly ECDomainParameters SM2_DOMAIN_PARAMS = new(GMNamedCurves.GetByName("SM2P256v1"));
        private static readonly byte[] SM2_DEFAULT_UID = new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38 };

        /// <summary>
        /// 生成SM2公私钥
        /// </summary>
        /// <returns></returns>
        public static (string, string) Sm2GenerateKey()
        {
            var (privateKey, publicKey) = Sm2GeneratePKCS8Key();
            privateKey = (PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey)) as ECPrivateKeyParameters).D.ToString(16);
            var sm2PublicKeyParams = PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey)) as ECPublicKeyParameters;
            string ecPublicKeyX = sm2PublicKeyParams.Q.XCoord.ToBigInteger().ToString(16);
            string ecPublicKeyY = sm2PublicKeyParams.Q.YCoord.ToBigInteger().ToString(16);
            publicKey = $"04{ecPublicKeyX}{ecPublicKeyY}";
            return (privateKey, publicKey);
        }

        /// <summary>
        /// 生成SM2公私钥
        /// </summary>
        /// <returns></returns>
        public static (string, string) Sm2GeneratePKCS8Key()
        {
            var gen = new ECKeyPairGenerator("EC");
            var keyGenParam = new ECKeyGenerationParameters(SM2_DOMAIN_PARAMS, new SecureRandom());
            gen.Init(keyGenParam);
            var keyPair = gen.GenerateKeyPair();

            var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);
            var serializedPrivateKey = privateKeyInfo.ToAsn1Object().GetEncoded();
            var privateKey = Convert.ToBase64String(serializedPrivateKey);

            var publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public);
            var serializedPublicKey = publicKeyInfo.ToAsn1Object().GetDerEncoded();
            var publicKey = Convert.ToBase64String(serializedPublicKey);
            return (privateKey, publicKey);
        }

        /// <summary>
        /// 私钥签名
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static string SM2Sign(string text, string privateKey, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var privateKeyParameters = new ECPrivateKeyParameters(new BigInteger(privateKey, 16), SM2_DOMAIN_PARAMS);
            var cipherBytes = SM2Sign(privateKeyParameters, encoding.GetBytes(text));
            return Convert.ToBase64String(cipherBytes);
        }

        /// <summary>
        /// 私钥签名
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static string SM2SignWithPKCS8(string text, string privateKey, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var privateKeyParameters = PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));
            var cipherBytes = SM2Sign(privateKeyParameters, encoding.GetBytes(text));
            return Convert.ToBase64String(cipherBytes);
        }

        /// <summary>
        /// 公钥验签
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="signedString">签名</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static bool SM2Verify(string text, string signedString, string publicKey, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var publicKeyParameters = ParseSM2PublicKey(publicKey);
            return SM2Verify(publicKeyParameters, encoding.GetBytes(text), Convert.FromBase64String(signedString));
        }

        /// <summary>
        /// 公钥验签
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="signedString">签名</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static bool SM2VerifyWithPKCS8(string text, string signedString, string publicKey, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var publicKeyParameters = PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return SM2Verify(publicKeyParameters, encoding.GetBytes(text), Convert.FromBase64String(signedString));
        }

        /// <summary>
        /// 公钥加密
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="publicKey">ECC公钥</param>
        /// <param name="encoding">编码 默认UTF-8</param>
        /// <returns></returns>
        public static string SM2Encrypt(string text, string publicKey, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var publicKeyParameters = ParseSM2PublicKey(publicKey);
            var cipherBytes = SM2Encrypt(publicKeyParameters, encoding.GetBytes(text));
            return Convert.ToBase64String(cipherBytes);
        }

        /// <summary>
        /// 公钥加密
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="publicKey">PKCS#8 公钥</param>
        /// <param name="encoding">编码 默认UTF-8</param>
        /// <returns></returns>
        public static string SM2EncryptWithPKCS8(string text, string publicKey, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var publicKeyParameters = PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            var cipherBytes = SM2Encrypt(publicKeyParameters, encoding.GetBytes(text));
            return Convert.ToBase64String(cipherBytes);
        }

        /// <summary>
        /// 公钥加密数据
        /// </summary>
        public static byte[] SM2Encrypt(ICipherParameters sm2PublicKeyParams, byte[] plainBytes)
        {
            var engine = new SM2Engine();
            engine.Init(true, new ParametersWithRandom(sm2PublicKeyParams, new SecureRandom()));
            return engine.ProcessBlock(plainBytes, 0, plainBytes.Length);
        }

        /// <summary>
        /// 私钥解密数据
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="privateKey">ECC私钥</param>
        /// <param name="encoding">编码 默认UTF-8</param>
        /// <returns></returns>
        public static string SM2Decrypt(string text, string privateKey, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var privateKeyParameters = new ECPrivateKeyParameters(new BigInteger(privateKey, 16), SM2_DOMAIN_PARAMS);
            var plainBytes = SM2Decrypt(privateKeyParameters, Convert.FromBase64String(text));
            return encoding.GetString(plainBytes);
        }

        /// <summary>
        /// 私钥解密数据
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="privateKey">PKCS#8私钥</param>
        /// <param name="encoding">编码 默认UTF-8</param>
        /// <returns></returns>
        public static string SM2DecryptWithPKCS8(string text, string privateKey, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var privateKeyParameters = PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));
            var plainBytes = SM2Decrypt(privateKeyParameters, Convert.FromBase64String(text));
            return encoding.GetString(plainBytes);
        }

        /// <summary>
        /// 私钥解密数据
        /// </summary>
        public static byte[] SM2Decrypt(ICipherParameters sm2PublicKeyParams, byte[] cipherBytes)
        {
            var engine = new SM2Engine();
            engine.Init(false, sm2PublicKeyParams);
            return engine.ProcessBlock(cipherBytes, 0, cipherBytes.Length);
        }

        /// <summary>
        /// 私钥签名
        /// </summary>
        private static byte[] SM2Sign(ICipherParameters sm2PrivateKeyParams, byte[] msgBytes)
        {
            var signer = SignerUtilities.GetSigner("SM3withSM2");
            signer.Init(true, new ParametersWithID(sm2PrivateKeyParams, SM2_DEFAULT_UID));
            signer.BlockUpdate(msgBytes, 0, msgBytes.Length);
            return signer.GenerateSignature();
        }

        /// <summary>
        /// 公钥验签
        /// </summary>
        private static bool SM2Verify(ICipherParameters sm2PublicKeyParams, byte[] msgBytes, byte[] signBytes)
        {
            var signer = SignerUtilities.GetSigner("SM3withSM2");
            signer.Init(false, new ParametersWithID(sm2PublicKeyParams, SM2_DEFAULT_UID));
            signer.BlockUpdate(msgBytes, 0, msgBytes.Length);
            return signer.VerifySignature(signBytes);
        }

        /// <summary>
        /// 处理ecc公钥
        /// </summary>
        /// <param name="hexKey"></param>
        /// <returns></returns>
        private static ECPublicKeyParameters ParseSM2PublicKey(string hexKey)
        {
            var ecPublicKeyBytes = Hex.Decode(hexKey);
            var keyLength = 64;
            var offset = ecPublicKeyBytes.FirstOrDefault() == 0x04 ? 1 : 0;
            MessageException.ThrowIf(ecPublicKeyBytes.Length != (keyLength + offset), $"错误的密钥长度");

            var ecPublicKeyXBytes = new byte[keyLength / 2];
            var ecPublicKeyYBytes = new byte[keyLength / 2];
            Buffer.BlockCopy(ecPublicKeyBytes, offset, ecPublicKeyXBytes, 0, ecPublicKeyXBytes.Length);
            Buffer.BlockCopy(ecPublicKeyBytes, ecPublicKeyXBytes.Length + offset, ecPublicKeyYBytes, 0, ecPublicKeyYBytes.Length);

            var ecPublicKeyParamsX = new BigInteger(Hex.ToHexString(ecPublicKeyXBytes), 16);
            var ecPublicKeyParamsY = new BigInteger(Hex.ToHexString(ecPublicKeyYBytes), 16);
            return new ECPublicKeyParameters(SM2_DOMAIN_PARAMS.Curve.CreatePoint(ecPublicKeyParamsX, ecPublicKeyParamsY), SM2_DOMAIN_PARAMS);
        }

        #endregion SM2

        #region SM4

        /// <summary>
        /// SM4加密
        /// </summary>
        /// <returns></returns>
        public static string EncryptSM4(string text, string key, string iv = null, Encoding encoding = null, CipherMode? mode = null, PaddingMode? padding = null)
        {
            encoding ??= Encoding.UTF8;
            var keys = GetSM3(encoding.GetBytes(key));
            var ivs = iv == null ? keys.TakeLast(16).ToArray() : GetSM3(encoding.GetBytes(iv)).Take(16).ToArray();
            return Convert.ToBase64String(EncryptSM4(encoding.GetBytes(text), keys.Take(16).ToArray(), ivs, mode, padding));
        }

        /// <summary>
        /// SM4加密
        /// </summary>
        /// <returns></returns>
        public static byte[] EncryptSM4(byte[] text, byte[] key, byte[] iv = null, CipherMode? mode = null, PaddingMode? padding = null)
        {
            mode ??= CipherMode.CBC;
            padding ??= PaddingMode.PKCS7;
            iv ??= key.Take(16).ToArray();
            var sm4KeyParams = ParameterUtilities.CreateKeyParameter("SM4", key);
            var sm4keyParamsWithIv = new ParametersWithIV(sm4KeyParams, iv);
            var cipher = CipherUtilities.GetCipher($"SM4/{mode.Value}/{padding.Value}");
            switch (mode)
            {
                case CipherMode.ECB:
                    cipher.Init(true, sm4KeyParams);
                    break;
                default:
                    cipher.Init(true, sm4keyParamsWithIv);
                    break;
            }
            return cipher.DoFinal(text);
        }

        /// <summary>
        /// SM4解密
        /// </summary>
        /// <returns></returns>
        public static string DecryptSM4(string text, string key, string iv = null, Encoding encoding = null, CipherMode? mode = null, PaddingMode? padding = null)
        {
            encoding ??= Encoding.UTF8;
            var keys = GetSM3(encoding.GetBytes(key));
            var ivs = iv == null ? keys.TakeLast(16).ToArray() : GetSM3(encoding.GetBytes(iv)).Take(16).ToArray();
            return encoding.GetString(DecryptSM4(Convert.FromBase64String(text), keys.Take(16).ToArray(), ivs, mode, padding));
        }

        /// <summary>
        /// SM4解密
        /// </summary>
        public static byte[] DecryptSM4(byte[] text, byte[] key, byte[] iv = null, CipherMode? mode = null, PaddingMode? padding = null)
        {
            mode ??= CipherMode.CBC;
            padding ??= PaddingMode.PKCS7;
            iv ??= key.Take(16).ToArray();
            var sm4KeyParams = ParameterUtilities.CreateKeyParameter("SM4", key);
            var sm4keyParamsWithIv = new ParametersWithIV(sm4KeyParams, iv);
            var cipher = CipherUtilities.GetCipher($"SM4/{Enum.GetName(mode.GetType(), mode)}/{Enum.GetName(padding.GetType(), padding)}");
            switch (mode)
            {
                case CipherMode.ECB:
                    cipher.Init(false, sm4KeyParams);
                    break;
                default:
                    cipher.Init(false, sm4keyParamsWithIv);
                    break;
            }
            return cipher.DoFinal(text);
        }

        #endregion SM4
    }
}