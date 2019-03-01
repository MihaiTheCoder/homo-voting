using System.Collections.Generic;

namespace SharedKeyManager.VotingPlace
{
    public interface ILocalBoothFinder
    {
        List<VotingBooth> GetVotingBooths();
    }
}