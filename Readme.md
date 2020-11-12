# Cadmus Tool

Cadmus configuration and utility tool.

Publishing: see https://stackoverflow.com/questions/44074121/build-net-core-console-application-to-output-an-exe (NET 5).

## Index Database Command

- **command**: `index <dbname> <json-profile-path> [-c]` where `-c`=clean (used for existing target databases).
- **purpose**: index the specified Cadmus database into a MySql database.

Sample:

```bash
./cadmus-tool index cadmus /documents/cadmus-profile.json
```

If the MySql database does not exist, it will be created.

## Seed Database Command

- **command**: `seed <dbname> <json-profile-path> [-c count] [-d] [-h]` where:
  - `-c N` or `--count N`: the number of items to be seeded. Default is 100.
  - `-d` or `--dry`: dry run, i.e. create the items and parts, but do not create the database nor store anything into it. This is used to test for seeder issues before actually running it.
  - `-h` or `--history`: add history items and parts together with the seeded items and parts. Default is `false`. In a real-world database you should set this to `true`.
- **purpose**: create a new Cadmus MongoDB database (if the specified database does not already exists), and seed it with a specified number of random items.

For a sample seed profile see `Assets/SeedProfile.json`.

## SQL Command

- **command**: `sql <dbtype> [-q query]`.
- **purpose**: build SQL code for querying the Cadmus index database, once or interactively.

Sample:

```bash
./cadmus-tool sql mysql
```

This allows you to interactively build SQL code. Otherwise, add your query after a `-q` option, e.g.:

```bash
./cadmus-tool sql mysql [dsc*=even]
```

Here `mysql` is the index database type, which is currently MySql.

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
