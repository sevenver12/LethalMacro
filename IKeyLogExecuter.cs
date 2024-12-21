
namespace LethalMacro
{
    public interface IKeyLogExecuter
    {
        Task<MacroBind> CreateNewKeybindAsync();
        void Stop();
        Task WaitForKeyAsync(IEnumerable<MacroBind> binds, CancellationToken token);
    }
}