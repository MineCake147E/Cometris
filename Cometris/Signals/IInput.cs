namespace Cometris.Signals
{
    public interface IInput<TInput> where TInput : unmanaged
    {
        TInput Status { get; set; }
    }
}
