
namespace LethalMacro
{
    public interface IKeyLogExecuter
    {
        Task<MacroBind> CreateNewKeybindAsync();
        void StopRecording();
        void StartRecording(IEnumerable<MacroBind> binds);
    }
}