using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace FilenameInserter;

internal class Program
{
    private static readonly Container _container;

    static Program()
    {
        _container = new();

        _container.Options.DefaultScopedLifestyle =
            new AsyncScopedLifestyle();

        _container.Register<FilenameInserter>();
        _container.Register<FileProcessor>();
        _container.Register<TextProcessor>();

        _container.Verify();
    }

    private static void Main(string[] args)
    {
        Option<DirectoryInfo> folderOption = new("--folder");
        folderOption.AddAlias("-f");

        Option<string> delimiterOption = new("--delimiter");
        delimiterOption.AddAlias("-d");

        Option<bool> silentOption = new("--silent");
        silentOption.AddAlias("-s");

        Option<bool> recursiveOption = new("--recursive");
        recursiveOption.AddAlias("-r");

        Option<List<string>> fileExtensionsOption = new("--extensions");
        fileExtensionsOption.AddAlias("-e");

        FinalizeOptions();

        var appendCommand = new Command(
            name: "append",
            description: "Append file name to the end of each string in a file.");

        var prependCommand = new Command(
            name: "prepend",
            description: "Add filename to the start of each string in a file.");

        AddOptionsToCommands();

        SetHandler(appendCommand, Mode.Append);
        SetHandler(prependCommand, Mode.Prepend);

        var rootCommand = new RootCommand
        {
            TreatUnmatchedTokensAsErrors = true,
            Description = "Filename inserter. Get files in folder and for each file insert its filename to each string.",
        };

        rootCommand.AddCommand(appendCommand);
        rootCommand.AddCommand(prependCommand);

        rootCommand.Invoke(args);

        void FinalizeOptions()
        {
            folderOption.Description = "Path to folder with files to be modified.";
            folderOption.IsRequired = true;

            delimiterOption.Description = "Delimiter character or sequence of characters which is used to glue filename to strings in file.";
            delimiterOption.IsRequired = true;

            silentOption.Description = "Do not ask confirmation from user before modifying files.";

            recursiveOption.Description = "Process files not only in a folder, but also in all of its subfolders.";

            fileExtensionsOption.Description = "File extensions to work with, delimited by space. E.g.: txt csv. By default: txt.";
            fileExtensionsOption.Arity = ArgumentArity.ZeroOrMore;
            fileExtensionsOption.AllowMultipleArgumentsPerToken = true;
        }

        void AddOptionsToCommands()
        {
            appendCommand.AddOption(folderOption);
            appendCommand.AddOption(delimiterOption);
            appendCommand.AddOption(silentOption);
            appendCommand.AddOption(recursiveOption);
            appendCommand.AddOption(fileExtensionsOption);

            prependCommand.AddOption(folderOption);
            prependCommand.AddOption(delimiterOption);
            prependCommand.AddOption(silentOption);
            prependCommand.AddOption(recursiveOption);
            prependCommand.AddOption(fileExtensionsOption);
        }

        void SetHandler(Command command, Mode mode)
        {
            command.SetHandler(
                (folder,
                delimiter,
                silent,
                recursive,
                fileExtensions) =>
                {
                    var options = new Options
                    {
                        DirectoryInfo = folder,
                        Recursive = recursive,
                        FileExtensions = fileExtensions,
                        Silent = silent,
                        Delimiter = delimiter,
                    };

                    FilenameInserter engine = _container
                        .GetInstance<FilenameInserter>();

                    engine.Init(mode, options);
                    engine.Process();
                },
                folderOption,
                delimiterOption,
                silentOption,
                recursiveOption,
                fileExtensionsOption);
        }
    }
}