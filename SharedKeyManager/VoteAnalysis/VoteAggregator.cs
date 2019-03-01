using Microsoft.Research.SEAL;
using SharedKeyManager.Keys;
using SharedKeyManager.VotingPlace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKeyManager.VoteAnalysis
{
    public class VoteAggregator
    {
        private readonly HomomorphicKeyManager homomorphicKeyManager;
        Evaluator evaluator;

        public VoteAggregator(HomomorphicKeyManager homomorphicKeyManager)
        {
            this.homomorphicKeyManager = homomorphicKeyManager;
            evaluator = homomorphicKeyManager.GetEvaluator();
        }

        public List<byte[]> AddVotes(List<PublicVote> publicVotes)
        {
            if (!publicVotes.Any())
                return new List<byte[]>();

            var firstCandidateVotes = publicVotes[0].CandidateVotes;


            List<Ciphertext> aggregates = new List<Ciphertext>();
            for(int i=0; i< firstCandidateVotes.Count; i++)
            {
                aggregates.Add(homomorphicKeyManager.DeserializeCiphertext(firstCandidateVotes[i]));
            }

            for (int i = 1; i < publicVotes.Count; i++)
            {
                var vote = publicVotes[i];
                for (int j = 0; j < vote.CandidateVotes.Count; j++)
                {
                    var cipher = homomorphicKeyManager.DeserializeCiphertext(vote.CandidateVotes[j]);
                    evaluator.AddInplace(aggregates[j], cipher);
                }
            }

            return aggregates.Select(a => homomorphicKeyManager.SerializeCiphertext(a)).ToList();
        }        
    }
}
