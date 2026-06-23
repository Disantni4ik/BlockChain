using CW1.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CW1.Services
{
    public class BlockChainService
    {
        public List<Block> Chain { get; set; }

        private readonly HashingService _hashingService;
        private readonly MiningService _miningService;
        private readonly TransactionService _transactionService;
        private readonly WalletService _walletService;

        public int Difficulty { get; set; }
        public double _targetBlockTime = 10;
        private readonly int _adjustmentInterval = 2;
        private readonly decimal _rewardAmount = 50;
        public int MaxBlockSizeBytes { get; } = 256;
        public decimal MaxSuply { get; } = 1000;
        public decimal TotalMined { get; private set; } = 0;

        public BlockChainService()
        {
            Chain = new List<Block>();
            _hashingService = new HashingService();
            _miningService = new MiningService(_hashingService);
            _transactionService = new TransactionService(Chain);
            _walletService = new WalletService(Chain);
            Difficulty = 5;

            CreateGenesisBlock();
        }
        private void CreateGenesisBlock()
        {
            Block genesisBlock = new Block(0, DateTime.UtcNow, new List<Transaction>(), "0", Difficulty);
            _miningService.MineBlock(genesisBlock, Difficulty);
            genesisBlock.Hash = _hashingService.ComputeHash(genesisBlock);
            Chain.Add(genesisBlock);
        }
        public void ProcessTransactions(List<Transaction> incomingTransactions, string minerAddress)
        {
            List<Transaction> currentChunk = new List<Transaction>();
            int currentChunkSize = 0;

            Console.WriteLine($"[START] Processing {incomingTransactions.Count} incoming transactions...");

            for (int i = 0; i < incomingTransactions.Count; i++)
            {
                Transaction tx = incomingTransactions[i];

                var validation = _transactionService.ValidateTransaction(tx);
                if (!validation.isValid)
                {
                    Console.WriteLine($"[REJECTED] Transaction #{i} failed validation: {validation.errorMessage}");
                    continue;
                }

                int txSize = Encoding.UTF8.GetByteCount(tx.ToRawString());

                if (txSize > MaxBlockSizeBytes)
                {
                    Console.WriteLine($"[REJECTED] Transaction #{i} size ({txSize} bytes) exceeds MaxBlockSizeBytes ({MaxBlockSizeBytes}).");
                    continue;
                }

                if (currentChunkSize + txSize > MaxBlockSizeBytes)
                {
                    Console.WriteLine($"[BLOCK FULL] Size {currentChunkSize} bytes. Mining current chunk...");
                    ExecuteMiningOfChunk(currentChunk, currentChunkSize, minerAddress);

                    currentChunk = new List<Transaction>();
                    currentChunkSize = 0;
                }

                currentChunk.Add(tx);
                currentChunkSize += txSize;
            }

            if (currentChunk.Count > 0)
            {
                Console.WriteLine($"[FINAL BLOCK] Mining remaining {currentChunk.Count} transactions...");
                ExecuteMiningOfChunk(currentChunk, currentChunkSize, minerAddress);
            }
        }
        private void ExecuteMiningOfChunk(List<Transaction> transactions, int totalSize, string minerAddress)
        {
            Block previousBlock = Chain.Last();

            if (TotalMined + _rewardAmount <= MaxSuply)
            {
                var rewardTransaction = new Transaction("COINBASE", minerAddress, _rewardAmount, new byte[0]);
                transactions.Add(rewardTransaction);
                TotalMined += _rewardAmount;
            }
            else if (TotalMined < MaxSuply)
            {
                decimal remainingReward = MaxSuply - TotalMined;
                var rewardTransaction = new Transaction("COINBASE", minerAddress, remainingReward, new byte[0]);
                transactions.Add(rewardTransaction);
                TotalMined = MaxSuply;
            }

            Block newBlock = new Block(
                previousBlock.Index + 1,
                DateTime.UtcNow,
                transactions,
                previousBlock.Hash,
                Difficulty
            )
            {
                Size = totalSize
            };

            _miningService.MineBlock(newBlock, Difficulty);
            Chain.Add(newBlock);

            if (newBlock.Index % _adjustmentInterval == 0)
            {
                AdjustDifficulty();
            }
        }

        private void AdjustDifficulty()
        {
            var recentBlocks = Chain.Skip(Math.Max(0, Chain.Count - _adjustmentInterval)).ToList();
            double avgTime = recentBlocks.Average(b => b.MiningDuration);

            if (avgTime < _targetBlockTime)
            {
                Difficulty++;
                //Console.WriteLine($"Increased difficulty to {Difficulty} (avg mining time: {avgTime:F2} seconds)");
            }
            else if (avgTime > _targetBlockTime && Difficulty > 1)
            {
                Difficulty--;
                //Console.WriteLine($"Decreased difficulty to {Difficulty} (avg mining time: {avgTime:F2} seconds)");
            }
        }

        public Block FindBlockByHash(string targetHash)
        {
            return Chain.FirstOrDefault(b => b.Hash == targetHash);
        }
        public bool IsValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                Block currentBlock = Chain[i];
                Block previousBlock = Chain[i - 1];
                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                {
                    return false;
                }
                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    return false;
                }

                //if (!currentBlock.Hash.StartsWith(new string('0', currentBlock.Difficulty)))
                //{
                //    return false;
                //}`    
                if (!currentBlock.Hash.StartsWith(currentBlock.DiffucultyText))
                {
                    return false;
                }

                if (currentBlock.TimeStamp <= previousBlock.TimeStamp)
                {
                    return false;
                }

                if (currentBlock.MiningDuration <= 0)
                {
                    return false;
                }

                //double timeDifference = (currentBlock.TimeStamp - previousBlock.TimeStamp).TotalSeconds;
                //if (currentBlock.MiningDuration > timeDifference)
                //{
                //    return false;
                //}
            }
            return true;
        }
        public int GetInvalidBlockIndex()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                Block currentBlock = Chain[i];
                Block previousBlock = Chain[i - 1];
                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock) || currentBlock.PreviousHash != previousBlock.Hash)
                {
                    return currentBlock.Index;
                }
            }
            return -1;
        }
        public void CompareHashes(string hash1, string hash2)
        {
            int sameChar = 0;
            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] == hash2[i])
                {
                    sameChar++;
                }
            }

            Console.WriteLine($"Hash 1: {hash1}");
            Console.WriteLine($"Hash 2: {hash2}");

            Console.WriteLine($"Number of matching characters: {sameChar} out of {hash1.Length}");
            Console.WriteLine($"Percentage of matching characters: {(double)sameChar / hash1.Length * 100:F2}%");
        }
        public Block GetLuckiestBlock()
        {
            foreach (Block block in Chain)
            {
                var firstChar = block.Hash[0];
                var repeatCount = 0;
                for (int i = 1; i <= 4; i++)
                {   
                    if (firstChar == block.Hash[i]) {
                        repeatCount++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (repeatCount >= 3)
                {
                    return block;
                }
            }
            return null;
        }
    }
}
