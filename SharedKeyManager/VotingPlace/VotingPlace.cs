using SharedKeyManager.Db;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedKeyManager.VotingPlace
{
    public class VotingPlace
    {
        private readonly ICentralDbRepository centralDbRepository;
        private readonly ILocalBoothFinder localBoothFinder;
        private readonly int electionId;
        private readonly int votingPlaceId;

        public VotingPlace(ICentralDbRepository centralDbRepository, 
            ILocalBoothFinder localBoothFinder, int electionId, int votingPlaceId)
        {
            this.centralDbRepository = centralDbRepository;
            this.localBoothFinder = localBoothFinder;
            this.electionId = electionId;
            this.votingPlaceId = votingPlaceId;
        }

        public async Task InitBooths()
        {
            List<VotingBooth> votingBooths = localBoothFinder.GetVotingBooths();
            var publicKey = await centralDbRepository.GetPublicKey(electionId);
            foreach (var votingBooth in votingBooths)
            {
                var boothCertificate = votingBooth.Init(new BoothInit
                {
                    Candidates = centralDbRepository.GetCandidates(),
                    ElectionId = electionId,
                    VotingPlaceId = votingPlaceId,
                    HomomorphicPublicKey = publicKey
                });

                await centralDbRepository.SaveBoothAsync(electionId, votingPlaceId, boothCertificate);
            }
        }

        public async Task SaveVotesPublicly()
        {
            List<VotingBooth> votingBooths = localBoothFinder.GetVotingBooths();            

            foreach (var booth in votingBooths)
            {
                var votes = booth.GetVotes();
                await centralDbRepository.SavePublicVotesAsync(votes.Select(v => (PublicVote)v).ToList());
                await centralDbRepository.SaveVotersAsync(votes.Select(v => (IVoter)v).ToList());
                booth.RemoveVotes(votes);
            }
        }        
    }
}
