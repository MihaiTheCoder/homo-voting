using Microsoft.Research.SEAL;
using SharedKeyManager;
using SharedKeyManager.Db;
using SharedKeyManager.Hsm;
using SharedKeyManager.Keys;
using SharedKeyManager.ShareHolder;
using SharedKeyManager.VoteAnalysis;
using SharedKeyManager.VotingPlace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Linq;

namespace CoreHomoVoting
{
    class Program
    {
        static void Main(string[] args)
        {
            //HomoExample();
            /*
             Create a list of shareholders. 
             We need to set the number of shares so that if enough shareholders want to decrypt the secret they can.
             But if the treshold is not met, they should not be able to decrypt the information
             */
            List<PasswordProtectedShareHolder> shareHolders = new List<PasswordProtectedShareHolder>
            {
                CreateShareholder("xxx", id: 1, numberOfShares: 2),
                CreateShareholder("yyy", 2, numberOfShares: 2),
                CreateShareholder("zzz", 3, numberOfShares: 4)
            };            

            /*
             HSM: Hardware security module - A machine that should zeroize (delete all keys/data) in case of phisical temper.
             HSM never exposes the keys that it stores. The user of the HSM can do cryptographic operations with the keys, and create new keys.             
             */
            FakeHsmFactory fakeHsmFactory = new FakeHsmFactory(Path.GetTempFileName());

            /*
             Creates the homomorphic encryption context (encryption details), and helper methods for homomorphic cryptography operations.
             */
            HomomorphicKeyManager keyManager = new HomomorphicKeyManager();
            
            /*
             A fake implementation of a database repository
             */
            FakeCentralDbRepository fakeCentralDbRepository = new FakeCentralDbRepository(numberOfCandidates: 3);


            /*
             Treshold, is the minimum number of shares needed to decrypt the splited secret.
             ShareManager not tied to homomorphic encryption, it can be used with any type of encryption.
             */
            ShareManager sm = new ShareManager(treshold: 4,hsmFactory: fakeHsmFactory, keyManager: keyManager, 
                centralDbRepository: fakeCentralDbRepository, publicShareHolders: shareHolders.Select(sh=> (IPublicShareHolder)sh).ToList());

            /*
             * This will save the splited secret using the shareholder method to save the secret
             * Also this will store in the database the public key that will be used to encrypt the votes.
             * Along with the public key, the master key handle from the HSM will be saved as well in the database.
             */
            sm.GenerateSplitedSecret(hsmSecret: null, electionId: 1).Wait();

            /*
             A key chain needs to be created so that every voting booth has a certificate signed by a certificate authority(Voting place-2019)
             Voting place CA would be signed by a voting city CA, and up to voting country+electionID.
             It is important for the vooting booth to sign all the votes that it prints so that the voter can later verify that he's vote has been taken into consideration.
             If the voter wouldn't see his vote in the public database, he could proove to the authorities that his vote was registered by a booth.
             Online he could see the hour of the vote, the vote hash, vote encrypted data, voting booth public certificate that can be used to validate the vote.
             */
            var booth = new VotingBooth(new SignCertificate(StoreName.My, StoreLocation.CurrentUser, "ceapa"), keyManager);
            List<VotingBooth> votingBooths = new List<VotingBooth>
            {
                booth
            };
            /*
             *  A voting place is a room with voting booth's. It has the responsibility of periodically geting the data from the booth and to publish it on the public database.
             *  The voters are saved in a separate database unlinked from the actual votes. You want them unlinked so that in future if the cipher is cracked, 
             *  no one will know what was my vote unless I showed it to somebody else.
             *  FakeLocalBoothFinder is just gives for now instances of the voting booths. 
             *  But in a real scenario, this would just be a wrapper over the booth that would invoke the Voting Booth Server.
             *  Only VotingPlace can access the Voting Booth Server (Using certificate authentication maybe)             *  
             */
            VotingPlace votingPlace = new VotingPlace(fakeCentralDbRepository, new FakeLocalBoothFinder(votingBooths), 1, 1);
            booth.AddVote(new List<uint> { 0, 1, 0 }, new Voter { ID = "x", DisplayName = "X-ulescu" });
            booth.AddVote(new List<uint> { 0, 1, 0 }, new Voter { ID = "y", DisplayName = "Y-ulescu" });
            
            /*
             * Save the votes in the public database.
             * Save the voters in the public database(it may be that this is private, only to have a hash of the person?)
             */
            votingPlace.SaveVotesPublicly().Wait();

            /*
             * Vote aggregator. Code that adds the votes together.
             * Note that the aggregator only takes the context as the parameter (Not even the public key)
             * So anyone can actually do math operations on homomorphic encryption if they know the parameters.
             */
            VoteAggregator aggregator = new VoteAggregator(keyManager);
            var aggregatedResult = aggregator.AddVotes(fakeCentralDbRepository.GetPublicVotesAsync().Result);

            /*
             * Combine the secret together again using the shareholder with ID 3(he has 4 shares, and that is the treshold for decryption)
             */
            byte[] res = sm.GetCombinedSecret(shareHolders.Where(s => s.ID == 3).Select(s => (IAuthenticatedShareHolder)s).ToList(), null, 1).Result;

            /*
             * You can only read the votes using the combined secret and the context.
             * The VoteReader should only decrypt aggregates. They should agree only on a few decryptions. 
             * Maybe a few tests on smaller aggregates, assuming that they already knwo the actual answer for those small agregates.
             */
            VoteReader voteReader = new VoteReader(homomorphicKeyManager: keyManager, secretKeyData: res);

            var decryptedVotes = voteReader.DecryptVotes(aggregatedResult);
        }

