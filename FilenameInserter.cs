using System.Collections.Generic;
using System.Threading.Tasks;
using Throw;

namespace FilenameInserter;

internal class FilenameInserter
{
    private readonly FileProcessor _fileProcessor;
    private readonly TextProcessor _textProcessor;

    private bool _silent;
    private bool _inited = false;

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

        _textProcessor.Init(mode, options.Delimiter);

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

        IEnumerable<string> filePaths = _fileProcessor.GetFilePaths();

        Parallel.ForEach(filePaths, path =>
            _fileProcessor.ModifyFile(path, _textProcessor));

        _fileProcessor.Cleanup();
    }
}
