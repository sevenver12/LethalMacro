using SharpHook;
using SharpHook.Native;
namespace LethalMacro
{
    public class KeyLogExecuter : IKeyLogExecuter
    {
        private Dictionary<(ModifierMask, KeyCode), string>? _keybinds;
        private TaskPoolGlobalHook? _hook;

        public async Task<MacroBind> CreateNewKeybindAsync()
        {
            var tSource = new TaskCompletionSource<MacroBind>();
            var hook = new TaskPoolGlobalHook(1, GlobalHookType.Keyboard);
            hook.KeyTyped += (_, e) =>
            {
                tSource.SetResult(new MacroBind
                {
                    KeyCode = e.Data.KeyCode,
                    Modifier = e.RawEvent.Mask,
                    Value = string.Empty,
                });
            };
            var hookRunner = hook.RunAsync();
            //var result = await hook.EventAsync<KeyboardHookEventArgs>(()=> hook.RunAsync(),nameof(hook.KeyTyped));
            //hook.Dispose();
            var result = await tSource.Task;
            
            //hook.Dispose();
            //await hookRunner;
            return result;
        }

        public async Task WaitForKeyAsync(IEnumerable<MacroBind> binds, CancellationToken cancellationToken)
        {
            _keybinds = binds.ToDictionary(k => (k.Modifier, k.KeyCode), v => v.Value);
            _hook = new TaskPoolGlobalHook(1, GlobalHookType.Keyboard);
            cancellationToken.Register(Stop);
            _hook.KeyTyped += HandleKeyTyped;
            await _hook.RunAsync();
        }

        public void Stop() => _hook?.Dispose();

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
    }
}
