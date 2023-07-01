using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Throw;

namespace FilenameInserter;

internal class FileProcessor
{
    private readonly List<string> _tempFileNames = new();
    private DirectoryInfo? _tempFolder;

    private DirectoryInfo? _folder;
    private List<string>? _fileExtensions;
    private SearchOption _searchOption;

    private bool _inited = false;

    public static void OverwriteOriginal(
        FileInfo tempFile, string originalPath)
    {
        tempFile.MoveTo(originalPath, overwrite: true);
    }

    public void Init(Options options)
    {
        _searchOption = !options.Recursive
            ? SearchOption.TopDirectoryOnly
            : SearchOption.AllDirectories;

        _folder = options.DirectoryInfo;
        _fileExtensions = options.FileExtensions;

        _inited = true;
    }

    public void Validate()
    {
        _inited.Throw().IfFalse();

        if (!_folder!.Exists)
        {
            throw new DirectoryNotFoundException();
        }

        EnsureWritingPossible();

        if (_fileExtensions!.Count == 0)
        {
            _fileExtensions.Add("txt");
        }
    }

    public void ShowConfirmation()
    {
        _inited.Throw().IfFalse();

        StringBuilder fileExtensions = new();
        fileExtensions.Append('[');

        for (int i = 0; i < _fileExtensions!.Count - 1; i++)
        {
            fileExtensions.Append(_fileExtensions[i]);
            fileExtensions.Append(", ");
        }

        fileExtensions.Append(_fileExtensions[^1]);
        fileExtensions.Append(']');

        Console.WriteLine($"Files {fileExtensions} in folder '{_folder!.FullName}' will be modified. Press enter to continue...");
        Console.ReadLine();
        Console.Clear();
    }

    public void CreateTempFolder()
    {
        _inited.Throw().IfFalse();

        _tempFolder = new DirectoryInfo(GetUniquePathOrName(_folder!));

        _tempFolder.Create();
    }

    public IEnumerable<string> GetFilePaths()
    {
        _inited.Throw().IfFalse();

        IEnumerable<string>? filePaths = default;

        foreach (string fileExtension in _fileExtensions!)
        {
            IEnumerable<string> currentEnumeration = Directory
                .EnumerateFiles(
                    _folder!.FullName,
                    $"*.{fileExtension}",
                    _searchOption);

            filePaths = filePaths != null
                ? filePaths.Concat(currentEnumeration)
                : currentEnumeration;
        }

        return filePaths!;
    }

    public FileInfo GetTempFile()
    {
        _inited.Throw().IfFalse();

        string fileName;

        do
        {
            fileName = GetUniquePathOrName(_tempFolder!, path: false);
        }
        while (_tempFileNames.Contains(fileName));

        // запись в словарь гораздо быстрее создания файла, а при
        // многопотоке теоретически возможна ситуация, когда нескольким
        // потокам отдаётся одинаковое имя файла - тогда последний поток
        // перезапишет данные предыдущих потоков, работавших с файлом;
        // либо будет исключение, т.к. предыдущий поток ещё не освободил файл
        _tempFileNames.Add(fileName);

        return new(Path.Combine(_tempFolder!.FullName, fileName));
    }

    public void WriteTempFile(
        FileInfo file, IEnumerable<string> lines)
    {
        _inited.Throw().IfFalse();

        using var writer = new StreamWriter(
            Path.Combine(_tempFolder!.FullName, file.Name),
            append: false,
            Encoding.UTF8);

        foreach (string line in lines)
        {
            try
            {
                writer.WriteLine(line);
            }
            catch (IOException ex)
            {
                writer.Dispose();
                Cleanup();

                Console.WriteLine(ex.Message);
                throw;
            }
        }

        _tempFileNames.Remove(file.Name);
    }

    public void Cleanup()
    {
        _inited.Throw().IfFalse();

        _tempFolder!.Delete(recursive: true);
    }

    private void EnsureWritingPossible()
    {
        FileInfo file = new(GetUniquePathOrName(_folder!));

        try
        {
            using (FileStream _ = file.Create())
            {
            }

            file.Delete();
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }

        DirectoryInfo folder = new(GetUniquePathOrName(_folder!));

        try
        {
            folder.Create();
            folder.Delete();
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    private static string GetUniquePathOrName(
        DirectoryInfo directory, bool path = true)
    {
        string uniqueName;
        string uniquePath;

        do
        {
            uniqueName = Path.GetRandomFileName();

            uniquePath = Path.Combine(directory.FullName, uniqueName);
        }
        while (File.Exists(uniquePath) || Directory.Exists(uniquePath));

        return path ? uniquePath : uniqueName;
    }
}
