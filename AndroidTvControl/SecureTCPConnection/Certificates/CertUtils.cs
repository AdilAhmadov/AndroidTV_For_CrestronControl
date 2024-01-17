
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crestron.SimplSharp.Cryptography.X509Certificates;
using Crestron.SimplSharp.Cryptography;

using AndroidTvControl.Hellpers;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Prng;


namespace AndroidTvControl.SecureTCPConnection.Certificates
{
    public class CertUtils
    {
        private static byte[] GetAlphaValue(string code, X509Certificate2 clientCertificate, X509Certificate2 serverCertificate)
        {
            // nonce are the last 4 characters of the code displayed on the TV
            byte[] nonce = FromHexString(code.Substring(2)).ToArray();

    
            // retrieve RSA key pair from key container
            var client = new RSAParameters();
            client.Equals(clientCertificate.GetPublicKey());

            var server = new RSAParameters();
            server.Equals(serverCertificate.GetPublicKey());


            var instance = new Sha256Digest();


            byte[] clientModulus = RemoveLeadingZeroBytes(client.Modulus);
            byte[] clientExponent = RemoveLeadingZeroBytes(client.Exponent);
            byte[] serverModulus = RemoveLeadingZeroBytes(server.Modulus);
            byte[] serverExponent = RemoveLeadingZeroBytes(server.Exponent);

            Debug.PrintLine("Hash inputs: ");
            Debug.PrintLine("client modulus: " + BitConverter.ToString(clientModulus));
            Debug.PrintLine("client exponent: " + BitConverter.ToString(clientExponent));
            Debug.PrintLine("server modulus: " + BitConverter.ToString(serverModulus));
            Debug.PrintLine("server exponent: " + BitConverter.ToString(serverExponent));
            Debug.PrintLine("nonce: " + BitConverter.ToString(nonce));

            instance.BlockUpdate(clientModulus, 0, clientModulus.Length);
            instance.BlockUpdate(clientExponent, 0, clientExponent.Length);
            instance.BlockUpdate(serverModulus, 0, serverModulus.Length);
            instance.BlockUpdate(serverExponent, 0, serverExponent.Length);
            instance.BlockUpdate(nonce, 0, nonce.Length);



            byte[] hash = new byte[instance.GetDigestSize()];
            instance.DoFinal(hash, 0);

            Debug.PrintLine("hash: " + BitConverter.ToString(hash));

            return hash;
        }
        //public static string GenerateCertificate2(string name, string country, string state, string locality, string organisation, string organisationUnit, string email)
        //{
        //    bool exClientAuth = true;
        //    int keyStrength = 2048;
        //    string signatureAlgorithm = "SHA256WITHRSA";

        //    var randomGenerator = new RNGCryptoServiceProvider();
          

        //    //CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();
        //    //SecureRandom random = new SecureRandom(randomGenerator);

        //    KeyGenerationParameters keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
        //    RsaKeyPairGenerator keyPairGenerator = new RsaKeyPairGenerator();
        //    keyPairGenerator.Init(keyGenerationParameters);

        //    AsymmetricCipherKeyPair CertKeyPair = keyPairGenerator.GenerateKeyPair();
        //    ISignatureFactory signatureFactory = new Asn1SignatureFactory(signatureAlgorithm, CertKeyPair.Private, random);

        //    X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();

        //    certificateGenerator.SetNotBefore(DateTime.UtcNow.Date);
        //    certificateGenerator.SetNotAfter(DateTime.UtcNow.Date.AddYears(10));

        //    List<DerObjectIdentifier> nameOids = new List<DerObjectIdentifier> { X509Name.CN, X509Name.O, X509Name.OU, X509Name.ST, X509Name.C, X509Name.L, X509Name.E };

        //    Dictionary<DerObjectIdentifier, string> nameValues = new Dictionary<DerObjectIdentifier, string>()
        //    {
        //        { X509Name.CN, name },
        //        { X509Name.O, organisation },
        //        { X509Name.OU, organisationUnit },
        //        { X509Name.ST, state },
        //        { X509Name.C, country },
        //        { X509Name.L, locality },
        //        { X509Name.E, email },
        //    };

        //    X509Name CertDN = new X509Name(nameOids, nameValues);

        //    certificateGenerator.SetIssuerDN(CertDN);
        //    certificateGenerator.SetSubjectDN(CertDN);
        //    certificateGenerator.SetPublicKey(CertKeyPair.Public);

        //    if (exClientAuth)
        //    {
        //        KeyUsage keyUsage = new KeyUsage(KeyUsage.KeyEncipherment);
        //        certificateGenerator.AddExtension(X509Extensions.KeyUsage, false, keyUsage.ToAsn1Object());

        //        // add client authentication extended key usage
        //        var extendedKeyUsage = new ExtendedKeyUsage(new[] { KeyPurposeID.id_kp_clientAuth });
        //        certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, true, extendedKeyUsage.ToAsn1Object());
        //    }

        //    // serial number is required, generate it randomly
        //    byte[] serial = new byte[20];
        //    random.NextBytes(serial);
        //    serial[0] = 1;
        //    certificateGenerator.SetSerialNumber(new Org.BouncyCastle.Math.BigInteger(serial));

        //    var certificate = certificateGenerator.Generate(signatureFactory);
        //    AsymmetricParameter privateKey = CertKeyPair.Private;
        //    Pkcs8Generator pkcs8 = new Pkcs8Generator(privateKey);

        //    using (var textWriter = new StringWriter())
        //    {
        //        using (PemWriter pemWriter = new PemWriter(textWriter))
        //        {
        //            pemWriter.WriteObject(certificate);
        //            pemWriter.WriteObject(pkcs8);
        //        }

        //        return textWriter.ToString();
        //    }
        //}
        private static byte[] RemoveLeadingZeroBytes(byte[] array)
        {
            int skip = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != 0)
                {
                    skip = i;
                    break;
                }
            }

            return array.Skip(skip).ToArray();
        }
        private static byte[] FromHexString(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
