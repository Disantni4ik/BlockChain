using CW1.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CW1.Services
{
    public class BlockChainDisplayService
    {
        public void PrintBlock(Block block)
        {
            Console.WriteLine($"Index: {block.Index}");
            Console.WriteLine($"TimeStep: {block.TimeStamp}");
            Console.WriteLine($"PreviousHash: {block.PreviousHash}");
            Console.WriteLine($"Hash: {block.Hash}");
            Console.WriteLine($"Nonce: {block.Nonce}");
            Console.WriteLine($"Mining Duration: {block.MiningDuration} seconds");
            Console.WriteLine($"Transactions: {block.Transactions.Count} | Size: {block.Size} bytes");

            for (int i = 0; i < block.Transactions.Count; i++)
            {
                for (int j = i + 1; j < block.Transactions.Count; j++)
                {
                    if (block.Transactions[i].Id == block.Transactions[j].Id)
                    {
                        Console.WriteLine($"\nWARNING DUPLICATE TRANSACTION FOUND: {block.Transactions[i].Id}\n");
                    }
                }
                PrintTransaction(block.Transactions[i]);
            }

            Console.WriteLine(new string('-', 50));
        }
        public void PrintTransaction(Transaction transaction)
        {
            Console.WriteLine(new string('-', 50));
            Console.WriteLine($"Transaction ID: {transaction.Id}");
            Console.WriteLine($"From: {transaction.From}");
            Console.WriteLine($"To: {transaction.To}");
            Console.WriteLine($"Amount: {transaction.Amount}");
            Console.WriteLine($"TimeStamp: {transaction.TimeStamp}");
            Console.WriteLine(new string('-', 50));
        }
        public void PrintBlockChain(List<Models.Block> chain)
        {
            foreach (var block in chain)
            {
                PrintBlock(block);
            }
        }
        public void PrintValidationResult(bool isValid)
        {
            if (isValid)
            {
                Console.WriteLine("The blockchain is valid.");
            }
            else
            {
                Console.WriteLine("The blockchain is invalid.");
            }
        }
    }
}
