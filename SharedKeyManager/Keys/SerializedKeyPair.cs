using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKeyManager.Keys
{
    public class SerializedKeyPair
    {
        public byte[] PublicKey { get; set; }

        public byte[] Secretkey { get; set; }
    }
}
