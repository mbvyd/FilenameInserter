namespace FilenameInserter;

internal class Program
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0042:Deconstruct variable declaration", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0008:Use explicit type", Justification = "<Pending>")]
    private static void Main(string[] args)
    {
        var tuple = ArgsDisassembler.Disassemble(args);

        var engine = new FileTraverser(
            tuple.Folder,
            tuple.Separator,
            tuple.Append,
            tuple.Recursive,
            tuple.FileExtensions);

        engine.Process();
    }
}