        private static PasswordProtectedShareHolder CreateShareholder(string str, int id, uint numberOfShares)
        {
            var securePass = new NetworkCredential("", str).SecurePassword;
            string result = Path.GetTempPath() + Path.GetRandomFileName();
            var dummyDevice = new DummyFolderDeviceIO(result);
            return new PasswordProtectedShareHolder(dummyDevice, securePass, 7, id, numberOfShares);
        } 

        private static void HomoExample()
        {
            EncryptionParameters parms = new EncryptionParameters(SchemeType.BFV);
            parms.PolyModulusDegree = 2048;
            parms.CoeffModulus = DefaultParams.CoeffModulus128(polyModulusDegree: 2048);
            parms.PlainModulus = new SmallModulus(1 << 8);
            SEALContext context = SEALContext.Create(parms);
            IntegerEncoder encoder = new IntegerEncoder(context);

            KeyGenerator keygen = new KeyGenerator(context);
            Microsoft.Research.SEAL.PublicKey publicKey = keygen.PublicKey;
            SecretKey secretKey = keygen.SecretKey;

            Encryptor encryptor = new Encryptor(context, publicKey);
            Evaluator evaluator = new Evaluator(context);

            Decryptor decryptor = new Decryptor(context, secretKey);

            int value1 = 5;
            Plaintext plain1 = encoder.Encode(value1);
            Console.WriteLine($"Encoded {value1} as polynomial {plain1.ToString()} (plain1)");

            int value2 = -7;
            Plaintext plain2 = encoder.Encode(value2);
            Console.WriteLine($"Encoded {value2} as polynomial {plain2.ToString()} (plain2)");

            Ciphertext encrypted1 = new Ciphertext();
            Ciphertext encrypted2 = new Ciphertext();
            Console.Write("Encrypting plain1: ");

            encryptor.Encrypt(plain1, encrypted1);
            Console.WriteLine("Done (encrypted1)");

            Plaintext plainResult = new Plaintext();
            decryptor.Decrypt(encrypted1, plainResult);
            Console.WriteLine(encoder.DecodeInt32(plainResult));


            Console.Write("Encrypting plain2: ");
            encryptor.Encrypt(plain2, encrypted2);
            Console.WriteLine("Done (encrypted2)");



            Console.WriteLine($"Noise budget in encrypted1: {decryptor.InvariantNoiseBudget(encrypted1)} bits");
            Console.WriteLine($"Noise budget in encrypted2: {decryptor.InvariantNoiseBudget(encrypted2)} bits");

            evaluator.NegateInplace(encrypted1);
            Console.WriteLine($"Noise budget in -encrypted1: {decryptor.InvariantNoiseBudget(encrypted1)} bits");

            evaluator.AddInplace(encrypted1, encrypted2);

            Console.WriteLine($"Noise budget in -encrypted1 + encrypted2: {decryptor.InvariantNoiseBudget(encrypted1)} bits");

            evaluator.MultiplyInplace(encrypted1, encrypted2);

            Console.WriteLine($"Noise budget in (-encrypted1 + encrypted2) * encrypted2: {decryptor.InvariantNoiseBudget(encrypted1)} bits");

            plainResult = new Plaintext();
            Console.Write("Decrypting result: ");
            decryptor.Decrypt(encrypted1, plainResult);
            Console.WriteLine("Done");

            Console.WriteLine($"Plaintext polynomial: {plainResult.ToString()}");

            Console.WriteLine($"Decoded integer: {encoder.DecodeInt32(plainResult)}");
        }
    }
}
