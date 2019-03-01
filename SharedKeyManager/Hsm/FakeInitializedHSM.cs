using Newtonsoft.Json;
using SharedKeyManager.Keys;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SharedKeyManager
{
    public class FakeInitializedHSM: InitializedHSM
    {
        private readonly object myLock = new object();
        private readonly string pathToKeys;
        Dictionary<int, string> masterKeys = new Dictionary<int, string>();
        public FakeInitializedHSM(string pathToKeys)
        {
            this.pathToKeys = pathToKeys;
            LoadMasterKeys();
        }

        private void LoadMasterKeys()
        {
            if (File.Exists(pathToKeys))
            {
                var keysText = File.ReadAllText(pathToKeys);
                masterKeys = JsonConvert.DeserializeObject<Dictionary<int, string>>(keysText);
            }
        }

        public void Dispose()
        {

        }

        public int GenerateMasterKey()
        {
            AesManaged aes = new AesManaged();
            aes.GenerateKey();
            aes.GenerateIV();
            var aesKey = new AesKey(aes);
            int maxKey;
            lock (myLock)
            {
                LoadMasterKeys();
                maxKey = masterKeys.Keys.Max() +1;
                masterKeys.Add(maxKey, aesKey.ToString());
                SaveMasterKeys();
            }

            return maxKey;            
        }

        private void SaveMasterKeys()
        {
            if (File.Exists(pathToKeys))
                File.WriteAllText(pathToKeys, JsonConvert.SerializeObject(masterKeys));
        }

        public byte[] GenerateRandomRsaKeyPair(int bitSize)
        {
            return PasswordGenerator.GeneratePasswordOfBitSize(2048);
        }

        public byte[] Encrypt(int handle, byte[] data)
        {
            var aesKey = (AesKey)masterKeys[handle];
            return aesKey.Encrypt(data);
        }

        public byte[] Decrypt(int handle, byte[] encryptedData)
        {
            var aesKey = (AesKey)masterKeys[handle];
            return aesKey.Decrypt(encryptedData);
        }
    }
}
