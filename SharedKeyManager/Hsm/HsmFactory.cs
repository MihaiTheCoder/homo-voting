using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SharedKeyManager
{
    public abstract class HsmFactory
    {
        public abstract Task<InitializedHSM> GetHsm(SecureString secret);
    }
}
