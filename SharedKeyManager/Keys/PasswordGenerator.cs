using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SharedKeyManager.Keys
{
    public class PasswordGenerator
    {
        public static byte[] GeneratePasswordOfBitSize(int bitSize)
        {
            return GeneratePasswordOfByteSize(bitSize / 8);
        }

        public static byte[] GeneratePasswordOfByteSize(int byteSize)
        {
            var totalBytes = new byte[byteSize];
            GeneratePassword(totalBytes);
            return totalBytes;
        }

        public static void GeneratePassword(byte[] data)
        {
            var rand = RandomNumberGenerator.Create();            
            rand.GetBytes(data);
        }
    }
}
