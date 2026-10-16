using DocumentFormat.OpenXml.Office2010.CustomUI;
using HyakuninKumiteApplication.Entities;
using HyakuninKumiteApplication.Entities.Category;
using System;
using System.Collections.Generic;
using System.Text;

namespace HyakuninKumiteApplication.Services
{
    internal static class CMDService
    {
        public static void Start()
        {
            while (true)
            {
                Console.Write("> ");

                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                CommandRegistry.Execute(input);
            }
        }
    }

    internal static class CommandRegistry
    {
        public static Category? selectedCategory;
        public static string? v3rdMessage;
        public static string? v1stMessage;

        public static string? roundCountMessage;

        public static void Execute(string input)
        {
            string[] parts = input.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries
            );

            string command = parts[0];

            switch (command)
            {
                case "help":
                    Commands.Help();
                    break;

                case "clear":
                    Commands.Clear();
                    break;

                case "test":
                    Commands.Test();
                    break;

                case "v1st":
                    Console.WriteLine(v1stMessage);
                    break;

                case "v3rd":
                    Console.WriteLine(v3rdMessage);
                    break;

                case "wc":
                    Console.WriteLine($"Current Selected WC: {selectedCategory?.Name}");
                    break;

                case "wcres":
                    Console.WriteLine($"1st Place: {selectedCategory?.Result[0]?.Name}\n2nd Place: {selectedCategory?.Result[1]?.Name}\n3rd Place: {selectedCategory?.Result[2]?.Name}\n4th Place: {selectedCategory?.Result[3]?.Name}");
                    break;

                case "wcpart":
                    Commands.WCPART(selectedCategory);
                    break;

                case "wcround":
                    Console.WriteLine($"Current Selected WC has {(selectedCategory as WeightCategory).Rounds.Count} rounds");
                    break;

                case "wcmatch":
                    Commands.WCMATCH(selectedCategory as WeightCategory);
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}");
                    break;
            }
        }
    }

    internal static class Commands
    {
        public static void Help()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("  help");
            Console.WriteLine("  clear");
            Console.WriteLine("  test");
            Console.WriteLine("  v1st - View the 1st place match in the current selected WC *if existent");
            Console.WriteLine("  v3rd - View the 3rd place match in the current selected WC *if existent");
            Console.WriteLine("  wc - View the name of the selected WC");
            Console.WriteLine("  wcres - View the results of the selected WC");
            Console.WriteLine("  wcpart - View the Participant list of the selected WC");
            Console.WriteLine("  wcmatch - View the Matches list of the selected WC");
        }

        public static void Clear()
        {
            Console.Clear();
        }

        public static void Test()
        {
            Console.WriteLine("Test command executed!");
        }

        public static void WCPART(Category wc)
        {
            string message = $"Participant List - {wc.Parts.Count}";

            List<Participant> parts = wc.Parts.OrderBy(p => p.CompId).ToList();

            foreach(var p in parts)
            {
                message += $"\n[{p.CompId}] {p.Name} {p.Surname}";
            }

            Console.WriteLine(message);
        }

        public static void WCMATCH(WeightCategory wc)
        {
            if(wc == null)
                return;

            Console.WriteLine($"=== Match Pair Details ===");

            foreach(var m in wc.Rounds[0].Matches)
            {
                for (int i = 0; i < m.Pair.Count; i++)
                {
                    Participant participant = m.Pair[i];
                    Console.WriteLine($"--- Slot {i} ---");

                    if (participant != null)
                    {
                        Console.WriteLine(participant.ToShortenedString());
                    }
                    else
                    {
                        Console.WriteLine("Raw Null Participant");
                    }
                }
            }

        }
    }

}
