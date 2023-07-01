using System.Collections.Generic;
using System.IO;

namespace FilenameInserter;

internal class FileTraverserOptions
{
    public required DirectoryInfo DirectoryInfo { get; set; }
    public required string Delimiter { get; set; }
    public required bool Silent { get; set; }
    public required bool Recursive { get; set; }
    public required List<string> FileExtensions { get; set; }

    public void Validate()
    {
        if (!DirectoryInfo.Exists)
        {
            throw new DirectoryNotFoundException();
        }

        if (FileExtensions.Count == 0)
        {
            FileExtensions.Add("txt");
        }
    }
}