using CW1.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CW1.Services
{
    public class TransactionService
    {
        private readonly WalletService _walletService;
        public TransactionService(List<Block> blockChain)
        {
            _walletService = new WalletService(blockChain);
        }
        public Transaction CreateTransaction(Wallet walletFrom, string to, decimal amount, byte[] senderPublicKey, decimal fee)
        {
            var balance = _walletService.GetBalance(walletFrom.Address);

            if (balance < amount)
            {
                throw new Exception("Not enought coins");
            }

            var newTransaction = new Transaction(walletFrom.Address, to, amount, senderPublicKey) {
                Fee = fee,
            };
            newTransaction.Signature = walletFrom.Sign(newTransaction.GetDataToSign());

            var validation = ValidateTransaction(newTransaction);
            if (validation.isValid)
            {
                return newTransaction;
            }
            throw new Exception("Invalid transaction data");
        }

        public (bool isValid, string errorMessage) ValidateTransaction(Transaction transaction)
        {
            if (transaction == null)
                return (false, "Transaction cannot be null");

            if (string.IsNullOrWhiteSpace(transaction.From))
                return (false, "Invalid sender address");
            if (string.IsNullOrWhiteSpace(transaction.To))
                return (false, "Invalid recipient address");

            if (transaction.Amount <= 0)
                return (false, "Invalid transaction amount");

            if (transaction.From == "COINBASE")
                return (true, string.Empty);

            if (!_walletService.VerifySignature(transaction.From, transaction.Signature, transaction.GetDataToSign()))
                return (false, "Invalid transaction signature");

            return (true, string.Empty);
        }
    }
}
