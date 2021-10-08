# Cadmus Tool

Cadmus configuration and utility tool.

This tool requires plugin providers under its `plugins` folder.

Plugins are used to get Cadmus factory providers. A Cadmus factory provider plugin acts as a hub entry point for all the components to be packed in the CLI tool for a specific project.

Place your own plugins in a subfolder of this folder, naming each subfolder after the DLL plugin filename.

For instance, plugin `Cadmus.Cli.Plugin.Mqdq.dll` should be placed in a subfolder of the `plugins` folder named `Cadmus.Cli.Plugin.Mqdq`.

Plugins are not found in this solution. They can be found in a corresponding CLI project in the respective Cadmus backend solutions.

## Index Database Command

Index the specified Cadmus database into a MySql database. If the MySql database does not exist, it will be created; if it exists, it will be cleared if requested.

This requires a plugin with providers for the repository factory and the parts seeders factory. Each project has its own plugin, which must be placed in a subfolder of the tool's `plugins` folder.

```ps1
./cadmus-tool index <DatabaseName> <JsonProfilePath> <RepositoryFactoryProviderTag> [-c]
```

- `-c`=clear the target database when it exists.

Sample:

```bash
./cadmus-tool index cadmus c:\users\dfusi\desktop\cadmus-profile.json repository-factory-provider.mqdq
```

## Seed Database Command

Create a new Cadmus MongoDB database (if the specified database does not already exists), and seed it with a specified number of random items.

```ps1
./cadmus-tool seed <DatabaseName> <JsonProfilePath> <RepositoryFactoryProviderTag> <SeedersFactoryProviderTag> [-c count] [-d] [-h]
```

- `-c N`: the number of items to be seeded. Default is 100.
- `-d`: dry run, i.e. create the items and parts, but do not create the database nor store anything into it. This is used to test for seeder issues before actually running it.
- `-h`: add history items and parts together with the seeded items and parts. Default is `false`. In a real-world database you should set this to `true`.

For a sample seed profile see `Assets/SeedProfile.json`.

## Build SQL Command

Build SQL code for querying the Cadmus index database, once or interactively.

```ps1
./cadmus-tool build-sql <DatabaseType> [-q query]
```

The database type can be `mysql` or `mssql`. Anyway, the current Cadmus implementation uses MySql.

- `-q`: the query (for non-interactive mode).

This allows you to interactively build SQL code. Otherwise, add your query after a `-q` option, e.g.:

```bash
./cadmus-tool build-sql mysql [dsc*=even]
```

where `mysql` is the index database type, which is currently MySql.

## Legacy Commands

These commands are obsolete, but we keep their documentation here for reference.

### Import LEX

Import into a Cadmus database an essential subset of roughly filtered data to be used as seed data. This is a very minimal conversion from Zingarelli LEX files, just to have some fake data to work with.

	cadmus-tool import-lex <lexDirectory> <databaseName> <profileXmlFilePath> [-p|--preflight]

The profile JSON file defines items facets and flags. You can find a sample in `cadmus-tool/Assets/Profile-lex.json`. Note that this profile is used only to provide a better editing experience, and does not reflect a real limitation for allowed parts in the database.

	cadmus-tool import-lex c:\users\dfusi\desktop\lex cadmuslex c:\users\dfusi\desktop\Profile.json -p

### Legacy Seed

Seed a Cadmus database (creating it if it does not exist) with a specified number of random items with their parts.

	cadmus-tool seed <databaseName> <profileXmlFilePath> <facetsCsvList> [-c|--count itemCount]

The profile JSON file defines items facets and flags. You can find a sample in `cadmus-tool/Assets/Profile.json`. Note that this profile is used only to provide a better editing experience, and does not reflect a real limitation for allowed parts in the database.

The items count defaults to 100. Example:

	.\cadmus-tool.exe seed cadmus \Projects\Core20\CadmusApi\cadmus-tool\Assets\Profile.json facet-default -c 100

### Import LEX

Create and seed a Cadmus MongoDB database with the specified profile, importing LEX files from a folder.

	cadmus-tool import-lex inputDirectory databaseName profilePath [-p]

Option `-p` = preflight, i.e. do not touch the target database.

Example:

	dotnet .\cadmus-tool.dll import-lex c:\users\dfusi\desktop\lex cadmuslex c:\users\dfusi\desktop\Profile.json -p
