using CW1.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CW1.Services
{
    public class MiningService
    {
        private readonly HashingService _hashingService;
        private static readonly object _lock = new object();
        private bool _found = false;
        private Block _winningBlock;

        private long _totalHashesChecked = 0;

        public MiningService(HashingService hashingService)
        {
            _hashingService = hashingService;
        }

        public long MineBlock(Block block, int difficulty)
        {
            //string target = new string('0', difficulty);
            string target = "CAFE";
            _winningBlock = null;
            _found = false;
            _totalHashesChecked = 0;

            int threadsCount = Environment.ProcessorCount;
            var stopWatch = Stopwatch.StartNew();

            Parallel.For(
                0,
                int.MaxValue,
                new ParallelOptions { MaxDegreeOfParallelism = threadsCount },
                () => 0L,
                (nonce, state, localHashes) =>
                {
                    if (_found) state.Stop();

                    Block newBlock = block.Copy();
                    newBlock.Nonce = nonce;
                    newBlock.Hash = _hashingService.ComputeHash(newBlock);

                    //Console.WriteLine($"Checking nonce: {newBlock.Hash}");

                    localHashes++;

                    if (newBlock.Hash.StartsWith(target))
                    {
                        lock (_lock)
                        {
                            if (!_found)
                            {
                                _found = true;
                                _winningBlock = newBlock;
                                stopWatch.Stop();
                                block.MiningDuration = stopWatch.Elapsed.TotalSeconds;
                                state.Stop();
                            }
                        }
                    }

                    return localHashes;
                },
                (finalLocalHashes) =>
                {
                    Interlocked.Add(ref _totalHashesChecked, finalLocalHashes);
                }
            );

            if (!_found)
            {
                throw new Exception("Failed to mine block within nonce limit.");
            }

            double duration = block.MiningDuration > 0 ? block.MiningDuration : 0.001;
            double hashrate = _totalHashesChecked / duration;
            double khs = hashrate / 1000;
            double mhs = khs / 1000;

            //Console.WriteLine("\n--- MINING RESULTS ---");
            //Console.WriteLine($"Threads Used:          {threadsCount}");
            //Console.WriteLine($"Difficulty:            {difficulty}");
            //Console.WriteLine($"Time Elapsed:          {duration:F4} seconds");
            //Console.WriteLine($"Total Hashes Checked:  {_totalHashesChecked:N0}");

            //if (mhs >= 1)
            //    Console.WriteLine($"Final Hashrate:        {mhs:F2} MH/s");
            //else
            //    Console.WriteLine($"Final Hashrate:        {khs:F2} KH/s");

            //Console.WriteLine($"Winning Nonce:         {_winningBlock.Nonce}");
            //Console.WriteLine($"Block Hash:            {_winningBlock.Hash}");
            //Console.WriteLine("-----------------------\n");

            block.Nonce = _winningBlock.Nonce;
            block.Hash = _winningBlock.Hash;
            block.DiffucultyText = target;
            return _winningBlock.Nonce;
        }
    }
}