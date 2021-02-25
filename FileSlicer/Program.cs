using System;

namespace FileSlicer
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Path to source file:");
            var fileName = Console.ReadLine();
            Console.WriteLine();

            var project = new FileSliceProject(fileName)
            {
                HeaderLineCount = ChallengeForWholeNumber("How many header lines?"),
                FooterLineCount = ChallengeForWholeNumber("How many footer lines?")
            };

            while (true)
            {
                var lines = ChallengeForWholeNumber(
                    $"How many data rows in file #{project.NextFileNumber}?",
                    defaultValue: 0);

                if (lines > 0)
                {
                    project.AddFile(lines);
                }
                else
                {
                    break;
                }
            }

            Console.WriteLine("Creating slices");
            project.MakeSlices();

            Console.WriteLine("Done!");
            Console.WriteLine($"Total lines read: {project.TotalLinesRead}");
        }

        static int ChallengeForWholeNumber(string challenge, int? defaultValue = null)
        {
            if (defaultValue != null)
            {
                challenge += $" (default: {defaultValue})";
            }
            
            while(true)
            {
                Console.WriteLine(challenge);
                var input = Console.ReadLine();

                if (input.Trim() == "" && defaultValue != null)
                {
                    return defaultValue.Value;
                }
                
                if (int.TryParse(input, out var value) && value >= 0)
                {
                    Console.WriteLine();
                    return value;
                }

                var response = "Please input a whole number";
                if (defaultValue != null)
                {
                    response += $" (or leave it blank to default to {defaultValue})";
                }
                
                Console.WriteLine();
                Console.WriteLine(response + ".");
                Console.WriteLine();
            }
        }
    }
}
