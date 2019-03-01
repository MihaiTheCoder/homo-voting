using Newtonsoft.Json;
using SharedKeyManager.Keys;
using System.IO;
using System.Security.Cryptography;

namespace SharedKeyManager
{
    public class AesKey
    {
        AesManaged aesManaged;
        public AesKey()
        {
            aesManaged = null;
        }

        public AesKey(AesManaged initializedAes)
        {
            aesManaged = initializedAes;
            IV = KeySerializer.ByteArrayToString(initializedAes.IV);
            Key = KeySerializer.ByteArrayToString(initializedAes.Key);
        }        

        public string IV { get; set; }

        public string Key { get; set; }

        public AesManaged GetAesManaged()
        {
            if (aesManaged == null)
            {
                aesManaged = new AesManaged();
                aesManaged.IV = KeySerializer.StringToByteArray(IV);
                aesManaged.Key = KeySerializer.StringToByteArray(Key);
            }
            return aesManaged;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public byte[] Encrypt(byte[] data)
        {
            var encryptor = GetAesManaged().CreateEncryptor();
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.Close();
            return ms.ToArray();
        }

        public byte[] Decrypt(byte[] encryptedData)
        {
            var decryptor = GetAesManaged().CreateDecryptor();
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
            cs.Write(encryptedData, 0, encryptedData.Length);
            cs.Close();
            return ms.ToArray();
        }

        public static explicit operator AesKey(string serializedAes)
        {
            return JsonConvert.DeserializeObject<AesKey>(serializedAes);
        }        
    }
}
