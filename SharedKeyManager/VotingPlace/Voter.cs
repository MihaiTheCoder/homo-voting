using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKeyManager.VotingPlace
{
    public class Voter : IVoter
    {
        public string ID { get; set; }
        public string DisplayName { get; set; }
    }
}
