using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using SharedKeyManager.VotingPlace;

namespace SharedKeyManager.Db
{
    public interface ICentralDbRepository
    {
        Task SaveCryptoDetailsAsync(int electionId, int masterKeyHandle, byte[] publicKey);

        Task<byte[]> GetPublicKey(int electionId);
        int GetMasterKeyHandle(int electionId);
        Task SaveBoothAsync(int electionId, int votingPlaceId, X509Certificate2 publicCertificate);
        Task SavePublicVotesAsync(List<PublicVote> votes);
        Task SaveVotersAsync(List<IVoter> voters);
        List<Candidate> GetCandidates();
    }
}
