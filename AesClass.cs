using System;
using System.Security.Cryptography;
using System.Text;

namespace WpfApp1
{
    public class AesClass
    {
        public static string PadSalt(string salt, int minLength)
        {
            if (salt.Length >= minLength) return salt;
            return salt.PadRight(minLength, '0'); // Pad with zeros to the required length
        }

        public static string Encrypt(string plaintext, string password, string salt)
        {
            using (var aes = Aes.Create())
            {
                var key = DeriveKey(password, salt);
                aes.Key = key;
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                    var encryptedBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

                    // Combine IV and encrypted bytes
                    var result = new byte[aes.IV.Length + encryptedBytes.Length];
                    aes.IV.CopyTo(result, 0);
                    encryptedBytes.CopyTo(result, aes.IV.Length);

                    return Convert.ToBase64String(result);
                }
            }
        }

        public static string Decrypt(string encryptedText, string password, string salt)
        {
            using (var aes = Aes.Create())
            {
                var fullCipher = Convert.FromBase64String(encryptedText);

                var iv = new byte[16];
                var cipher = new byte[fullCipher.Length - iv.Length];

                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                var key = DeriveKey(password, salt);
                aes.Key = key;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    try
                    {
                        var decryptedBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                    catch
                    {
                        return "decryption_failed";
                    }
                }
            }
        }

        public static byte[] DeriveKey(string password, string salt)
        {
            var pdb = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 10000);
            return pdb.GetBytes(32); // AES-256 key size
        }
    }
}
