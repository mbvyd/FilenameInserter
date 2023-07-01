using System.Collections.Generic;
using Throw;

namespace FilenameInserter;

internal class TextProcessor
{
    private delegate string LineModifier(string line, string addition);

    private string? _delimiter;
    private LineModifier? _lineModifier;

    private bool _inited;

    public void Init(Mode mode, Options options)
    {
        _lineModifier = mode switch
        {
            Mode.Append => ModifyLineAppend,
            Mode.Prepend => ModifyLinePrepend,
            _ => ModifyLineAppend,
        };

        _delimiter = options.Delimiter;
        _inited = true;
    }

    public IEnumerable<string> UpdateLines(
        IEnumerable<string> lines, string fileName)
    {
        _inited.Throw().IfFalse();

        foreach (string line in lines)
        {
            yield return _lineModifier!(line, fileName);
        }
    }

    private string ModifyLineAppend(string line, string addition)
    {
        return $"{line}{_delimiter}{addition}";
    }

    private string ModifyLinePrepend(string line, string addition)
    {
        return $"{addition}{_delimiter}{line}";
    }
}
