using System;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;


namespace tibiamonoopengl.Rsa
{




    public class RsaDecryptor
    {
        private RSAParameters _rsaParameters;
        public const int BufferLength = 128;

        public RsaDecryptor(string pemFile)
        {
            LoadPrivateKey(pemFile);
        }


        public byte[] Decrypt(byte[] data, AsymmetricCipherKeyPair keyPair)
        {
            var decryptEngine = new Pkcs1Encoding(new RsaEngine());
            decryptEngine.Init(false, keyPair.Private);

            return decryptEngine.ProcessBlock(data, 0, data.Length);
        }

        private AsymmetricCipherKeyPair LoadPrivateKey(string filename)
        {
            using (var reader = File.OpenText(filename))
            {
                var pemReader = new PemReader(reader);
                var keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
                var privateKey = (RsaPrivateCrtKeyParameters)keyPair.Private;
                _rsaParameters = DotNetUtilities.ToRSAParameters(privateKey);
                return (AsymmetricCipherKeyPair)pemReader.ReadObject();
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            using (var rsa = RSA.Create())
            {
                rsa.ImportParameters(_rsaParameters);
                return rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
            }
        }
    }
}