using CW1.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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
        public byte[] SignMessage(Wallet wallet, string message)
        {
            if (wallet == null) throw new ArgumentNullException(nameof(wallet));
            if (string.IsNullOrEmpty(message)) throw new ArgumentException("Message cannot be empty", nameof(message));

            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            return wallet.Sign(messageBytes);
        }
        public bool VerifyMessage(string claimedAddress, byte[] publicKey, string message, byte[] signature)
        {
            if (string.IsNullOrEmpty(claimedAddress) || publicKey == null || string.IsNullOrEmpty(message) || signature == null)
            {
                return false;
            }

            byte[] hashBytes = SHA256.HashData(publicKey);
            string generatedAddressFromKey = Convert.ToBase64String(hashBytes);

            if (claimedAddress != generatedAddressFromKey)
            {
                Console.WriteLine("Authorization failed: Invalid public key");
                Console.ResetColor();
                return false;
            }

            try
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                using var ecdsa = ECDsa.Create();
                ecdsa.ImportSubjectPublicKeyInfo(publicKey, out _);

                bool isSignatureValid = ecdsa.VerifyData(messageBytes, signature, HashAlgorithmName.SHA256);

                if (!isSignatureValid)
                {
                    Console.WriteLine("Authorization failed: Invali signature");
                    Console.ResetColor();
                }

                return isSignatureValid;
            }
            catch (Exception)
            {
                return false;
            }
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
