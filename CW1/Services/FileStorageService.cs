using CW1.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CW1.Services
{
    public class FileStorageService
    {
        private readonly string _blockChainFilePath = "blockchain.json";
        private readonly string _walletFilePath = "wallets.json";

        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };

        public void SaveBlockchain(List<Block> blockChin)
        {
            string json = JsonSerializer.Serialize(blockChin, _jsonSerializerOptions);

            if (File.Exists(_blockChainFilePath))
            {
                if (File.Exists("blockchain_backup.json"))
                {
                    File.Delete("blockchain_backup.json");
                }
                File.Copy(_blockChainFilePath, "blockchain_backup.json");
            }
            File.WriteAllText(_blockChainFilePath, json);
        }

        public List<Block> LoadBlockChain()
        {
            if (!File.Exists(_blockChainFilePath)) {
                return new List<Block>();
            }

            try
            {
                string json = File.ReadAllText(_blockChainFilePath);
                return JsonSerializer.Deserialize<List<Block>>(json) ?? new List<Block>();
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[CRITICAL ERROR] Blockchain file corrupted");
                Console.ForegroundColor = ConsoleColor.White;
                File.Move(_blockChainFilePath, "blockchain_corrupted.json");
                
                if (File.Exists("blockchain_backup.json"))
                {
                    Console.ForegroundColor= ConsoleColor.Yellow;
                    Console.WriteLine("🔄 Attempting to restore the database from a backup...");
                    Console.ForegroundColor = ConsoleColor.White;

                    string newJson = File.ReadAllText("blockchain_backup.json");
                    return JsonSerializer.Deserialize<List<Block>>(newJson) ?? new List<Block>();
                }
                return null;
            }
            return null;
        }

        public void SaveWallets(List<Wallet> wallets)
        {
            string json = JsonSerializer.Serialize(wallets, _jsonSerializerOptions);
            File.WriteAllText(_walletFilePath, json);
        }

        public List<Wallet> LoadWallets()
        {
            if (!File.Exists(_walletFilePath))
            {
                return new List<Wallet>();
            }

            string json = File.ReadAllText(_walletFilePath);
            return JsonSerializer.Deserialize<List<Wallet>>(json) ?? new List<Wallet>();
        }
    }
}
