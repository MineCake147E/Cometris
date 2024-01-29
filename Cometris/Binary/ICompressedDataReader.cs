namespace Cometris.Binary
{
    public interface ICompressedDataReader<T, TCompressedSegment, TState>
    {
        static abstract TState Advance(TState state, nint count);
    }
}
