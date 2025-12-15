using System;
using System.IO;
using System.Security.Cryptography;

namespace SecureFileTransferWeb.Services
{
    /// <summary>
    /// AES 加密解密模組
    /// 提供 AES-128 對稱式加密功能
    /// 
    /// 安全性注意事項 (Security Notice):
    /// 此實作使用硬編碼的金鑰與 IV，僅供教育與示範用途。
    /// 實際部署時，應使用以下安全措施：
    /// 1. 使用安全的金鑰管理系統 (Key Management System)
    /// 2. 實作金鑰交換協定 (如 Diffie-Hellman)
    /// 3. 定期輪替金鑰
    /// 4. 使用環境變數或安全配置儲存金鑰
    /// </summary>
    public class AesEncryption
    {
        // 128-bit (16 bytes) 金鑰 - 發送端與接收端必須相同
        // ⚠️ 警告：此為示範用金鑰，實際應用中請勿使用硬編碼金鑰
        private static readonly byte[] Key = new byte[16]
        {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10
        };

        // 128-bit (16 bytes) 初始向量 (IV) - 發送端與接收端必須相同
        private static readonly byte[] IV = new byte[16]
        {
            0x10, 0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09,
            0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01
        };

        /// <summary>
        /// 加密位元組陣列
        /// </summary>
        /// <param name="data">要加密的位元組陣列</param>
        /// <returns>加密後的位元組陣列</returns>
        public static byte[] Encrypt(byte[] data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(data, 0, data.Length);
                        csEncrypt.FlushFinalBlock();
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// 解密位元組陣列
        /// </summary>
        /// <param name="encryptedData">加密的位元組陣列</param>
        /// <returns>解密後的位元組陣列</returns>
        public static byte[] Decrypt(byte[] encryptedData)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (MemoryStream msOutput = new MemoryStream())
                {
                    csDecrypt.CopyTo(msOutput);
                    return msOutput.ToArray();
                }
            }
        }

        /// <summary>
        /// 解密並儲存為檔案
        /// </summary>
        /// <param name="encryptedData">加密的位元組陣列</param>
        /// <param name="outputFilePath">解密後要儲存的檔案路徑</param>
        public static void DecryptToFile(byte[] encryptedData, string outputFilePath)
        {
            byte[] decryptedData = Decrypt(encryptedData);
            File.WriteAllBytes(outputFilePath, decryptedData);
        }
    }
}
