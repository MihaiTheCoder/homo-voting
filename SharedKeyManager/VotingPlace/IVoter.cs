using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKeyManager.VotingPlace
{
    public interface IVoter
    {
        string ID { get; set; }

        string DisplayName { get; set; }
    }
}
