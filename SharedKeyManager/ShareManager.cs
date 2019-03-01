using Moserware.Security.Cryptography;
using Newtonsoft.Json;
using SharedKeyManager.Db;
using SharedKeyManager.Keys;
using SharedKeyManager.ShareHolder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SharedKeyManager
{
    public class ShareManager
    {
        private readonly HsmFactory hsmFactory;
        private readonly IKeyManager keyManager;
        private readonly ICentralDbRepository centralDbRepository;

        public ShareManager(int treshold, HsmFactory hsmFactory, IKeyManager keyManager, ICentralDbRepository centralDbRepository,
            List<IPublicShareHolder> publicShareHolders)
        {
            ShareHolders = publicShareHolders;
            Treshold = treshold;
            this.hsmFactory = hsmFactory;
            this.keyManager = keyManager;
            this.centralDbRepository = centralDbRepository;
        }

        public List<IPublicShareHolder> ShareHolders { get; protected set; }

        public int Treshold { get; set; }
        public string PathToSharesDir { get; }

        public async Task GenerateSplitedSecret(SecureString hsmSecret, int electionId)
        {
            using (var initializedHsm = await hsmFactory.GetHsm(hsmSecret))
            {                

                var keys = keyManager.GenerateNewSerializedKeyPair();

                var handle = initializedHsm.GenerateMasterKey();
                var encryptedSecretKey = initializedHsm.Encrypt(handle, keys.Secretkey);

                var totalNumberOfShares = (int)ShareHolders.Sum(s => s.NumberOfShares);
                var splitedSecret = SecretSplitter.SplitMessage(KeySerializer.ByteArrayToString(encryptedSecretKey), Treshold, totalNumberOfShares);
                int index = 0;

                foreach (var shareHolder in ShareHolders)
                {
                    var shares = splitedSecret.Skip(index).Take((int)shareHolder.NumberOfShares);
                    index += (int)shareHolder.NumberOfShares;
                    var bytes = SerializeShares(shares.ToArray());
                    await shareHolder.SaveShares(bytes);
                }

                await centralDbRepository.SaveCryptoDetailsAsync(electionId, handle, keys.PublicKey);                
            }
        }

        private byte[] SerializeShares(string[] shares)
        {
            var json = JsonConvert.SerializeObject(shares);
            byte[] bytes = Encoding.ASCII.GetBytes(json);
            return bytes;
        }

        private string[] DeserializeShares(byte[] shares)
        {
            var json = Encoding.ASCII.GetString(shares);
            return JsonConvert.DeserializeObject<string[]>(json);
        }

        public async Task<byte[]> GetCombinedSecret(List<IAuthenticatedShareHolder> authenticatedShareHolders, SecureString hsmSecret, int electionId)
        {
            using (var initializedHsm = await hsmFactory.GetHsm(hsmSecret))
            {
                var availableShares = new List<string>();
                foreach (var shareHolder in authenticatedShareHolders)
                {
                    var shares = DeserializeShares(await shareHolder.GetShares());
                    availableShares.AddRange(shares);
                }

                var combinedSecret = SecretCombiner.Combine(availableShares);
                var byteArrayKey = KeySerializer.StringToByteArray(combinedSecret.RecoveredTextString);
                var decryptedSecretKey = initializedHsm.Decrypt(centralDbRepository.GetMasterKeyHandle(electionId), byteArrayKey);
                return decryptedSecretKey;
            }
        }
    }
}
