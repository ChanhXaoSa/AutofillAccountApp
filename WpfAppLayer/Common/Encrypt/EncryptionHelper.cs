using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppLayer.Common.Encrypt
{
    public class EncryptionHelper
    {
        private static readonly string key = "ChanhDayVjpPro@123";

        public static string Encrypt(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (Aes aes = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                aes.Key = pdb.GetBytes(32);
                aes.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(plainTextBytes, 0, plainTextBytes.Length);
                        cs.Close();
                    }
                    plainText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return plainText;
        }

        public static string Decrypt(string encryptedText)
        {
            byte[] encryptedTextBytes = Convert.FromBase64String(encryptedText);
            using (Aes aes = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                aes.Key = pdb.GetBytes(32);
                aes.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedTextBytes, 0, encryptedTextBytes.Length);
                        cs.Close();
                    }
                    encryptedText = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            return encryptedText;
        }
    }
}
