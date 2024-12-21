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
            await _context.Database.EnsureCreatedAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                var binds = await _context.Binds.ToListAsync(stoppingToken);
                var cst = new CancellationTokenSource();
                var macro = _keyLog.WaitForKeyAsync(binds, cst.Token);
                AnsiConsole.WriteLine("Macros are running!");

                bool confirmation = PromtsAndStuff.Config();
                cst.Cancel();
                await macro;

                if (!confirmation)
                {
                    Environment.Exit(0);
                }
                var chosenMacro = PromtsAndStuff.ConfigChoices(binds);
                var operation = PromtsAndStuff.ModifyMacro(chosenMacro);
                if (operation == ModifyOperation.Modify)
                {
                    AnsiConsole.Markup("What should be the [green]Keybind[/]?");
                    var newMacro = await _keyLog.CreateNewKeybindAsync();
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
                if (operation == ModifyOperation.Delete)
                {
                    _context.Binds.Entry(chosenMacro).State = EntityState.Deleted;
                    await _context.SaveChangesAsync();
                }
            }
        }


    }
}
