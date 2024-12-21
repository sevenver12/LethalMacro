using Microsoft.EntityFrameworkCore;
using SharpHook.Native;

namespace LethalMacro
{
    public class MacroDbContext : DbContext
    {
        public MacroDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<MacroBind> Binds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MacroBind>().HasKey(c => new { c.Modifier, c.KeyCode });
        }

        public static IEnumerable<MacroBind> GetSeed()
        {
            return [new MacroBind()
            {
                KeyCode = KeyCode.Vc0,
                Modifier = ModifierMask.LeftAlt,
                Value = @"switch"
            },

            new MacroBind
            {
                KeyCode = KeyCode.Vc1,
                Modifier = ModifierMask.LeftAlt,
                Value = "switch JustDolt"
            },
            new MacroBind
            {
                KeyCode = KeyCode.Vc2,
                Modifier = ModifierMask.LeftAlt,
                Value = "switch Coachpotato"
            },
            new MacroBind
            {
                KeyCode = KeyCode.Vc3,
                Modifier = ModifierMask.LeftAlt,
                Value = "switch Phoeniixia"
            },
            new MacroBind
            {
                KeyCode = KeyCode.Vc4,
                Modifier = ModifierMask.LeftAlt,
                Value = "switch NeroNomad"
            },
            new MacroBind
            {
                KeyCode = KeyCode.Vc5,
                Modifier = ModifierMask.LeftAlt,
                Value = "switch justdolt"
            }];
        }
    }
}
