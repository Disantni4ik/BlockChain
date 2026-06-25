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
        private readonly FileStorageService _fileStorageService;

        public int Difficulty { get; set; }
        public double _targetBlockTime = 10;
        public int MaxBlockSizeBytes { get; } = 2048;
        public int MaxMempoolSize { get; } = 5;
        public decimal MaxSuply { get; } = 1000;
        public decimal TotalMined { get; private set; } = 0;

        private readonly int _adjustmentInterval = 2;
        private readonly decimal _rewardAmount = 20;
        private readonly int maxTransactionAmount = 2;

        public List<Transaction> PendingTransactions { get; set; } = new List<Transaction>();

        public BlockChainService()
        {
            Chain = new List<Block>();
            _fileStorageService = new FileStorageService();
            _hashingService = new HashingService();
            _miningService = new MiningService(_hashingService);
            Difficulty = 5;
            _transactionService = new TransactionService(Chain);
            _walletService = new WalletService(Chain);


            var loadedChain = _fileStorageService.LoadBlockChain();

            if (loadedChain != null && loadedChain.Count > 0)
            {
                Chain = loadedChain;
                _transactionService = new TransactionService(Chain);
                _walletService = new WalletService(Chain);
            }
            else
            {
                CreateGenesisBlock();
                _fileStorageService.SaveBlockchain(Chain);
            }

            if (!IsValid())
            {
                throw new Exception("BlockChain Is invalid");
            }
        }
        private void CreateGenesisBlock()
        {
            Block genesisBlock = new Block(0, DateTime.UtcNow, new List<Transaction>(), "0", Difficulty);
            _miningService.MineBlock(genesisBlock, Difficulty);
            genesisBlock.Hash = _hashingService.ComputeHash(genesisBlock);
            Chain.Add(genesisBlock);
        }
        public void MineBlock(string minerAdress)
        {
            List<Transaction> validTransactions = new();
            int totalSize = 0;
            decimal tempBalance = _walletService.GetBalance(minerAdress);

            foreach (Transaction transaction in PendingTransactions)
            {
                if (tempBalance - transaction.Amount < 0) {
                    continue;
                }
                tempBalance -= transaction.Amount;

                if (!_transactionService.ValidateTransaction(transaction).isValid)
                {
                    throw new InvalidOperationException("Invalid transaction found.");
                }

                //int txSize = Encoding.UTF8.GetByteCount(transaction.ToRawString());

                //if (totalSize + txSize > MaxBlockSizeBytes)
                //{
                //    Console.WriteLine("Block is full. Remaining transactions skipped.");
                //    break;
                //}

                validTransactions.Add(transaction);
                //totalSize += txSize;
            }

            Block previousBlock = Chain.Last();
            
            var sortedTransactions = validTransactions.OrderByDescending(t => t.Fee).Take(maxTransactionAmount).ToList();

            PendingTransactions.RemoveAll(t => sortedTransactions.Contains(t));

            var totalReward = sortedTransactions.Sum(t => t.Fee) + _rewardAmount;

            if (TotalMined + _rewardAmount <= MaxSuply)
            {
                var rewardTransaction = new Transaction("COINBASE", minerAdress, totalReward, new byte[0]);
                sortedTransactions.Add(rewardTransaction);
                TotalMined += _rewardAmount;
            }
            else if (TotalMined + _rewardAmount > MaxSuply && TotalMined <= MaxSuply)
            {
                var rewardTransaction = new Transaction("COINBASE", minerAdress, MaxSuply - TotalMined + totalReward, new byte[0]);
                sortedTransactions.Add(rewardTransaction);
                TotalMined += MaxSuply - TotalMined;
            }

            Block newBlock = new Block(
                previousBlock.Index + 1,
                DateTime.UtcNow,
                sortedTransactions,
                previousBlock.Hash,
                Difficulty
            )
            {
                Size = totalSize
            };

            _miningService.MineBlock(newBlock, Difficulty);

            Chain.Add(newBlock);

            _fileStorageService.SaveBlockchain(Chain);

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

                foreach (var transaction in currentBlock.Transactions)
                {
                    if (transaction.From != "COINBASE" && !_walletService.VerifySignature(transaction.From, transaction.Signature, transaction.GetDataToSign()))
                    {
                        Console.WriteLine($"[CRITICAL THREAT] A fake transaction was detected in block {currentBlock.Index}");
                        return false;
                    }
                }

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
        public void AddTransactionToMempool(Transaction transaction)
        {
            var validationResult = _transactionService.ValidateTransaction(transaction);
            if (!validationResult.isValid)
            {
                throw new InvalidOperationException($"Invalid transaction: {validationResult.errorMessage}");
            }

            if (transaction.From != "COINBASE")
            {
                var senderBalance = GetPendingBalance(transaction.From);
                if (senderBalance < transaction.Amount + transaction.Fee)
                {
                    return;
                }

                if (PendingTransactions.Find(t => t.From == transaction.From && t.To == transaction.To && t.Amount == transaction.Amount) != null)
                {
                    var transactionCopy = PendingTransactions.Find(t => t.From == transaction.From && t.To == transaction.To && t.Amount == transaction.Amount);
                    if (transaction.Fee > transactionCopy.Fee)
                    {
                        PendingTransactions.Remove(transactionCopy);
                        PendingTransactions.Add(transaction);
                        return;
                    }
                    //}else
                    //{
                    //    return;
                    //    //throw new Exception("A similar transaction already exists. Increase fee to replace");
                    //}
                }

                if (PendingTransactions.Count >= MaxMempoolSize)
                {
                    var minFeeTransaction = PendingTransactions.OrderByDescending(t => t.Fee).ToList()[PendingTransactions.Count - 1];
                    if (transaction.Fee > minFeeTransaction.Fee)
                    {
                        PendingTransactions.Remove(minFeeTransaction);
                        PendingTransactions.Add(transaction);
                        return;
                    }
                    else
                    {
                        throw new Exception("Mempool is full. Fee is too low");
                    }
                    
                }
                PendingTransactions.Add(transaction);
            }
        }
        private decimal GetPendingBalance(string address)
        {
            var balance = _walletService.GetBalance(address);
            foreach (var transaction in PendingTransactions)
            {
                if (transaction.From == address)
                {
                    balance -= transaction.Amount + transaction.Fee;
                }
                if (transaction.To == address)
                {
                    balance += transaction.Amount;
                }
            }
            return balance;
        }
    }
}
