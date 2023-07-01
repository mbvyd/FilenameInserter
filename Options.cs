using System.Collections.Generic;
using System.IO;

namespace FilenameInserter;

internal class Options
{
    public required DirectoryInfo DirectoryInfo { get; set; }
    public required string Delimiter { get; set; }
    public required bool Silent { get; set; }
    public required bool Recursive { get; set; }
    public required List<string> FileExtensions { get; set; }
}