using DoaWVATool.Wva;

if (args.Length < 1)
{
    PrintUsageAndDie();
}



try
{

    var fileInfo = new FileInfo(args[0]);

    switch (fileInfo.Extension)
    {
        case ".json":
            PackWva(args);
            break;
        case ".wva":
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
    Console.WriteLine("Usage: DoaWVATool <input> [output]");
    Console.WriteLine();
    Console.WriteLine("Description:");
    Console.WriteLine("  This tool packs a JSON manifest into a WVA file or unpacks a WVA file to a directory.");
    Console.WriteLine();
    Console.WriteLine("Input File Types:");
    Console.WriteLine("  .json  - Packs the manifest file into a WVA file.");
    Console.WriteLine("           Example: DoaWVATool manifest.json [output.wva]");
    Console.WriteLine();
    Console.WriteLine("  .wva   - Unpacks the WVA file to the specified directory.");
    Console.WriteLine("           Example: DoaWVATool input.wva [extracted_files/]");
    Console.WriteLine();
    Console.WriteLine("Arguments:");
    Console.WriteLine("  <input>   - The input file (JSON manifest for packing or WVA file for unpacking).");
    Console.WriteLine("  [output]  - Optional. The output file path for packing or the directory for unpacking.");
    Console.WriteLine("              Defaults to:");
    Console.WriteLine("                - For packing: <input_name>_packed.wva");
    Console.WriteLine("                - For unpacking: A folder named after the input file.");
    Console.WriteLine();
    Console.WriteLine("Error: Invalid or insufficient arguments provided.");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    Environment.Exit(1);
}


void UnpackWva(IReadOnlyList<string> args)
{
    var fileInfo = new FileInfo(args[0]);
    var fileName = (fileInfo.Name).Replace(fileInfo.Extension, string.Empty);

    var dir = args.Count < 2 ? $"./{fileName}" : args[1];

    Console.WriteLine($"Unpacking {args[0]} into {dir}...");
    var file = WvaFile.FromFile(args[0]);
    file.UnpackToDirectory(dir);
}

void PackWva(IReadOnlyList<string> args)
{
    var fileInfo = new FileInfo(args[0]);
    var outFilePath = args.Count < 2
        ? $"./{(fileInfo.Name)
            .Replace(fileInfo.Extension, ".wva")
            .Replace("_manifest", "_packed")}"
        : args[1];


    Console.WriteLine($"Packing {args[0]} into {outFilePath}...");
    var file = WvaFile.FromManifest(args[0]);

    file.PackToFile(outFilePath);
    Console.WriteLine("Packing completed successfully!");
}
