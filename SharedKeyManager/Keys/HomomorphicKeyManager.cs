using Microsoft.Research.SEAL;
using SharedKeyManager.Keys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKeyManager.Keys
{
    public class HomomorphicKeyManager: IKeyManager
    {
        public SEALContext Context { get; protected set; }
        public HomomorphicKeyManager()
        {
            EncryptionParameters parms = new EncryptionParameters(SchemeType.BFV);
            parms.PolyModulusDegree = 2048;
            parms.CoeffModulus = DefaultParams.CoeffModulus128(polyModulusDegree: 2048);
            parms.PlainModulus = new SmallModulus(1 << 8);
            Context = SEALContext.Create(parms);
        }

        public Tuple<PublicKey, SecretKey> GenerateNewKeys()
        {
            KeyGenerator keygen = new KeyGenerator(Context);
            PublicKey publicKey = keygen.PublicKey;
            SecretKey secretKey = keygen.SecretKey;
            return Tuple.Create(publicKey, secretKey);
        }

        public byte[] Serialize(PublicKey publicKey)
        {
            MemoryStream memoryStream = new MemoryStream();
            publicKey.Save(memoryStream);
            return memoryStream.ToArray();
        }

        public byte[] Serialize(SecretKey secretKey)
        {
            MemoryStream memoryStream = new MemoryStream();
            secretKey.Save(memoryStream);
            return memoryStream.ToArray();
        }

        public PublicKey DeserializePublicKey(byte[] publicKeyData)
        {
            var memoryStream = new MemoryStream(publicKeyData);
            var publicKey = new PublicKey();
            publicKey.Load(Context, memoryStream);
            return publicKey;
        }

        public SecretKey DeserializeSecretKey(byte[] privateKeyData)
        {
            var memoryStream = new MemoryStream(privateKeyData);
            var secretKey = new SecretKey();
            secretKey.Load(Context, memoryStream);
            return secretKey;
        }

        public SerializedKeyPair GenerateNewSerializedKeyPair()
        {
            var keys = GenerateNewKeys();
            return new SerializedKeyPair { PublicKey = Serialize(keys.Item1), Secretkey = Serialize(keys.Item2) };
        }

        public Evaluator GetEvaluator()
        {
            return new Evaluator(Context);
        }

        public Decryptor GetDecryptor(byte[] secretKeyData)
        {
            return new Decryptor(Context, DeserializeSecretKey(secretKeyData));
        }

        public Encryptor GetEncryptor(PublicKey publicKey)
        {
            return new Encryptor(Context, publicKey);
        }

        public IntegerEncoder GetEncoder()
        {
            return new IntegerEncoder(Context);
        }

        public Ciphertext Encrypt(PublicKey publicKey, UInt32 val)
        {
            var encoder = GetEncoder();
            Ciphertext ciphertext = new Ciphertext();
            var encryptor = GetEncryptor(publicKey);
            encryptor.Encrypt(encoder.Encode(val), ciphertext);
            encryptor.Dispose();
            return ciphertext;

        }

        public Ciphertext DeserializeCiphertext(byte[] data)
        {
            var memoryStream = new MemoryStream(data);
            var cipher = new Ciphertext();
            cipher.Load(Context, memoryStream);
            return cipher;
        }

        public byte[] SerializeCiphertext(Ciphertext ciphertext)
        {
            MemoryStream memoryStream = new MemoryStream();
            ciphertext.Save(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
