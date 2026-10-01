using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeneticSearch
{
    class Protein
    {
        public string Name { get; set; }
        public string Organism { get; set; }
        public string Sequence { get; set; }
    }

    class Command
    {
        public string Type { get; set; }
        public string Target1 { get; set; }
        public string Target2 { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: dotnet run <commands_file> <sequences_file> [output_file]");
                return;
            }

            string commandFile = args[0];
            string sequenceFile = args[1];
            string outputFile = args.Length > 2 ? args[2] : "genedata.txt";

            List<Protein> proteins = LoadProteins(sequenceFile);
            List<Command> commands = LoadCommands(commandFile);

            ExecuteCommands(proteins, commands, outputFile);
        }

        static List<Protein> LoadProteins(string filepath)
        {
            var proteins = new List<Protein>();
            if (!File.Exists(filepath)) return proteins;

            string[] lines = File.ReadAllLines(filepath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split('\t');
                if (parts.Length >= 3)
                {
                    proteins.Add(new Protein
                    {
                        Name = parts[0].Trim(),
                        Organism = parts[1].Trim(),
                        Sequence = parts[2].Trim()
                    });
                }
            }
            return proteins;
        }

        static List<Command> LoadCommands(string filepath)
        {
            var commands = new List<Command>();
            if (!File.Exists(filepath)) return commands;

            string[] lines = File.ReadAllLines(filepath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split('\t');
                if (parts.Length > 0)
                {
                    var cmd = new Command { Type = parts[0].Trim() };
                    if (parts.Length > 1) cmd.Target1 = parts[1].Trim();
                    if (parts.Length > 2) cmd.Target2 = parts[2].Trim();
                    commands.Add(cmd);
                }
            }
            return commands;
        }

        static void ExecuteCommands(List<Protein> proteins, List<Command> commands, string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                writer.WriteLine("Dwight Barnette");
                writer.WriteLine("Genetic Searching");
                writer.WriteLine("--------------------------------------------------------------------------");

                int index = 1;
                foreach (var cmd in commands)
                {
                    string header = $"{index:D3} {cmd.Type}";
                    if (!string.IsNullOrEmpty(cmd.Target1)) header += $" {cmd.Target1}";
                    if (!string.IsNullOrEmpty(cmd.Target2)) header += $" {cmd.Target2}";
                    writer.WriteLine(header);

                    if (cmd.Type == "search")
                    {
                        var match = proteins.FirstOrDefault(p => p.Sequence != null && p.Sequence.Contains(cmd.Target1));
                        if (match != null)
                        {
                            writer.WriteLine("organism protein");
                            writer.WriteLine($"{match.Organism} {match.Name}");
                        }
                        else
                        {
                            writer.WriteLine("organism protein");
                            writer.WriteLine("NOT FOUND");
                        }
                    }
                    else if (cmd.Type == "diff")
                    {
                        var p1 = proteins.FirstOrDefault(p => p.Name != null && p.Name.Equals(cmd.Target1, StringComparison.OrdinalIgnoreCase));
                        var p2 = proteins.FirstOrDefault(p => p.Name != null && p.Name.Equals(cmd.Target2, StringComparison.OrdinalIgnoreCase));

                        if (p1 == null || p2 == null || p1.Sequence == null || p2.Sequence == null)
                        {
                            writer.WriteLine("NOT FOUND");
                        }
                        else
                        {
                            int diff = CalculateDifference(p1.Sequence, p2.Sequence);
                            writer.WriteLine("amino-acids difference:");
                            writer.WriteLine(diff);
                        }
                    }
                    else if (cmd.Type == "mode")
                    {
                        var match = proteins.FirstOrDefault(p => p.Name != null && p.Name.Equals(cmd.Target1, StringComparison.OrdinalIgnoreCase));
                        if (match == null || string.IsNullOrEmpty(match.Sequence))
                        {
                            writer.WriteLine("NOT FOUND");
                        }
                        else
                        {
                            var mostCommon = match.Sequence
                                .GroupBy(c => c)
                                .OrderByDescending(g => g.Count())
                                .ThenBy(g => g.Key)
                                .FirstOrDefault();

                            writer.WriteLine("amino-acid occurs:");
                            writer.WriteLine($"{mostCommon.Key} {mostCommon.Count()}");
                        }
                    }

                    writer.WriteLine("--------------------------------------------------------------------------");
                    index++;
                }
            }
        }

        public static int CalculateDifference(string seq1, string seq2)
        {
            if (seq1 == null || seq2 == null) return 0;

            int diffCount = 0;
            int minLength = Math.Min(seq1.Length, seq2.Length);

            for (int i = 0; i < minLength; i++)
            {
                if (seq1[i] != seq2[i])
                {
                    diffCount++;
                }
            }

            diffCount += Math.Abs(seq1.Length - seq2.Length);
            return diffCount;
        }
    }
}