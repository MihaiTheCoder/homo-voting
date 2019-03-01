namespace SharedKeyManager.VotingPlace
{

    public  class Vote: PublicVote, IVoter
    {
        public string ID { get; set; }

        public string DisplayName { get; set; }
    }
}