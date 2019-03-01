using System.Collections.Generic;

namespace SharedKeyManager.VotingPlace
{
    public class BoothInit
    {
        public int ElectionId { get; set; }

        public int VotingPlaceId { get; set; }

        public List<Candidate> Candidates { get; set; }

        public byte[] HomomorphicPublicKey { get; set; }
    }
}
