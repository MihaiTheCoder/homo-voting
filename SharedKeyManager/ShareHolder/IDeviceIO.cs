using System.Threading.Tasks;

namespace SharedKeyManager.ShareHolder
{
    public interface IDeviceIO
    {
        byte[] GetSalt();
        void SaveSalt(byte[] value);
        void SaveEncryptedShares(byte[] encryptedShares);
        byte[] ReadEncryptedShares();
        Task WaitForDevice();
    }
}