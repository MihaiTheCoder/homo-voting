using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKeyManager.Keys
{
    public interface IKeyManager
    {
        SerializedKeyPair GenerateNewSerializedKeyPair();
    }    
}
