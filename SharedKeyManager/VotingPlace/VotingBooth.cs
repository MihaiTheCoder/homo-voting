using SharedKeyManager.Keys;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Research.SEAL;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SharedKeyManager.VotingPlace
{
    public class VotingBooth
    {
        public VotingBooth(SignCertificate signCertificate, HomomorphicKeyManager homomorphicKeyManager)
        {
            this.signCertificate = signCertificate;
            this.homomorphicKeyManager = homomorphicKeyManager;
        }

        bool isInitialized = false;
        List<Vote> votes = new List<Vote>();
        BoothInit boothInit = null;

        public uint[] NullVote { get; private set; }

        private readonly SignCertificate signCertificate;
        private readonly HomomorphicKeyManager homomorphicKeyManager;

        public byte[] AddVote(List<uint> rawVoteData, IVoter voter)
        {
            if (rawVoteData.Any(v => !(v == 0 || v == 1)))
                throw new ArgumentException("Allowed values are 0 and 1");

            if (rawVoteData.Sum(v => v) > 1)
                rawVoteData = NullVote.ToList();

            var publicKey = homomorphicKeyManager.DeserializePublicKey(boothInit.HomomorphicPublicKey);

            List<byte[]> encryptedTexts = new List<byte[]>();

            foreach (var candidate_vote in rawVoteData)
            {
                var encryptedVote = homomorphicKeyManager.Encrypt(publicKey, candidate_vote);
                encryptedTexts.Add(homomorphicKeyManager.SerializeCiphertext(encryptedVote));
            }
            var serializedVote = SerializeList(encryptedTexts);

            Vote vote = new Vote
            {
                BoothPublicCertificate = signCertificate.GetPublicKey(),
                CandidateVotes = encryptedTexts,
                ID = voter.ID,
                DisplayName = voter.DisplayName
            };
            vote.VoteSignature = signCertificate.Sign(vote.VoteHash);
            
            votes.Add(vote);

            return vote.VoteHash;
        }

        public byte[] SerializeList(List<byte[]> list)
        {
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, list);

            //This gives you the byte array.
            return mStream.ToArray();
        }

        public List<Vote> GetVotes()
        {
            if(boothInit == null)
                throw new InvalidOperationException("You need to invoke Init before invoking any other method");

            return votes;
        }

        internal X509Certificate2 Init(BoothInit boothInit)
        {
            this.boothInit = boothInit;
            this.NullVote = new uint[boothInit.Candidates.Count];
            return signCertificate.GetPublicKey();
        }

        public void RemoveVotes(List<Vote> votesToRemove)
        {
            if (boothInit == null)
                throw new InvalidOperationException("You need to invoke Init before invoking any other method");

            votes.RemoveAll(v => votesToRemove.Contains(v));
        }



    }
}