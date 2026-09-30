using System.Diagnostics.Contracts;
using System.Globalization;

namespace RBException;

public class SymbolInfo
{
    public uint StartAddress;
    public uint EndAddress;
    public string FunctionName;
    public string ModuleName;

    public SymbolInfo(uint startAddress, uint endAddress, string functionName, string moduleName)
    {
        StartAddress = startAddress;
        EndAddress = endAddress;
        FunctionName = functionName;
        ModuleName = moduleName;
    }
}

public class SymbolTable
{
    public static Dictionary<uint, SymbolInfo> symbols = new();

    public static void LoadSymbols(string module, string[] lines)
    {
        foreach(string line in lines)
        {
            string[] split = line.Split(",");
            // don't parse it if it's the first line of a
            if (split[0].ToLower() == "start")
                continue;
            // TODO: make this parser safe and not throw exceptions for fun
            if (split[0].StartsWith("0x")) split[0] = split[0].Replace("0x", "");
            if (split[1].StartsWith("0x")) split[1] = split[1].Replace("0x", "");
            uint start = uint.Parse(split[0], NumberStyles.HexNumber);
            uint end = uint.Parse(split[1], NumberStyles.HexNumber);
            string name = split[2];
            symbols[start] = new SymbolInfo(start, end, name, module);
        }
    }

    public static bool FindSymbolAtAddress(uint instructionAddress, out SymbolInfo? info, out int offset)
    {
        foreach(uint key in symbols.Keys)
        {
            // if the key is after the instruction in memory then it doesn't matter
            if (instructionAddress < key)
                continue;
            // get the symbol info
            SymbolInfo si = symbols[key];
            // if the instruction lands between the start and end then this is probably the symbol
            if (instructionAddress >= si.StartAddress && instructionAddress <= si.EndAddress)
            {
                info = si;
                offset = (int)(instructionAddress - si.StartAddress);
                return true;
            }
        }
        info = null;
        offset = 0;
        return false; // not found one :(
    }
}