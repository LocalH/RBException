using System.Diagnostics.Contracts;
using System.Text;

namespace RBException.Types;

public class ExceptionHeaderRB3E
{
    public uint Magic;
    public int Version;

    public ExceptionHeaderRB3E(Stream s)
    {
        Magic = s.ReadUInt32BE();
        Version = s.ReadInt32BE();
    }
}

public class ExceptionHeaderRB3E_v0 : ExceptionHeaderRB3E
{
    public string BuildTag;
    public string Commit;
    public ushort NumStackwalks;
    public ushort NumMemchunks;

    public ExceptionHeaderRB3E_v0(Stream s) : base(s)
    {
        byte[] tagBytes = s.ReadBytes(48);
        BuildTag = Encoding.UTF8.GetString(tagBytes).TrimEnd('\0');
        byte[] commitBytes = s.ReadBytes(48);
        Commit = Encoding.UTF8.GetString(commitBytes).TrimEnd('\0');
        NumStackwalks = s.ReadUInt16BE();
        NumMemchunks = s.ReadUInt16BE();
    }
}

public class XeExceptionRecord
{
    private static int EXCEPTION_MAXIMUM_PARAMETERS = 15;

    public uint ExceptionCode;
    public uint ExceptionFlags;
    public uint ExceptionRecordPtr;
    public uint ExceptionAddress;
    public uint NumberParameters;
    public uint[] ExceptionInformation;

    public XeExceptionRecord(Stream s)
    {
        ExceptionCode = s.ReadUInt32BE();
        ExceptionFlags = s.ReadUInt32BE();
        ExceptionRecordPtr = s.ReadUInt32BE();
        ExceptionAddress = s.ReadUInt32BE();
        NumberParameters = s.ReadUInt32BE();
        ExceptionInformation = new uint[EXCEPTION_MAXIMUM_PARAMETERS];
        for (int i = 0; i < EXCEPTION_MAXIMUM_PARAMETERS; i++)
            ExceptionInformation[i] = s.ReadUInt32BE();
    }
}

public class XeContext
{
    private static int NumGpr = 32;
    private static int NumFpr = 32;
    private static int NumVr = 128;

    public uint ContextFlags;
    public uint Msr;
    public uint Iar;
    public uint Lr;
    public ulong Ctr;
    public ulong[] Gpr;
    public uint Cr;
    public uint Xer;
    public double Fpscr;
    public double[] Fpr;
    public uint UserModeControl;
    public uint Fill;
    // public float[] Vscr; // length 4
    // public float[][] Vr; // length NumVr*4

    public XeContext(Stream s)
    {
        long a = s.Position;
        ContextFlags = s.ReadUInt32BE();
        Msr = s.ReadUInt32BE();
        Iar = s.ReadUInt32BE();
        Lr = s.ReadUInt32BE();
        Ctr = s.ReadUInt64BE();
        Gpr = new ulong[NumGpr];
        for (int i = 0; i < NumGpr; i++)
            Gpr[i] = s.ReadUInt64BE();
        Cr = s.ReadUInt32BE();
        Xer = s.ReadUInt32BE();
        Fpscr = s.ReadDoubleBE();
        Fpr = new double[NumFpr];
        for (int i = 0; i < NumFpr; i++)
            Fpr[i] = s.ReadDoubleBE();
        UserModeControl = s.ReadUInt32BE();
        Fill = s.ReadUInt32BE();
        // ew. VMX128 registers. not currently included by RB3E in exceptions!
        // Vscr = new float[4];
        // for (int i = 0; i < 4; i++)
        //     Vscr[i] = s.ReadFloatBE();
        // Vr = new float[NumVr][];
        // for (int i = 0; i < NumVr; i++)
        // {
        //     Vr[i] = new float[4];
        //     for (int j = 0; j < 4; j++)
        //         Vr[i][j] = s.ReadFloatBE();
        // }
    }
}

public class MemChunkRB3E
{
    public uint Address;
    public int Length;
    public byte[] Data;

    public MemChunkRB3E(Stream s)
    {
        Address = s.ReadUInt32BE();
        Length = s.ReadInt32BE();
        Data = s.ReadBytes(Length);
    }
}
