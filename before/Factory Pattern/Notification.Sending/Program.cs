using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var app = builder.Build();

static void PrintHeader(string title)
{
    Console.WriteLine();
    Console.WriteLine($"┌─ {title} {new string('─', Math.Max(0, 60 - title.Length))}┐");
    Console.WriteLine();
}
