using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileSlicer
{
    class FileSliceProject
    {
        public record DestinationFile(FileInfo File, int LineCount);
        private readonly List<DestinationFile> destinationFiles;
        
        public FileSliceProject(string inputFilePath)
        {
            InputFile = new FileInfo(inputFilePath);
            destinationFiles = new();
        }
        
        public FileInfo InputFile { get; }
        public int HeaderLineCount { get; set; }
        public int FooterLineCount { get; set; }
        public int NextFileNumber => destinationFiles.Count + 1;
        public int TotalLinesRead { get; private set; }

        public void AddFile(int lineCount)
        {
            var fileNumber = NextFileNumber;
            var destDir = InputFile.Directory.FullName;
            var destFileNamePrefix = Path.GetFileNameWithoutExtension(InputFile.FullName);
            var fileName = $@"{destDir}\{destFileNamePrefix}-{fileNumber}{InputFile.Extension}";
            
            destinationFiles.Add(new DestinationFile(new FileInfo(fileName), lineCount));
        }

        public void MakeSlices()
        {
            using var readStream = InputFile.OpenRead();
            using var reader = new StreamReader(readStream);
            TotalLinesRead = 0;
            var headerLines = new List<string>();
            var history = new CircularBuffer<string>(FooterLineCount);
            var filesWritten = 0;
            
            string ReadLine()
            {
                var line = reader.ReadLine();
                if (line != null)
                {
                    history.PushFront(line);
                    TotalLinesRead++;
                }
                return line;
            }

            for (var i = 0; i < HeaderLineCount; i++)
            {
                var line = ReadLine();

                if (line == null) 
                {
                    throw new Exception($"The source file contained only {i} lines.");
                }

                headerLines.Add(line);
            }

            foreach (var destFile in destinationFiles)
            {
                using var writeStream = destFile.File.OpenWrite();
                using var writer = new StreamWriter(writeStream);
                var linesWritten = 0;

                foreach (var headerLine in headerLines)
                {
                    writer.WriteLine(headerLine);
                }

                for (var i = 0; i < destFile.LineCount; i++)
                {
                    var line = ReadLine();
                    
                    if (line == null)
                    {
                        var fileNum = filesWritten + 1;
                        var lineNum = linesWritten + 1;
                        throw new Exception(
                            "Reached end of input file when writing destination " +
                            $"file #{fileNum}, line #{lineNum}.");
                    }

                    writer.WriteLine(line);
                    linesWritten++;
                }
            }

            while (ReadLine() != null)
            {
            }

            var footerLines = history.ToList();
            footerLines.Reverse();

            foreach (var destinationFile in destinationFiles)
            {
                using var writer = File.AppendText(destinationFile.File.FullName);
                foreach (var footerLine in footerLines)
                {
                    writer.Write(footerLine);
                }
            }
        }
    }
}
