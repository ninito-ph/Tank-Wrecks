using System.Text;
using System.Security.Cryptography;
using System;

public static class DataCrypto
{
    // Initializes the hash with a randomly generated string
    // This hash would take on average 34 thousand years to bruteforce
    // Someone could still just look at the assembly and find this key
    private static string hash = "O8t!7Tto^you";

    // Encrypts text
    public static string EncryptText(string inputText)
    {
        // Stores the input in a byte array
        byte[] data = UTF8Encoding.UTF8.GetBytes(inputText);

        // NOTE: MD5 is *NOT* secure anymore, but its enough to keep players from editing save files directly
        using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
        {
            // Makes the key a byte array
            byte[] key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash));

            // Using Triple DES, give it a key equal to our defined key, set it to ECB cipher and use PKCS7 padding
            using (TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider() { Key = key, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
            {
                // Creates an encryptor
                ICryptoTransform transform = tripleDES.CreateEncryptor();
                // Stores the encryption results
                byte[] results = transform.TransformFinalBlock(data, 0, data.Length);
                // Converts results to a string
                return Convert.ToBase64String(results, 0, results.Length);
            }
        }
    }

    // Decrypts text
    public static string DecryptText(string inputCrypt)
    {
        // Stores the input in a byte array
        byte[] data = Convert.FromBase64String(inputCrypt);

        // NOTE: MD5 is not actually secure anymore, but its enough to keep players from editing save files directly
        using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
        {
            // Makes the key a byte array
            byte[] key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash));

            // Using Triple DES, give it a key equal to our defined key, set it to ECB cipher and use PKCS7 padding
            using (TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider() { Key = key, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
            {
                // Creates a decryptor
                ICryptoTransform transform = tripleDES.CreateDecryptor();
                // Stores the decryption results
                byte[] results = transform.TransformFinalBlock(data, 0, data.Length);
                // Converts results to a string
                return UTF8Encoding.UTF8.GetString(results);
            }
        }
    }
}
