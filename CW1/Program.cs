using CW1.Models;
using CW1.Services;
using System.Transactions;

var displayService = new BlockChainDisplayService();
var hashingService = new HashingService();
//var vanityWalletService = new VanityWalletService();
var blockchainService = new BlockChainService();
var walletService = new WalletService(blockchainService.Chain);
var transactionService = new TransactionService(blockchainService.Chain);

//var wallet1 = vanityWalletService.MineWallet("aa").wallet;
//var wallet2 = vanityWalletService.MineWallet("abcd").wallet;

var wallet1 = walletService.CreateWallet("Alice");
var wallet2 = walletService.CreateWallet("Bob");

while (true)
{
    Console.WriteLine("Menu:");
    Console.WriteLine("1. Mine Block");
    Console.WriteLine("2. Add Transaction | From Alice To Bob");
    Console.WriteLine("3. Add Transaction | From Bob To Alice");
    Console.WriteLine("4. Show Alice Balance");
    Console.WriteLine("5. Show Bob Balance");
    Console.WriteLine("6. Show BlockCHain");
    Console.WriteLine("7. Show Pending Transactions");
    Console.WriteLine("8. Exit");
    Console.Write("Choose an option: ");
    var choice = Console.ReadLine();

    switch (choice) 
    {
        case "1":
            blockchainService.MineBlock(wallet1.Address);
            Console.WriteLine("Block added successfully.");
            break;
        case "2":
            Console.Write("Amount: ");
            int amountAlice = int.Parse(Console.ReadLine());
            Console.Write("Fee: ");
            int feeAlice = int.Parse(Console.ReadLine());
            var transactionAlice = transactionService.CreateTransaction(wallet1, wallet2.Address, amountAlice, wallet1.PublicKey, feeAlice);
            blockchainService.AddTransactionToMempool(transactionAlice);
            Console.WriteLine("Transactions Added");
            break;
        case "3":
            Console.Write("Amount: ");
            int amountBob = int.Parse(Console.ReadLine());
            Console.Write("Fee: ");
            int feeBob = int.Parse(Console.ReadLine());
            var transactionBob = transactionService.CreateTransaction(wallet1, wallet2.Address, amountBob, wallet1.PublicKey, feeBob);
            blockchainService.AddTransactionToMempool(transactionBob);
            Console.WriteLine("Transactions Added");
            break;
        case "4":
            Console.WriteLine(walletService.GetBalance(wallet1.Address));
            break;
        case "5":
            Console.WriteLine(walletService.GetBalance(wallet2.Address));
            break;
        case "6":
            displayService.PrintBlockChain(blockchainService.Chain);
            break;
        case "7":
            foreach (var tr in blockchainService.PendingTransactions)
            {
                Console.WriteLine(new string('-', 50));
                Console.WriteLine($"Transaction ID: {tr.Id}");
                Console.WriteLine($"From: {tr.From}");
                Console.WriteLine($"To: {tr.To}");
                Console.WriteLine($"Fee: {tr.Fee}");
                Console.WriteLine($"Amount: {tr.Amount}");
                Console.WriteLine($"TimeStamp: {tr.TimeStamp}");
            }
            break;
    }
}