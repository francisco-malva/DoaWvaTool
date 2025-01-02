using DoaWVATool;

if (args.Length < 1)
{
    PrintUsageAndDie();
}



try
{
    switch (args[0].Trim().ToLower())
    {
        case "pack":
            PackWva(args);
            break;
        case "unpack":
            UnpackWva(args);
            break;
        default:
            PrintUsageAndDie();
            break;
    }
}
catch (Exception e)
{
    await Console.Error.WriteLineAsync(e.Message);
}

return;


void PrintUsageAndDie()
{
    Console.WriteLine("Usage: DoaWVATool <mode> <input> <output>");
    Console.WriteLine();
    Console.WriteLine("Modes:");
    Console.WriteLine("  pack     - Creates a WVA file from the specified manifest file.");
    Console.WriteLine("             Syntax: DoaWVATool pack <manifest_path> <output_wva_file>");
    Console.WriteLine("             Example: DoaWVATool pack manifest.json output.wva");
    Console.WriteLine();
    Console.WriteLine("  unpack   - Extracts files from a WVA archive to a specified directory and creates an accompanying JSON manifest file.");
    Console.WriteLine("             Syntax: DoaWVATool unpack <input_wva_file> <output_directory>");
    Console.WriteLine("             Example: DoaWVATool unpack input.wva extracted_files/");
    Console.WriteLine();
    Console.WriteLine("Arguments:");
    Console.WriteLine("  <mode>            - The operation mode, either 'pack' or 'unpack'.");
    Console.WriteLine("  <input>           - The input file or manifest (for 'pack') or the WVA file to unpack.");
    Console.WriteLine("  <output>          - The output file (for 'pack') or the directory (for 'unpack').");
    Console.WriteLine();
    Console.WriteLine("Error: Invalid or insufficient arguments provided.");
    Environment.Exit(1);
}

void UnpackWva(IReadOnlyList<string> args)
{
    if (args.Count < 3)
    {
        PrintUsageAndDie();
    }

    Console.WriteLine($"Unpacking {args[1]} into {args[2]}...");
    var file = WvaFile.FromFile(args[1]);
    file.UnpackToDirectory(args[2]);
}

void PackWva(IReadOnlyList<string> args)
{
    if (args.Count < 3)
    {
        PrintUsageAndDie();
    }

    Console.WriteLine($"Packing {args[1]} into {args[2]}...");
    var file = WvaFile.FromManifest(args[1]);

    file.PackToFile(args[2]);
    Console.WriteLine("Packing completed successfully!");
}
