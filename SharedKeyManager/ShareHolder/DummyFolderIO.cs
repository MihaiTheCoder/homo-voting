using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKeyManager.ShareHolder
{
    public class DummyFolderDeviceIO : IDeviceIO
    {
        private readonly string path;

        public string SaltPath { get { return Path.Combine(path, "Salt"); } }

        public string EncryptedSharesPath { get { return Path.Combine(path, "EncryptedShares"); } }

        public DummyFolderDeviceIO(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            this.path = path;
        }
        
        public byte[] GetSalt()
        {
            return GetBytes(SaltPath);
        }        

        public void SaveSalt(byte[] value)
        {
            SaveBytes(SaltPath, value);
        }       

        public byte[] ReadEncryptedShares()
        {
            return GetBytes(EncryptedSharesPath);
        }

        public void SaveEncryptedShares(byte[] encryptedShares)
        {
            SaveBytes(EncryptedSharesPath, encryptedShares);
        }        

        public Task WaitForDevice()
        {
            return Task.CompletedTask;
        }

        private byte[] GetBytes(string path)
        {
            if (File.Exists(path))
                return File.ReadAllBytes(path);
            else
                return null;
        }

        private void SaveBytes(string path, byte[] value)
        {
            File.WriteAllBytes(path, value);
        }
    }
}
