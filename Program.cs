
using System;
using System.Collections.Generic;
using System.Data.HashFunction;
using System.Data.HashFunction.xxHash;

class Program
{
    private static readonly IHashFunction Hasher = xxHashFactory.Instance.Create(
        new xxHashConfig { HashSizeInBits = 32 }
    );

    private static readonly Random GlobalRandom = new Random();

    static void Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("=========================================");
            Console.WriteLine("Stardew Valley 1.6 Fairy ID Prediction & Search Tool");
            Console.WriteLine("=========================================");
            Console.WriteLine("1. Predict Fairy Type from Multiplayer ID (1-8)");
            Console.WriteLine("2. Search Random IDs for a Specific Fairy Type");
            Console.WriteLine("0. Exit");
            Console.Write("Select an option (0-2): ");

            string? choice = Console.ReadLine();
            Console.WriteLine();

            if (choice == "1")
            {
                RunPredictionFeature();
            }
            else if (choice == "2")
            {
                RunSearchFeature();
            }
            else if (choice == "0" || choice?.ToLower() == "exit")
            {
                break;
            }
            else
            {
                Console.WriteLine("Invalid selection. Please try again.\n");
            }
        }
    }

    private static void RunPredictionFeature()
    {
        Console.Write("Enter Multiplayer ID (e.g. 5336205887356168600): ");
        string? input = Console.ReadLine();

        if (long.TryParse(input, out long multiplayerID))
        {
            int fairyType = CalculateFairyType(multiplayerID, out int seed, out int subVariant, out int startingY);

            Console.WriteLine("\n--- Prediction Result ---");
            Console.WriteLine($"Multiplayer ID              : {multiplayerID}");
            Console.WriteLine($"Computed Seed               : {seed}");
            Console.WriteLine($"Sub-Variant (0-3)           : {subVariant}");
            Console.WriteLine($"Sprite Y Offset (0 or 176)  : {startingY}");
            Console.WriteLine($"Final Fairy Type (1-8)      : {fairyType}");
            Console.WriteLine("------------------------\n");
        }
        else
        {
            Console.WriteLine("Invalid ID format. Please enter a valid 64-bit integer.\n");
        }
    }

    private static void RunSearchFeature()
    {
        Console.Write("Enter target Fairy Type (1-8): ");
        if (!int.TryParse(Console.ReadLine(), out int targetType) || targetType < 1 || targetType > 8)
        {
            Console.WriteLine("Invalid Fairy Type. Please enter a value between 1 and 8.\n");
            return;
        }

        Console.Write("Enter the number of matching IDs to find: ");
        if (!int.TryParse(Console.ReadLine(), out int targetCount) || targetCount <= 0)
        {
            Console.WriteLine("Invalid count. Please enter a positive integer.\n");
            return;
        }

        Console.WriteLine($"\nSearching for Fairy Type [{targetType}], targeting {targetCount} matching IDs. Please wait...\n");

        List<long> foundIds = new List<long>();
        long totalAttempts = 0;

        while (foundIds.Count < targetCount)
        {
            totalAttempts++;
            long randomId = GenerateRandomLong();

            int fairyType = CalculateFairyType(randomId, out _, out _, out _);
            if (fairyType == targetType)
            {
                foundIds.Add(randomId);
                Console.WriteLine($"[Progress {foundIds.Count}/{targetCount}] Found matching ID: {randomId}");
            }
        }

        Console.WriteLine("\n================ Search Complete ================");
        Console.WriteLine($"Target Fairy Type : {targetType}");
        Console.WriteLine($"Matches Found     : {foundIds.Count}");
        Console.WriteLine($"Total Attempts    : {totalAttempts}");
        Console.WriteLine($"Match Rate        : {(double)foundIds.Count / totalAttempts * 100:F4}%");
        Console.WriteLine("---------------- Matching IDs ------------------");

        foreach (var id in foundIds)
        {
            Console.WriteLine(id);
        }

        Console.WriteLine("================================================\n");
    }

    private static int CalculateFairyType(long multiplayerID, out int seed, out int subVariant, out int startingY)
    {
        seed = CreateRandomSeed((double)multiplayerID, 0.0);
        Random r = new Random(seed);

        subVariant = r.Next(4);
        bool isAlternateColor = r.NextDouble() < 0.5;
        startingY = isAlternateColor ? 176 : 0;

        return subVariant + (isAlternateColor ? 4 : 0) + 1;
    }

    public static long GenerateRandomLong()
    {
        byte[] buf = new byte[8];
        GlobalRandom.NextBytes(buf);
        return BitConverter.ToInt64(buf, 0);
    }

    public static int CreateRandomSeed(double seedA, double seedB = 0.0, double seedC = 0.0, double seedD = 0.0, double seedE = 0.0)
    {
        return GetDeterministicHashCode(
            (int)(seedA % 2147483647.0),
            (int)(seedB % 2147483647.0),
            (int)(seedC % 2147483647.0),
            (int)(seedD % 2147483647.0),
            (int)(seedE % 2147483647.0)
        );
    }

    public static int GetDeterministicHashCode(params int[] values)
    {
        byte[] data = new byte[values.Length * 4];
        Buffer.BlockCopy(values, 0, data, 0, data.Length);
        return GetDeterministicHashCode(data);
    }

    public static int GetDeterministicHashCode(byte[] data)
    {
        IHashValue hashValue = Hasher.ComputeHash(data);
        return BitConverter.ToInt32(hashValue.Hash, 0);
    }
}
