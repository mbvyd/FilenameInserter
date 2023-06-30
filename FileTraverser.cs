using System.Collections.Generic;
using System.IO;
using System.Linq;
using SearchOption = System.IO.SearchOption;

namespace FilenameInserter;

internal class FileTraverser
{
    private readonly IEnumerable<string> _filePaths;
    private readonly string _separator;
    private readonly bool _isAppend;

    public FileTraverser(
        string dirPath,
        string separator,
        bool isAppend,
        bool recursive,
        string fileExtensions)
    {
        SearchOption searchOption = recursive
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        _filePaths = GetFilePaths(
            dirPath, fileExtensions, searchOption);

        _separator = separator;
        _isAppend = isAppend;
    }

    public void Process()
    {
        foreach (string path in _filePaths)
        {
            string addition = GetFileName(path);

            string[] lines = File.ReadAllLines(path);

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = ModifyLine(lines[i], addition);
            }

            File.WriteAllLines(path, lines);
        }
    }

    private static IEnumerable<string> GetFilePaths(
        string dirPath,
        string fileExtensions,
        SearchOption searchOption)
    {
        char separator = '|';

        IEnumerable<string>? filePaths = default;

        if (fileExtensions.Contains(separator))
        {
            string[] patterns = fileExtensions.Split(separator);

            foreach (string pattern in patterns)
            {
                IEnumerable<string> current = Directory
                    .EnumerateFiles(
                        dirPath, pattern, searchOption);

                filePaths = filePaths != null
                    ? filePaths.Concat(current)
                    : current;
            }
        }
        else
        {
            filePaths = Directory.EnumerateFiles(
                dirPath, fileExtensions, searchOption);
        }

        return filePaths!;
    }

    private static string GetFileName(string path)
    {
        string fileName = Path.GetFileName(path);

        int lastDotIndex = fileName.LastIndexOf('.');

        if (lastDotIndex != -1)
        {
            fileName = fileName[..lastDotIndex] + "_" + fileName[(lastDotIndex + 1)..];
        }

        return fileName;
    }

    private string ModifyLine(string line, string addition)
    {
        return _isAppend
            ? $"{line}{_separator}{addition}"
            : $"{addition}{_separator}{line}";
    }
}