using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SharedKeyManager.Hsm
{
    public class FakeHsmFactory : HsmFactory
    {
        public FakeHsmFactory(string filePathToKeys)
        {
            FilePathToKeys = filePathToKeys;
        }

        public string FilePathToKeys { get; }

        public override Task<InitializedHSM> GetHsm(SecureString secret)
        {
            return Task.FromResult((InitializedHSM)new FakeInitializedHSM(FilePathToKeys));
        }
    }
}
