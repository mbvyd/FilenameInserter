using System;
using System.IO;

namespace FilenameInserter;

internal static class ArgsDisassembler
{
    public static (
        string Folder,
        string Separator,
        bool Append,
        bool Recursive,
        string FileExtensions)
        Disassemble(string[] args)
    {
        if (args.Length is < 3 or > 5)
        {
            throw new ArgumentException("Invalid number of arguments - must be 2 to 4 arguments - path to folder with files, separator, file extensions (optional), recursive (optional)");
        }

        ReadOnlySpan<char> path = GetStrippedParam(
            args[0], "path", "p");

        if (!Directory.Exists(path.ToString()))
        {
            throw new DirectoryNotFoundException();
        }

        ReadOnlySpan<char> separator = GetStrippedParam(
            args[1], "separator", "s");

        // append - дописать к концу строки;
        // иначе prepend (добавить в начале строки)
        ReadOnlySpan<char> append = GetStrippedParam(
            args[2], "append", "a");

        bool isAppend = IsTrue(append);

        bool isRecursive = false;

        if (args.Length > 3)
        {
            ReadOnlySpan<char> recursive = GetStrippedParam(
                args[3], "recursive", "r");

            isRecursive = IsTrue(recursive);
        }

        string fileExtensions = "*.txt";

        if (args.Length > 4)
        {
            ReadOnlySpan<char> extensionsUser = GetStrippedParam(
                args[4], "extensions", "e");

            fileExtensions = extensionsUser.ToString();
        }

        return (
            path.ToString(),
            separator.ToString(),
            isAppend,
            isRecursive,
            fileExtensions);
    }

    private static ReadOnlySpan<char> GetStrippedParam(
        string arg, string aliasLong, string aliasShort)
    {
        int start = 0;

        if (arg.StartsWith($"--{aliasLong}="))
        {
            // 3: two dashes and equals, e.g. --param=
            start = aliasLong.Length + 3;
        }
        else if (arg.StartsWith($"-{aliasShort}="))
        {
            // 2: one dash and equals, e.g. -p=
            start = aliasShort.Length + 2;
        }

        return arg.AsSpan()[start..arg.Length];
    }

    private static bool IsTrue(ReadOnlySpan<char> arg)
    {
        return arg is "true" or "1";
    }
}