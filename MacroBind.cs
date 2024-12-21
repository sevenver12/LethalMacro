using SharpHook.Native;

namespace LethalMacro
{
    public record MacroBind
    {
        public required ModifierMask Modifier { get; set; }
        public required KeyCode KeyCode { get; set; }
        public required string Value { get; set; }

        public static MacroBind Empty => new()
        {
            KeyCode = KeyCode.VcUndefined,
            Modifier = ModifierMask.None,
            Value = string.Empty
        };
    }
}
