using System.ComponentModel;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SharpHook.Native;
using Spectre.Console;

namespace LethalMacro
{
    public class Worker : BackgroundService
    {
        private readonly IKeyLogExecuter _keyLog;
        private readonly MacroDbContext _context;

        public Worker(IKeyLogExecuter keyLog, MacroDbContext context)
        {
            _keyLog = keyLog;
            _context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _context.Database.EnsureCreatedAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var binds = await _context.Binds.ToListAsync(stoppingToken);
                _keyLog.StartRecording(binds);
                AnsiConsole.WriteLine("Macros are running!");
                _keyLog.StopRecording();

                bool confirmation = PromtsAndStuff.PromptConfig();

                if (!confirmation)
                {
                    Environment.Exit(0);
                }
                var chosenMacro = PromtsAndStuff.PromptConfigChoices(binds);
                var operation = PromtsAndStuff.PromptModifyMacro(chosenMacro);
                if (operation == ModifyOperation.Modify)
                {
                    await ModifyAsync(binds, chosenMacro);
                }
                if (operation == ModifyOperation.Delete)
                {
                    _context.Binds.Entry(chosenMacro).State = EntityState.Deleted;
                    await _context.SaveChangesAsync(stoppingToken);
                }
            }
        }

        private async Task ModifyAsync(List<MacroBind> binds, MacroBind chosenMacro)
        {
            MacroBind newMacro = new()
            {
                KeyCode = chosenMacro.KeyCode,
                Modifier = chosenMacro.Modifier,
                Value = chosenMacro.Value,
            };

            int loopCount = 0;
            bool loop = true;
            do
            {
                Console.Clear();
                AnsiConsole.MarkupLine("What should be the [green]Keybind[/]?");
                AnsiConsole.MarkupLine($"{newMacro.Modifier} + {newMacro.KeyCode.ToString()[2..]}");
                var dupe = binds.Where(w => w != chosenMacro).Any(a => a.Modifier == newMacro.Modifier && a.KeyCode == newMacro.KeyCode);
                if (dupe)
                {
                    AnsiConsole.MarkupLine($"[red]Keybind already in use[/]");
                }
                var keybind = await _keyLog.CreateNewKeybindAsync();
                if (keybind.KeyCode == KeyCode.VcEnter && loopCount > 0 && !dupe)
                {
                    loop = false;
                }
                else if (keybind.KeyCode != KeyCode.VcEnter)
                {
                    newMacro = keybind;
                }

                loopCount++;
            } while (loop);

            Console.ReadLine();
            Console.Clear();
            
            newMacro.Value = AnsiConsole.Prompt(new TextPrompt<string>("What should be the value?"));
            if (chosenMacro.Modifier != newMacro.Modifier || chosenMacro.KeyCode != newMacro.KeyCode)
            {
                _context.Remove(chosenMacro);
                _context.Add(newMacro);
            }
            else
            {
                chosenMacro.Value = newMacro.Value;
            }
            await _context.SaveChangesAsync();
        }
    }
}
