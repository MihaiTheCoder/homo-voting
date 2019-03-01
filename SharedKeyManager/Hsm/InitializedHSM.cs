using System;
using System.Text;
using System.Threading.Tasks;

namespace SharedKeyManager
{
    public interface InitializedHSM : IDisposable
    {
        byte[] GenerateRandomRsaKeyPair(int bitSize);
        int GenerateMasterKey();
        byte[] Encrypt(int handle, byte[] data);
        byte[] Decrypt(int handle, byte[] encryptedData);
    }
}
