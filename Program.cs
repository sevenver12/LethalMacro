using LethalMacro;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddTransient<IKeyLogExecuter, KeyLogExecuter>();
builder.Services.AddHostedService<Worker>();

builder.Services.AddDbContext<MacroDbContext>(c =>
{
    var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    c.UseSqlite($"Data Source={Path.Join(path, "LethalMacro.db")}");
    c.UseAsyncSeeding(async (context, _, cst) =>
    {
        if (await context.Set<MacroBind>().AnyAsync(cst))
        {
            return;
        }
        context.AddRange(MacroDbContext.GetSeed());
        await context.SaveChangesAsync(cst);
    });
}, ServiceLifetime.Singleton);

var host = builder.Build();
host.Run();
