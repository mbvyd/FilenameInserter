using System;
using System.IO;

namespace FilenameInserter;

internal class TextProcessor
{
    public bool Inited { get; private set; } = false;

    private delegate void SpanProcessor(
        string line, string addition, StreamWriter writer);

    private delegate string LineModifier(string line, string addition);

    private string? _delimiter;

    private SpanProcessor? _spanProcessor;
    private LineModifier? _lineModifier;

    public void Init(Mode mode, string delimiter)
    {
        _spanProcessor = mode switch
        {
            Mode.Append => ProcessSpanAppend,
            Mode.Prepend => ProcessSpanPrepend,
            _ => ProcessSpanAppend,
        };

        _lineModifier = mode switch
        {
            Mode.Append => ModifyLineAppend,
            Mode.Prepend => ModifyLinePrepend,
            _ => ModifyLineAppend,
        };

        _delimiter = delimiter;
        Inited = true;
    }

    public void ModifyWriteLine(
        string line, string filePath, StreamWriter writer)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);

        bool failedWithSpan = false;

        try
        {
            _spanProcessor!(line: line, addition: fileName, writer);
        }
        // if stackalloc failed to allocate memory
        catch (InsufficientExecutionStackException)
        {
            failedWithSpan = true;
        }
        // if some other problem with stack because of allocation
        catch (StackOverflowException)
        {
            failedWithSpan = true;
        }
        // if writing position in span is incorrect
        catch (ArgumentOutOfRangeException)
        {
            failedWithSpan = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }

        if (failedWithSpan)
        {
            try
            {
                writer.WriteLine(
                    _lineModifier!(line: line, addition: fileName));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }

    private void ProcessSpanAppend(
        string line, string addition, StreamWriter writer)
    {
        int length = GetSpanLength(line: line, addition: addition);

        (int writeIndex2, int writeIndex3) = GetSpanWriteIdx(line);

        Span<char> span = stackalloc char[length];

        line.TryCopyTo(span);
        _delimiter!.TryCopyTo(span[writeIndex2..]);
        addition.TryCopyTo(span[writeIndex3..]);

        writer.WriteLine(span);
    }

    private void ProcessSpanPrepend(
        string line, string addition, StreamWriter writer)
    {
        int length = GetSpanLength(line: line, addition: addition);

        (int writeIndex2, int writeIndex3) = GetSpanWriteIdx(addition);

        Span<char> span = stackalloc char[length];

        addition.TryCopyTo(span);
        _delimiter!.TryCopyTo(span[writeIndex2..]);
        line.TryCopyTo(span[writeIndex3..]);

        writer.WriteLine(span);
    }

    private int GetSpanLength(string line, string addition)
    {
        return line.Length + addition.Length + _delimiter!.Length;
    }

    private (int WriteIndex2, int WriteIndex3) GetSpanWriteIdx(
        string firstFragment)
    {
        int writeIndex2 = firstFragment.Length;
        int writeIndex3 = firstFragment.Length + _delimiter!.Length;

        return (writeIndex2, writeIndex3);
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
