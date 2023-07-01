using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SearchOption = System.IO.SearchOption;

namespace FilenameInserter;

internal class FileTraverser
{
    public enum Mode
    {
        Append,
        Prepend,
    }

    private readonly Mode _mode;
    private readonly DirectoryInfo _directoryInfo;
    private readonly List<string> _fileExtensions;
    private readonly SearchOption _searchOption;
    private readonly string _delimiter;
    private readonly bool _silent;

    public FileTraverser(Mode mode, FileTraverserOptions options)
    {
        _mode = mode;

        _searchOption = !options.Recursive
            ? SearchOption.TopDirectoryOnly
            : SearchOption.AllDirectories;

        _directoryInfo = options.DirectoryInfo;
        _fileExtensions = options.FileExtensions;
        _delimiter = options.Delimiter;
        _silent = options.Silent;
    }

    public void Process()
    {
        if (!_silent)
        {
            ShowConfirmation();
        }

        LineModifier lineModifier = _mode switch
        {
            Mode.Append => ModifyLineAppend,
            Mode.Prepend => ModifyLinePrepend,
            _ => ModifyLineAppend,
        };

        IEnumerable<string> filePaths = GetFilePaths(
            _fileExtensions,
            _directoryInfo.FullName,
            _searchOption);

        ModifyAll(filePaths, lineModifier);
    }

    private void ShowConfirmation()
    {
        StringBuilder fileExtensions = new();
        fileExtensions.Append('[');

        for (int i = 0; i < _fileExtensions.Count - 1; i++)
        {
            fileExtensions.Append(_fileExtensions[i]);
            fileExtensions.Append(", ");
        }

        fileExtensions.Append(_fileExtensions[^1]);
        fileExtensions.Append(']');

        Console.WriteLine($"Files {fileExtensions} in folder '{_directoryInfo.FullName}' will be modified. Press enter to continue...");
        Console.ReadLine();
        Console.Clear();
    }

    private static IEnumerable<string> GetFilePaths(
        List<string> fileExtensions,
        string folderPath,
        SearchOption searchOption)
    {
        IEnumerable<string>? filePaths = default;

        foreach (string fileExtension in fileExtensions)
        {
            IEnumerable<string> currentEnumeration = Directory
                .EnumerateFiles(
                    folderPath,
                    $"*.{fileExtension}",
                    searchOption);

            filePaths = filePaths != null
                ? filePaths.Concat(currentEnumeration)
                : currentEnumeration;
        }

        return filePaths!;
    }

    private static void ModifyAll(
        IEnumerable<string> filePaths, LineModifier lineModifier)
    {
        foreach (string path in filePaths)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);

            string[] lines = File.ReadAllLines(path);

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lineModifier(lines[i], fileName);
            }

            File.WriteAllLines(path, lines);
        }
    }

    private delegate string LineModifier(
        string line, string addition);

    private string ModifyLineAppend(
        string line, string addition)
    {
        return $"{line}{_delimiter}{addition}";
    }

    private string ModifyLinePrepend(
        string line, string addition)
    {
        return $"{addition}{_delimiter}{line}";
    }
}