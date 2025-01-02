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
    Console.WriteLine("Usage: DoaWVATool mode");
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
