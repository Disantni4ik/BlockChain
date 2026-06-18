using CW1.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CW1.Services
{
    public class TransactionService
    {
        public Transaction CreateTransaction(string from, string to, decimal amount)
        {
            var newTransaction = new Transaction(from, to, amount);
            if (ValidateTransaction(newTransaction).isValid)
            {
                return newTransaction;
            }
            throw new Exception("Invalid transaction data");
        }

        public (bool isValid, string errorMessage) ValidateTransaction(Transaction transaction)
        {
            if(transaction == null)
                return (false, "Transaction cannot be null");
            if (string.IsNullOrWhiteSpace(transaction.From))
                return (false, "Invalid sender address");

            if (string.IsNullOrWhiteSpace(transaction.To))
                return (false, "Invalid recipient address");

            if (transaction.Amount <= 0)
                return (false, "Invalid transaction amount");

            return (true, string.Empty);
        }
    }
}
