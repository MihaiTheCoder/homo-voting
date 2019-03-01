using System.Threading.Tasks;

namespace SharedKeyManager.ShareHolder
{
    public interface IAuthenticatedShareHolder : IPublicShareHolder
    {
        Task<byte[]> GetShares();
    }
}
