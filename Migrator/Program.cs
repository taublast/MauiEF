// See https://aka.ms/new-console-template for more information

using MauiEF.Shared.Services;

Console.WriteLine("Migrator running..");

using (var blogContext = new LocalDatabase())
{
    var all = blogContext.Authors.ToList();
}
