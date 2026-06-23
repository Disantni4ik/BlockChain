using CW1.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CW1.Services
{
    public class VanityWalletService
    {
        private readonly WalletService _walletService;
        public VanityWalletService()
        {
            //_walletService = new WalletService();
        }
        public (Wallet wallet, long attempts) MineWallet(string desiredPrefix)
        {
            long attempts = 0;
            while (true)
            {
                using var ecdsa = System.Security.Cryptography.ECDsa.Create();
                byte[] PublicKey = ecdsa.ExportSubjectPublicKeyInfo();
                byte[] PrivateKey = ecdsa.ExportECPrivateKey();
                byte[] HashBytes = SHA256.HashData(PublicKey);
                string Hash = Convert.ToBase64String(HashBytes);
                Console.WriteLine(Hash);
                if (Hash.StartsWith(desiredPrefix))
                {
                    Console.WriteLine($"FOUND :{Hash}");
                    return (new Wallet($"User_{desiredPrefix}", Hash, PublicKey, PrivateKey), attempts);
                }
                attempts++;
            }
        }

    }
}
