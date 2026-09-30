using RBException;

string FormatAddressWithSymbol(uint address)
{
    string outstr = $"0x{address:X8}";
    SymbolInfo? info;
    int offset;
    if (SymbolTable.FindSymbolAtAddress(address, out info, out offset))
        outstr += $": {info!.FunctionName} +0x{offset:X} ({info!.ModuleName})";
    else
        outstr += " (unknown)";
    return outstr;
}

if (args.Length < 1)
{
    Console.WriteLine("No exception file provided");
    return;
}

// TODO: better code for handling version/platform
FileStream fs = File.OpenRead(args[0]);
ExceptionRB3E_Xbox exc = ExceptionRB3E_Xbox.LoadExceptionFile(fs);
fs.Close();

// read symbol tables
string gameSymbols = "rb3_xbox_tu5.csv";
string kernelSymbols = "xboxkrnl_17559.csv"; //just assume anyone is on 17559, change this if we include kernel version
if (File.Exists(gameSymbols))
    SymbolTable.LoadSymbols("rb3_xbox_tu5.xex", File.ReadAllLines(gameSymbols));
if (File.Exists(kernelSymbols))
    SymbolTable.LoadSymbols("xboxkrnl_17559.exe", File.ReadAllLines(kernelSymbols));
// this takes in user input so be a bit more careful
string? modSymbols = null;
if (exc.Header.Commit.IndexOf(".:<>\"|?*/\\\0") == -1 && !exc.Header.Commit.EndsWith("-dirty"))
    modSymbols = $"rb3e_syms_{exc.Header.Commit}.csv";
if (modSymbols != null && File.Exists(modSymbols))
    SymbolTable.LoadSymbols($"RB3Enhanced_{exc.Header.Commit}.dll", File.ReadAllLines(modSymbols));

Console.WriteLine($"RB3Enhanced (Xbox) {exc.Header.BuildTag} ({exc.Header.Commit})");
// TODO: parse the specific NT exception code type and display more information
Console.WriteLine($"Exception: 0x{exc.ExceptionRecord.ExceptionCode:X8} @ {FormatAddressWithSymbol(exc.ExceptionRecord.ExceptionAddress)}");
Console.WriteLine();
Console.WriteLine("Stack Walk:");
Console.WriteLine("================================================================================");
for (int i = 0; i < exc.StackWalk.Length; i++)
    Console.WriteLine($"- {FormatAddressWithSymbol(exc.StackWalk[i])}");
Console.WriteLine();
Console.WriteLine("Thread Context:");
Console.WriteLine("================================================================================");
Console.WriteLine($"MSR: 0x{exc.Context.Msr:X8} IAR: 0x{exc.Context.Iar:X8} LR: 0x{exc.Context.Lr:X8} CTR: 0x{exc.Context.Ctr:X16}");
for (int i = 0; i < exc.Context.Gpr.Length; i++)
{
    Console.Write($"GPR{i:00}: 0x{exc.Context.Gpr[i]:X16} ");
    if ((i % 3) == 2)
        Console.WriteLine();
}
Console.WriteLine();
Console.WriteLine();
Console.WriteLine("Memory Chunks:");
Console.WriteLine("================================================================================");
for (int i = 0; i < exc.MemChunks.Length; i++)
{
    Console.Write($"0x{exc.MemChunks[i].Address:X8} (0x{exc.MemChunks[i].Length:X}) ");
    if ((i % 4) == 3)
        Console.WriteLine();
    // because of a bug in RB3Enhanced's memory chunk writing code, 
    // sometimes this will just be all zeroes and no future chunks will be on-disk
    // lawl fuck
    if (exc.MemChunks[i].Address == 0)
        break;
    // TODO: some better way to view information in said memory chunks
}
Console.WriteLine();
Console.WriteLine();
