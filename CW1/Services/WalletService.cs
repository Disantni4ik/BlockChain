using CW1.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CW1.Services
{
    public class WalletService
    {
        private readonly List<Block> _blockChain;
        public WalletService(List<Block> blockChain)
        {
            _blockChain = blockChain;
        }

        public Wallet CreateWallet(string name)
        {
            using var ecdsa = System.Security.Cryptography.ECDsa.Create();

            byte[] privateKey = ecdsa.ExportECPrivateKey();
            byte[] publicKey = ecdsa.ExportSubjectPublicKeyInfo();

            string address = Convert.ToBase64String(publicKey);
            return new Wallet(name, address, publicKey, privateKey);
        }
        public bool VerifySignature(string publicKey, byte[] signature, byte[] data)
        {
            using var ecdsa = System.Security.Cryptography.ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
            return ecdsa.VerifyData(data, signature, System.Security.Cryptography.HashAlgorithmName.SHA256);
        }
        public decimal GetBalance(string adress)
        {
            decimal balance = 0;
            foreach (var block in _blockChain)
            {
                foreach (var transaction in block.Transactions)
                {
                    if (transaction.From == adress)
                    {
                        balance -= transaction.Amount;
                    }
                    if (transaction.To == adress)
                    {
                        balance += transaction.Amount;
                    }
                }
            }
            return balance;
        }
    }
}
