using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKeyManager.VotingPlace
{
    public class FakeLocalBoothFinder : ILocalBoothFinder
    {
        private readonly List<VotingBooth> votingBooths;

        public FakeLocalBoothFinder(List<VotingBooth> votingBooths)
        {
            this.votingBooths = votingBooths;
        }
        public List<VotingBooth> GetVotingBooths()
        {
            return votingBooths;
        }
    }
}
