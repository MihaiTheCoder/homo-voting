using Microsoft.Research.SEAL;
using SharedKeyManager.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKeyManager.VoteAnalysis
{
    public class VoteReader
    {
        private readonly HomomorphicKeyManager homomorphicKeyManager;
        Decryptor decryptor;
        IntegerEncoder encoder;

        public VoteReader(HomomorphicKeyManager homomorphicKeyManager, byte[] secretKeyData)
        {
            this.homomorphicKeyManager = homomorphicKeyManager;
            decryptor = homomorphicKeyManager.GetDecryptor(secretKeyData);
            encoder = homomorphicKeyManager.GetEncoder();
        }

        public List<uint> DecryptVotes(List<byte[]> candidatesAggregatedVote)
        {
            List<uint> results = new List<uint>();
            foreach (var vote in candidatesAggregatedVote)
            {
                Plaintext plainResult = new Plaintext();
                var cipher = homomorphicKeyManager.DeserializeCiphertext(vote);
                decryptor.Decrypt(cipher, plainResult);
                results.Add(encoder.DecodeUInt32(plainResult));
            }
            return results;
        }
    }
}
