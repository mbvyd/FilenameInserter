using System.Collections.Generic;
using System.IO;
using Throw;

namespace FilenameInserter;

internal class FilenameInserter
{
    public enum Mode
    {
        Append,
        Prepend,
    }

    private readonly FileProcessor _fileProcessor;

    private string? _delimiter;
    private bool _silent;

    private Mode _mode;

    private bool _inited;

    public FilenameInserter(FileProcessor fileProcessor)
    {
        _fileProcessor = fileProcessor;
    }

    public void Init(Mode mode, FileInserterOptions options)
    {
        _mode = mode;

        _delimiter = options.Delimiter;
        _silent = options.Silent;

        _fileProcessor.Init(options);
        _fileProcessor.Validate();

        _inited = true;
    }

    public void Process()
    {
        _inited.Throw().IfFalse();

        if (!_silent)
        {
            _fileProcessor.ShowConfirmation();
        }

        _fileProcessor.CreateTempFolder();

        LineModifier lineModifier = _mode switch
        {
            Mode.Append => ModifyLineAppend,
            Mode.Prepend => ModifyLinePrepend,
            _ => ModifyLineAppend,
        };

        IEnumerable<string> filePaths = _fileProcessor.GetFilePaths();

        ModifyFiles(filePaths, lineModifier);

        _fileProcessor.Cleanup();
    }

    private void ModifyFiles(
        IEnumerable<string> filePaths, LineModifier lineModifier)
    {
        foreach (string path in filePaths)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);

            IEnumerable<string> lines = UpdateLines(
                File.ReadLines(path), lineModifier, fileName);

            // нельзя записывать в состояние объекта - чтобы использовать
            // неограниченное число временных файлов при многопотоке
            FileInfo tempFile = _fileProcessor.GetTempFile();

            _fileProcessor.WriteTempFile(tempFile, lines);

            FileProcessor.OverwriteOriginal(tempFile, path);
        }
    }

    private static IEnumerable<string> UpdateLines(
        IEnumerable<string> lines,
        LineModifier lineModifier,
        string fileName)
    {
        foreach (string line in lines)
        {
            yield return lineModifier(line, fileName);
        }
    }

    private delegate string LineModifier(string line, string addition);

    private string ModifyLineAppend(string line, string addition)
    {
        return $"{line}{_delimiter}{addition}";
    }

    private string ModifyLinePrepend(string line, string addition)
    {
        return $"{addition}{_delimiter}{line}";
    }
}