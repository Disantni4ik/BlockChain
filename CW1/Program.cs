using CW1.Models;
using CW1.Services;

BlockChainService blockchain = new BlockChainService();
WalletService walletService = new WalletService(blockchain.Chain);
VanityWalletService vanityWalletService = new VanityWalletService();

Wallet userWallet = vanityWalletService.MineWallet("aa").wallet;
string authMessage = "This is my custom wallet";

//#1
byte[] userSignature = walletService.SignMessage(userWallet, authMessage);

bool isUserValid = walletService.VerifyMessage(userWallet.Address, userWallet.PublicKey, authMessage, userSignature);

if (isUserValid)
{
    Console.WriteLine("Authorization successful\n");
}

//#2
Wallet hackerWallet = walletService.CreateWallet("Hacker");

byte[] hacherSignature = walletService.SignMessage(hackerWallet, authMessage);

bool isHackerValid = walletService.VerifyMessage(userWallet.Address, hackerWallet.PublicKey, authMessage, hacherSignature);

if (!isHackerValid)
{
    Console.WriteLine("Hacking attempt blocked");
}else
{
    Console.WriteLine("The server let the hacker in");
}

