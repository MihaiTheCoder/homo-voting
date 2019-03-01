using System.Threading.Tasks;

namespace SharedKeyManager.ShareHolder
{
    public interface IPublicShareHolder
    {
        int ID { get; }

        uint NumberOfShares { get; }

        Task SaveShares(byte[] shares);
    }
}
