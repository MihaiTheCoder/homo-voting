using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SharedKeyManager.VotingPlace
{
    public class PublicVote
    {
        /// <summary>
        /// For each person have 1 or 0 encrypted
        /// </summary>
        public List<byte[]> CandidateVotes { get; set; }

        public byte[] SerializedVotes { get { return SerializeList(CandidateVotes); } }

        public byte[] VoteSignature { get; set; }

        public X509Certificate2 BoothPublicCertificate { get; set; }


        public byte[] SerializeList(List<byte[]> list)
        {
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, list);

            //This gives you the byte array.
            return mStream.ToArray();
        }

        public byte[] HashData(byte[] data)
        {
            SHA1Managed sha1 = new SHA1Managed();
            byte[] hash = sha1.ComputeHash(data);
            return hash;
        }

        public byte[] VoteHash { get { return HashData(SerializedVotes); } }
    }
}