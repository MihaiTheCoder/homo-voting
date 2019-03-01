using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using SharedKeyManager.VotingPlace;

namespace SharedKeyManager.Db
{
    public class FakeCentralDbRepository : ICentralDbRepository
    {
        public FakeCentralDbRepository(int numberOfCandidates)
        {
            this.numberOfCandidates = numberOfCandidates;
        }
        Dictionary<int, X509Certificate2> votingPlaceToPublicCerts;
        Dictionary<int, int> ElectionToMasterKey = new Dictionary<int, int>();
        Dictionary<int, byte[]> ElectionToPublicKey = new Dictionary<int, byte[]>();
        private readonly int numberOfCandidates;
        private List<PublicVote> publicVotes = new List<PublicVote>();
        List<IVoter> voters = new List<IVoter>();

        public int GetCurrentElectionId()
        {
            return 1;
        }

        public List<Candidate> GetCandidates()
        {
            List<Candidate> candidates = new List<Candidate>();
            for (int i = 0; i < numberOfCandidates; i++)
            {
                candidates.Add(new Candidate());
            }
            return candidates;
        }

        public int GetMasterKeyHandle(int electionId)
        {
            return ElectionToMasterKey[electionId];
        }

        public Task<byte[]> GetPublicKey(int electionId)
        {
            return Task.FromResult(ElectionToPublicKey[electionId]);
        }

        public Task SaveBoothAsync(int electionId, int votingPlaceId, X509Certificate2 publicCertificate)
        {
            votingPlaceToPublicCerts.Add(votingPlaceId, publicCertificate);
            return Task.CompletedTask;
        }

        public Task SaveCryptoDetailsAsync(int electionId, int masterKeyHandle, byte[] publicKey)
        {
            ElectionToMasterKey.Add(electionId, masterKeyHandle);
            ElectionToPublicKey.Add(electionId, publicKey);
            return Task.CompletedTask;
        }

        public Task SavePublicVotesAsync(List<PublicVote> votes)
        {
            publicVotes.AddRange(votes);
            return Task.CompletedTask;
        }

        public Task<List<PublicVote>> GetPublicVotesAsync()
        {
            return Task.FromResult(publicVotes);
        }

        public Task SaveVotersAsync(List<IVoter> voters)
        {
            voters.AddRange(voters);
            return Task.CompletedTask;
        }
    }
}
