using SharedKeyManager.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SharedKeyManager.ShareHolder
{
    public class PasswordProtectedShareHolder: IPublicShareHolder, IAuthenticatedShareHolder
    {
        private readonly IDeviceIO deviceIO;
        private readonly SecureString secret;        

        public PasswordProtectedShareHolder(IDeviceIO deviceIO, SecureString secret, int saltLength, int id, uint numberOfShares)
        {
            this.deviceIO = deviceIO;
            this.secret = secret;
            ID = id;
            NumberOfShares = numberOfShares;
        }

        public int ID { get; protected set; }
        public uint NumberOfShares { get; protected set; }
               

        private byte[] _salt;

        public byte[] Salt
        {
            get
            {
                if (_salt != null)
                    return _salt;
                else
                    return deviceIO.GetSalt();
            }
            set { _salt = value; deviceIO.SaveSalt(value); }
        }


        public async Task SaveShares(byte[] shares)
        {
            await deviceIO.WaitForDevice();
            Salt = GenerateRandomSalt(7);
            var aesKey = GetAesKey();
            var encryptedShares = aesKey.Encrypt(shares);
            
            deviceIO.SaveEncryptedShares(encryptedShares);            
        }

        private AesKey GetAesKey()
        {
            var pdb = new PasswordDeriveBytes(Encoding.ASCII.GetBytes(secret.ToString()), Salt);
            AesManaged aes = new AesManaged();
            aes.Key = pdb.GetBytes(aes.KeySize / 8);
            aes.IV = pdb.GetBytes(aes.BlockSize / 8);
            return new AesKey(aes);
        }

        private byte[] GenerateRandomSalt(int numberOfBytes)
        {
            return PasswordGenerator.GeneratePasswordOfByteSize(numberOfBytes);
        }

        public async Task<byte[]> GetShares()
        {
            await deviceIO.WaitForDevice();

            var aesKey = GetAesKey();
            byte[] encryptedShares = deviceIO.ReadEncryptedShares();
            return aesKey.Decrypt(encryptedShares);
        }
    }
}
