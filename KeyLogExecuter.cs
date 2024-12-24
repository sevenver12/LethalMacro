using SharpHook;
using SharpHook.Native;
using SharpHook.Providers;
namespace LethalMacro
{
    public class KeyLogExecuter : IKeyLogExecuter, IDisposable
    {
        private Dictionary<(ModifierMask, KeyCode), string>? _keybinds;
        private readonly TaskPoolGlobalHook _hook;

        public KeyLogExecuter()
        {
            _hook = new TaskPoolGlobalHook(1, GlobalHookType.Keyboard);
            _hook.RunAsync();
        }
        public async Task<MacroBind> CreateNewKeybindAsync()
        {
            var tSource = new TaskCompletionSource<MacroBind>();
            void valueFunc(object? _, KeyboardHookEventArgs e)
            {
                tSource.SetResult(new MacroBind
                {
                    KeyCode = e.Data.KeyCode,
                    Modifier = e.RawEvent.Mask,
                    Value = string.Empty,
                });
                e.SuppressEvent = true;
            }

            _hook.KeyTyped += valueFunc;
            var result = await tSource.Task;
            _hook.KeyTyped -= valueFunc;
            return result;
        }

        public void StartRecording(IEnumerable<MacroBind> binds)
        {
            _keybinds = binds.ToDictionary(k => (k.Modifier, k.KeyCode), v => v.Value);
            _hook.KeyTyped += HandleKeyTyped;
        }

        public void StopRecording()
        {
            _hook.KeyTyped -= HandleKeyTyped;
        }

        private void HandleKeyTyped(object? sender, KeyboardHookEventArgs e)
        {
            if (!e.IsEventSimulated && _keybinds!.TryGetValue((e.RawEvent.Mask, e.Data.KeyCode), out var toType))
            {
                var sm = new EventSimulator();

                if (e.RawEvent.Mask.HasCtrl())
                    sm.SimulateKeyRelease(KeyCode.VcLeftControl);
                if (e.RawEvent.Mask.HasAlt())
                    sm.SimulateKeyRelease(KeyCode.VcLeftAlt);
                if (e.RawEvent.Mask.HasShift())
                    sm.SimulateKeyRelease(KeyCode.VcLeftShift);

                sm.SimulateTextEntry(toType);
                sm.SimulateKeyPress(KeyCode.VcEnter);
                sm.SimulateKeyRelease(KeyCode.VcEnter);

                if (e.RawEvent.Mask.HasCtrl())
                    sm.SimulateKeyPress(KeyCode.VcLeftControl);
                if (e.RawEvent.Mask.HasAlt())
                    sm.SimulateKeyPress(KeyCode.VcLeftAlt);
                if (e.RawEvent.Mask.HasShift())
                    sm.SimulateKeyPress(KeyCode.VcLeftShift);
                e.SuppressEvent = true;
            }
        }

        public void Dispose()
        {
            _hook.Dispose();
            UioHookProvider.Instance.Stop();
            GC.SuppressFinalize(this);
        }
    }
}
