using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeneticSearch
{
    class Program
    {
        struct Protein
        {
            public string Name;
            public string Organism;
            public string AminoAcids;
        }


        struct Command
        {
            public string Name;
            public string Parameter1;
            public string Parameter2;
        }

        static void Main(string[] args)
        {
            string sequencesFile = "sequences.0.txt";
            string commandsFile = "commands.0.txt";
            string outputFile = "genedata.txt";

            List<Protein> proteins = ReadSequences(sequencesFile);

            List<Command> commands = ReadCommands(commandsFile);

            ExecuteCommands(proteins, commands, outputFile);

            Console.WriteLine($"Обработка завершена! Результаты сохранены в файл {outputFile}.");
        }

        static string Decoding(string aminoAcids)
        {
            if (string.IsNullOrEmpty(aminoAcids))
                return string.Empty;

            string decoded = string.Empty;
            for (int i = 0; i < aminoAcids.Length; i++)
            {
                char ch = aminoAcids[i];
                if (char.IsDigit(ch))
                {
                    int count = ch - '0';
                    char letter = aminoAcids[i + 1];
                    for (int j = 0; j < count; j++)
                    {
                        decoded += letter;
                    }
                    i++;
                }
                else
                {
                    decoded += ch;
                }
            }
            return decoded;
        }

        static List<Protein> ReadSequences(string filename)
        {
            List<Protein> list = new List<Protein>();
            if (!File.Exists(filename)) return list;

            string[] lines = File.ReadAllLines(filename);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split('\t');
                if (parts.Length >= 3)
                {
                    Protein p = new Protein
                    {
                        Name = parts[0].Trim(),
                        Organism = parts[1].Trim(),
                        AminoAcids = Decoding(parts[2].Trim())
                    };
                    list.Add(p);
                }
            }
            return list;
        }

        static List<Command> ReadCommands(string filename)
        {
            List<Command> commands = new List<Command>();
            if (!File.Exists(filename)) return commands;

            string[] lines = File.ReadAllLines(filename);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split('\t');
                Command cmd = new Command();
                cmd.Name = parts[0].Trim();

                if (parts.Length > 1)
                    cmd.Parameter1 = Decoding(parts[1].Trim());

                if (parts.Length > 2)
                    cmd.Parameter2 = Decoding(parts[2].Trim());
                else
                    cmd.Parameter2 = string.Empty;

                commands.Add(cmd);
            }
            return commands;
        }

        static void ExecuteCommands(List<Protein> proteins, List<Command> commands, string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                // Заголовок выходного файла
                writer.WriteLine("Dwight Barnette");
                writer.WriteLine("Genetic Searching");

                for (int i = 0; i < commands.Count; i++)
                {
                    writer.WriteLine("--------------------------------------------------------------------------");

                    string opNum = (i + 1).ToString("D3");
                    Command cmd = commands[i];

                    if (cmd.Name == "search")
                    {
                        writer.WriteLine($"{opNum} {cmd.Name} {cmd.Parameter1}");
                        writer.WriteLine("organism protein");

                        bool found = false;
                        foreach (var p in proteins)
                        {
                            if (p.AminoAcids.Contains(cmd.Parameter1))
                            {
                                writer.WriteLine($"{p.Organism} {p.Name}");
                                found = true;
                            }
                        }

                        if (!found)
                        {
                            writer.WriteLine("NOT FOUND");
                        }
                    }
                    else if (cmd.Name == "diff")
                    {
                        writer.WriteLine($"{opNum} {cmd.Name} {cmd.Parameter1} {cmd.Parameter2}");
                        writer.WriteLine("amino-acids difference:");

                        Protein? p1 = proteins.FirstOrDefault(p => p.Name == cmd.Parameter1);
                        Protein? p2 = proteins.FirstOrDefault(p => p.Name == cmd.Parameter2);


                        if (!p1.HasValue || !p2.HasValue)
                        {
                            writer.Write("MISSING:");
                            if (!p1.HasValue) writer.Write($" {cmd.Parameter1}");
                            if (!p2.HasValue) writer.Write($" {cmd.Parameter2}");
                            writer.WriteLine();
                        }
                        else
                        {
                            int diffCount = CalculateDifference(p1.Value.AminoAcids, p2.Value.AminoAcids);
                            writer.WriteLine(diffCount);
                        }
                    }
                    else if (cmd.Name == "mode")
                    {
                        writer.WriteLine($"{opNum} {cmd.Name} {cmd.Parameter1}");
                        writer.WriteLine("amino-acid occurs:");

                        Protein? p = proteins.FirstOrDefault(pr => pr.Name == cmd.Parameter1);
                        if (!p.HasValue)
                        {
                            writer.WriteLine($"MISSING: {cmd.Parameter1}");
                        }
                        else
                        {
                            var (mostFrequentChar, count) = GetMostFrequentAminoAcid(p.Value.AminoAcids);
                            writer.WriteLine($"{mostFrequentChar} {count}");
                        }
                    }
                }
            }
        }

        static int CalculateDifference(string seq1, string seq2)
        {
            int minLen = Math.Min(seq1.Length, seq2.Length);
            int diff = 0;

            for (int i = 0; i < minLen; i++)
            {
                if (seq1[i] != seq2[i])
                    diff++;
            }

            diff += Math.Abs(seq1.Length - seq2.Length);

            return diff;
        }

        static (char AminoAcid, int Count) GetMostFrequentAminoAcid(string seq)
        {
            var counts = new Dictionary<char, int>();

            foreach (char c in seq)
            {
                if (counts.ContainsKey(c))
                    counts[c]++;
                else
                    counts[c] = 1;
            }

            var top = counts.OrderByDescending(kv => kv.Value)
                              .ThenBy(kv => kv.Key)
                              .First();

            return (top.Key, top.Value);
        }
    }
}