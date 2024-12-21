﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SharpHook.Native;
using Spectre.Console;

namespace LethalMacro
{
    public static class PromtsAndStuff
    {
        public static bool Config()
        {
            return AnsiConsole.Prompt(
                                new TextPrompt<bool>("[yellow]C to configure[/], [red]X to exit[/]")
                                    .AddChoice(true)
                                    .AddChoice(false)
                                    .DefaultValue(true)
                                    .WithConverter(choice => choice ? "c" : "n"));
        }

        public static MacroBind ConfigChoices(List<MacroBind> binds)
        {
            var selector = new SelectionPrompt<MacroBind>()
                    .Title("Select the Keybind that you want to modify?")
                    .PageSize(10)
                    .UseConverter(macro =>
                    {
                        if (macro == MacroBind.Empty)
                        {
                            return "[green]Add new macro...[/]";
                        }
                        return $"{string.Join(" + ", [.. macro.Modifier.Split(), macro.KeyCode])}: {macro.Value}";
                    })
                    .AddChoices([.. binds, MacroBind.Empty]);
            return AnsiConsole.Prompt(selector);
        }

        public static ModifyOperation ModifyMacro(MacroBind chosenMacro)
        {
            var selector = new SelectionPrompt<ModifyOperation>()
                   .Title("Select the Keybind that you want to modify?")
                   .PageSize(10)
                   .UseConverter(macro => macro switch
                    {
                        ModifyOperation.Modify => "[yellow]Edit[/]",
                        ModifyOperation.Delete => "[red]Delete[/]",
                        ModifyOperation.Back => "[blue]Back[/]",
                        _ => throw new NotImplementedException()
                    })
                   .AddChoices([ModifyOperation.Modify, ModifyOperation.Delete]);
            return AnsiConsole.Prompt(selector);
        }
    }
}