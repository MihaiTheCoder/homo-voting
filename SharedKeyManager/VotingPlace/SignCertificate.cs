using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SharedKeyManager.VotingPlace
{
    public class SignCertificate
    {
        private RSACryptoServiceProvider privateKey;
        private RSACryptoServiceProvider publicKey;
        private string algorithm;
        private X509Certificate2 certificate;

        public SignCertificate(StoreName storeName, StoreLocation storeLocation, string certSubject)
        {
            StoreName = storeName;
            StoreLocation = storeLocation;
            CertSubject = certSubject;
            certificate = GetCertificate();
            
            if (certificate == null)
                throw new ArgumentException($"Could not find certificate with storeName: {storeName}, storeLocation:{storeLocation}, subject:{certSubject}");

            privateKey = (RSACryptoServiceProvider)certificate.PrivateKey;
            if (privateKey == null)
                throw new ArgumentException($"Could not find private key for the certificate:{certificate}");

            algorithm = CryptoConfig.MapNameToOID("SHA1");
            

            publicKey = (RSACryptoServiceProvider)certificate.PublicKey.Key;            
        }

        private StoreName StoreName { get; }
        private StoreLocation StoreLocation { get; }
        private string CertSubject { get; }

        public byte[] Sign(byte[] hash)
        {
            // Sign the hash
            return privateKey.SignHash(hash, algorithm);
        }

        public bool Verify(byte[] hash, byte[] signature)
        {
            // Verify the signature with the hash
            return publicKey.VerifyHash(hash, algorithm, signature);
        }

        private X509Certificate2 GetCertificate()
        {
            X509Store my = new X509Store(StoreName, StoreLocation);
            my.Open(OpenFlags.ReadOnly);

            // Find the certificate we'll use to sign
            foreach (X509Certificate2 cert in my.Certificates)
            {
                return cert;
            }
            return null;
        }


        public X509Certificate2 GetPublicKey()
        {
            return new X509Certificate2(certificate.Export(X509ContentType.Cert));
        }
    }
}
