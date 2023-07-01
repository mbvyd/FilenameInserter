using System.Collections.Generic;
using System.IO;
using Throw;

namespace FilenameInserter;

internal class FilenameInserter
{
    private readonly FileProcessor _fileProcessor;
    private readonly TextProcessor _textProcessor;

    private bool _silent;
    private bool _inited;

    public FilenameInserter(
        FileProcessor fileProcessor, TextProcessor textProcessor)
    {
        _fileProcessor = fileProcessor;
        _textProcessor = textProcessor;
    }

    public void Init(Mode mode, Options options)
    {
        _silent = options.Silent;

        _fileProcessor.Init(options);
        _fileProcessor.Validate();

        _textProcessor.Init(mode, options);

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

        ModifyFiles();

        _fileProcessor.Cleanup();
    }

    private void ModifyFiles()
    {
        IEnumerable<string> filePaths = _fileProcessor.GetFilePaths();

        foreach (string path in filePaths)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);

            IEnumerable<string> lines = _textProcessor.UpdateLines(
                File.ReadLines(path), fileName);

            // нельзя записывать в состояние объекта - чтобы использовать
            // неограниченное число временных файлов при многопотоке
            FileInfo tempFile = _fileProcessor.GetTempFile();

            _fileProcessor.WriteTempFile(tempFile, lines);

            FileProcessor.OverwriteOriginal(tempFile, path);
        }
    }
}
