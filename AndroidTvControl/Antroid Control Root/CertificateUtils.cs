using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;


namespace AndroidTvControl.Antroid_Control_Root
{
    internal class CertificateUtils
    {      
        public static string GenerateCertificate(string name, string country, string state, string locality, string organisation, string organisationUnit, string email)
        {
            bool exClientAuth = true;
            int keyStrength = 2048;
            string signatureAlgorithm = "SHA256WITHRSA";

            CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();
            SecureRandom random = new SecureRandom(randomGenerator);

            KeyGenerationParameters keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            RsaKeyPairGenerator keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);

            AsymmetricCipherKeyPair subjectKeyPair = keyPairGenerator.GenerateKeyPair();
            AsymmetricCipherKeyPair issuerKeyPair = subjectKeyPair;
            ISignatureFactory signatureFactory = new Asn1SignatureFactory(signatureAlgorithm, issuerKeyPair.Private, random);

            X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();

            certificateGenerator.SetNotBefore(DateTime.UtcNow.Date);
            certificateGenerator.SetNotAfter(DateTime.UtcNow.Date.AddYears(10));

            List<DerObjectIdentifier> nameOids = new List<DerObjectIdentifier> { X509Name.CN, X509Name.O, X509Name.OU, X509Name.ST, X509Name.C, X509Name.L, X509Name.E };

            Dictionary<DerObjectIdentifier, string> nameValues = new Dictionary<DerObjectIdentifier, string>()
            {
                { X509Name.CN, name },
                { X509Name.O, organisation },
                { X509Name.OU, organisationUnit },
                { X509Name.ST, state },
                { X509Name.C, country },
                { X509Name.L, locality },
                { X509Name.E, email },
            };

            X509Name subjectDN = new X509Name(nameOids, nameValues);
            X509Name issuerDN = subjectDN;

            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);
            certificateGenerator.SetPublicKey(issuerKeyPair.Public);

            if (exClientAuth)
            {
                KeyUsage keyUsage = new KeyUsage(KeyUsage.KeyEncipherment);
                certificateGenerator.AddExtension(X509Extensions.KeyUsage, false, keyUsage.ToAsn1Object());

                // add client authentication extended key usage
                var extendedKeyUsage = new ExtendedKeyUsage(new[] { KeyPurposeID.id_kp_clientAuth });
                certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, true, extendedKeyUsage.ToAsn1Object());
            }

            // serial number is required, generate it randomly
            byte[] serial = new byte[20];
            random.NextBytes(serial);
            serial[0] = 1;
            certificateGenerator.SetSerialNumber(new Org.BouncyCastle.Math.BigInteger(serial));

            var certificate = certificateGenerator.Generate(signatureFactory);
            AsymmetricKeyParameter privateKey = issuerKeyPair.Private;
            Pkcs8Generator pkcs8 = new Pkcs8Generator(privateKey);

            using (var textWriter = new StringWriter())
            {
                using (PemWriter pemWriter = new PemWriter(textWriter))
                {
                    pemWriter.WriteObject(certificate);
                    pemWriter.WriteObject(pkcs8);
                }

                return textWriter.ToString();
            }
        }
        public static X509Certificate2 LoadCertificateFromPEM(string pem)
        {
            using (var textReader = new StringReader(pem))
            {
                using (PemReader reader = new PemReader(textReader))
                {
                    Org.BouncyCastle.Utilities.IO.Pem.PemObject read;

                    Org.BouncyCastle.X509.X509Certificate certificate = null;
                    AsymmetricKeyParameter privateKey = null;

                    while ((read = reader.ReadPemObject()) != null)
                    {
                        switch (read.Type)
                        {
                            case "CERTIFICATE":
                                {
                                    certificate = new Org.BouncyCastle.X509.X509Certificate(read.Content);
                                    //certificate = new Crestron.SimplSharp.Cryptography.X509Certificates.X509Certificate(read.Content);
                                }
                                break;

                            case "PRIVATE KEY":
                                {
                                    privateKey = PrivateKeyFactory.CreateKey(read.Content);
                                }
                                break;

                            default:
                                throw new NotSupportedException(read.Type);
                        }
                    }

                    if (certificate == null || privateKey == null)
                    {
                        throw new Exception("Unable to load certificate with the private key from the PEM!");
                    }

                    return GetX509CertificateWithPrivateKey(certificate, privateKey);
                }
            }
        }
        private static X509Certificate2 GetX509CertificateWithPrivateKey(Org.BouncyCastle.X509.X509Certificate bouncyCastleCert, AsymmetricKeyParameter privateKey)
        {
            // this workaround is needed to fill in the Private Key in the X509Certificate2
            string alias = bouncyCastleCert.SubjectDN.ToString();
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();

            X509CertificateEntry certEntry = new X509CertificateEntry(bouncyCastleCert);
            store.SetCertificateEntry(alias, certEntry);

            AsymmetricKeyEntry keyEntry = new AsymmetricKeyEntry(privateKey);
            store.SetKeyEntry(alias, keyEntry, new X509CertificateEntry[] { certEntry });

            byte[] certificateData;
            string password = Guid.NewGuid().ToString();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                store.Save(memoryStream, password.ToCharArray(), new SecureRandom());
                memoryStream.Flush();
                certificateData = memoryStream.ToArray();
            }

            return new X509Certificate2(certificateData, password, X509KeyStorageFlags.Exportable);
        }
    }
}
