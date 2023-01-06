# Entity Framework With Code-First Migrations in .Net Maui

**By Nick Kovalsky**

![Image](https://github.com/taublast/MauiEF/blob/main/Images/banner.jpg?raw=true)

## Onboarding

If you are a fan like me of EF and would like to use it in your mobile app, my guess is with Maui on the market it’s time to start.

Just a small reminder, for better app startup time it might be better to store boostrap data to the mobile device local storage in a json form. And when it comes to managing big local data with filters, ordering and such, EF is definitely a way to go.

This article's goal is to help one avoid all the hassle of looking for different solutions to small problems when implementing a production-ready mobile local database. At the end you would find the link to the full sample source.

The proven standart for a mobile client database is SQLite. We’ll instantly look for **Microsoft.EntityFrameworkCore.Sqlite** nuget package to install, along with **SQLitePCLRaw.bundle_e_sqlite3** for native sqlite implementations. To create EF migrations we’d also need to install **Microsoft.EntityFrameworkCore.Tools**.

Things are that we cannot install these nugets into our single maui project because if we then try to create migrations we’d get a nice

> *Startup project 'MauiEF.Client' targets platform 'Android'. The Entity Framework Core Package Manager Console Tools don't support this platform. See https://aka.ms/efcore-docs-pmc-tfms for more information.*

..so we'd need to find a way to work around it.

For those who are not sure what migrations are for, it's basically the code that instructs EF of database structure to create and use. The important benefit here is that when you change your models you would just have to auto-generate additional migrations for EF to modify the existing database accordingly, adding-removing tables, columns etc.

## Projects Setup

Now that we know that EF design tools have unsupported frameworks, we would have to create a special "Migrator" project directly runnable by EF and targeting pure net-7.0, and we will be able to create migrations on a windows or an apple machine.

Both ***Migrator*** and our ***Client*** would need to access the database, so we’ll move all our context-related code into a separate ***Shared*** project. Our solution structure would then look like this:

![Image](https://github.com/taublast/MauiEF/blob/main/Images/image1.jpg?raw=true)

The ***Shared*** project would reference

    <!--Local database-->
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="7.0.1" ></PackageReference>
    <PackageReference Include="SQLitePCLRaw.bundle_e_sqlite3" Version="2.1.3" ></PackageReference>

while ***Migrator*** would need additional tools to create migrations:

    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="7.0.1">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>

## Sample Models

We will define our example context inside the *Shared* project as follows:

<details open><summary>LocalDatabase.cs</summary>

```csharp

    /// <summary>
    /// Constructor for creating migrations
    /// </summary>
    public LocalDatabase()
    {
        File = Path.Combine("../", "Data1.db3");
        Initialize();
    }

    /// <summary>
    /// Constructor for mobile app
    /// </summary>
    /// <param name="filenameWithPath"></param>
    public LocalDatabase(string filenameWithPath)
    {
        File = filenameWithPath;
        Initialize();
    }

    void Initialize()
    {
        if (!Initialized)
        {
            Initialized = true;

            SQLitePCL.Batteries_V2.Init();

            Database.Migrate();
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlite($"Filename={File}");
    }
```

</details>

Notice 2 constructors here, one for the EF migrator and one for our app consumption. **Database.Migrate();** creates the database files if it dowsn't exist and applies provided migrations.

You might maybe want to implement the **Database.EnsureDeleted();** method for debug purposes, to wipe out the data at start.

In case you have a breaking app change you might also want change the database filename to recreate the db from scratch for existing users.

OK, now we can inject the context inside our **MauiProgram.cs**:

```csharp
builder.Services.AddTransient<LocalDatabase>((services) =>
        {
            return new LocalDatabase(Path.Combine(FileSystem.AppDataDirectory, "SQLite001.db3"));
        });
```
## Create Migrations

The provided source code doesn’t include migrations. If you just compile and run the solution it will throw an exception, as EF would not know how you what it to **Migrate();**. 

But please don't worry migrations are very easy to create.

If you are using **Visual Studio for Windows** :

1. Change your startup project from ***Client*** to ***Migrator***.

2. Open Package Manager Console: go to View->Other Windows->Package Manager Console.

3. Set the default project to ***Shared***, EF will look for the context model inside and add migrations there.

4. Enter the following command to create the initial migration:

```csharp
add-migration Initial -Context MauiEF.Shared.Services.LocalDatabase -Verbose
```
![Image](https://github.com/taublast/MauiEF/blob/main/Images/image2.jpg?raw=true)

The next method via command like will be illustrated using **Visual Studio for Mac**.

1 Open console: Right-click your solution name, then select Open In Terminal. You should land inside the solution folder.

2 Enter the following command to create the initial migration via command line:

    dotnet ef migrations add Initial -s Migrator -p Shared  -c MauiEF.Shared.Services.LocalDatabase

You will notice that we specified our startup project subfolder **-s Migrator** and default project subfolder **-p Shared**.

If you encounter an error due to ef command not present, install it and add it to global path:
```csharp
dotnet tool install --global dotnet-ef
export PATH="$PATH:/Users/YOUR_USERNAME/.dotnet/tools"
```
Every time you change your context models you'd have to create additional migrations. This way your app won't break, if you release a new version with modified migrations EF will (most probably) keep the user's existing database and modify it according to the new scheme upon initialization of your context model. Why most probably? Because you might change your models in a way a foreign key relation or property will be lost, in this case EF will warn you when creating a migration with something like "Warning data will be lost", so it's all under your control, to manage the existing users old apps versions data.

To create a new migration just create a unique name for it (**Visual Studio for Windows** example):
```csharp
add-migration Change1 -Context MauiEF.Shared.Services.LocalDatabase -Verbose
```

## Run The App

We can now compile our sample and run, to have user-created data to be persistent between app launches. 

As you will see the sample is a Maui App template with database logic added. Context operations are executed asynchronously, so we don't block the UI thread.

<details open><summary>MainPage.cs</summary>

```csharp
 
    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        var title = $"My Story Part {count}";
        var book = _author.Books.FirstOrDefault(x => x.Title == title);
        if (book == null)
        {
            CounterBtn.Text = $"Wrote \"{title}\"";

            Task.Run(async () =>
            {
                _author.Books.Add(new Book
                {
                    Title = title
                });
                _context.Authors.Update(_author);
                await _context.SaveChangesAsync();

                Update();

            }).ConfigureAwait(false);
        }
        else
        {
            CounterBtn.Text = $"Reading \"{title}\"";
        }

        SemanticScreenReader.Announce(CounterBtn.Text);
    }

```
</details>
    
An important note: when you compile your EF Maui app for iOS Release it would most probably crash at runtime on real device, due to the fact that iOS AOT compilation doesn't support some EF techniques. I wouldn't be more precise here, you can [read more about it](http://github.com/xamarin/xamarin-macios/issues/16228 "read more about it"), but the remedy is to add some flavor into your .csprj file for that specific case:
```csharp
<!--IOS RELEASE-->
<PropertyGroup Condition="'$(Configuration)|$(TargetFramework)|$(Platform)'=='Release|net7.0-ios|AnyCPU'">
    <!--to be able to use some EF core methods-->
    <MtouchExtraArgs>--interpreter</MtouchExtraArgs>
    <UseInterpreter>True</UseInterpreter>
    <!--your codesign parameters will go below-->
</PropertyGroup>
```
Apps compiled with such settings already have been approved to AppStore and no performance impact has been reported.

I hope you would find this small article to be useful!
