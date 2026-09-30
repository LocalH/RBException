using RBException.Types;
namespace RBException;

// Currently, RB3Enhanced only supports generating exception report files
// on Xbox 360, so it doesn't matter that this is as hardcoded as it is.
// We have room for future expansion; for when there is a new version of
// the exception handler or when we support da wee or da ps triple.
public class ExceptionRB3E_Xbox
{
#pragma warning disable CS8618
    public ExceptionHeaderRB3E_v0 Header;
    public XeExceptionRecord ExceptionRecord;
    public XeContext Context;
    public uint[] StackWalk;
    public MemChunkRB3E[] MemChunks;
#pragma warning restore CS8618

    public static ExceptionRB3E_Xbox LoadExceptionFile(Stream s)
    {
        ExceptionRB3E_Xbox exc = new();
        exc.Header = new(s);
        if (exc.Header.Magic != 0x33455858)
            throw new Exception("Input file not a valid Xbox 360 RB3Enhanced exception.");
        if (exc.Header.Version != 0)
            throw new Exception("Unsupported exception version");
        exc.ExceptionRecord = new(s);
        exc.Context = new(s);
        exc.StackWalk = new uint[exc.Header.NumStackwalks];
        for (int i = 0; i < exc.Header.NumStackwalks; i++)
            exc.StackWalk[i] = s.ReadUInt32BE();
        exc.MemChunks = new MemChunkRB3E[exc.Header.NumMemchunks];
        for (int i = 0; i < exc.Header.NumMemchunks; i++)
            exc.MemChunks[i] = new(s);

        return exc;
    }
}
