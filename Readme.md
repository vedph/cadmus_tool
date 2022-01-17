# Cadmus Tool

- [Cadmus Tool](#cadmus-tool)
  - [Index Database Command](#index-database-command)
  - [Seed Database Command](#seed-database-command)
  - [Graph One Command](#graph-one-command)
  - [Graph Many Command](#graph-many-command)
  - [Add Graph Presets Command](#add-graph-presets-command)
  - [Update Node Classes Command](#update-node-classes-command)
  - [Build SQL Command](#build-sql-command)
  - [Legacy Commands](#legacy-commands)
    - [Import LEX](#import-lex)
    - [Legacy Seed](#legacy-seed)

Cadmus configuration and utility tool.

Since version 2, this tool requires plugin providers under its `plugins` folder. The plugin architecture is a planned upgrade to avoid the tool being dependent on Cadmus projects, each having its own models. Otherwise, every Cadmus project should be included as a dependency in the CLI tool, defeating the purpose of a generic and universal tool. Plugins are used to get Cadmus factory providers. A Cadmus factory provider plugin acts as a hub entry point for all the components to be packed in the CLI tool for a specific project.

The tool has now become more like an empty shell, where project-dependent components are demanded to plugins under its `plugins` folder. The commands requiring plugins are those used to build a full Cadmus MySql index from its Mongo database, or to seed a Mongo Cadmus database with mock data. To this end, the CLI tool requires two factory objects: one for the repository, acting as the hub for all its parts and fragments; and another for the part and fragment seeders.

These providers glue together the composable Cadmus parts, and as such are surface components laid on top of each Cadmus solution, just like services in the web APIs. A similar service is also required for these two functions of the CLI tool, for each Cadmus soluition. So, each solution will provide its plugin. Plugins are an easy solution for the CLI tool because runtime binding via reflection there is a viable option, which instead is not the case for the API backend (this gets packed into a different Docker image for each solution).

Consequently, plugins are not found in this CLI solution. They can be found in a corresponding CLI project in the respective Cadmus backend solutions.

To use a plugin, place its files (the library and its dependencies) in a subfolder of this folder, naming each subfolder after the DLL plugin filename. For instance, plugin `Cadmus.Cli.Plugin.Mqdq.dll` should be placed in a subfolder of the `plugins` folder named `Cadmus.Cli.Plugin.Mqdq`.

## Index Database Command

Index the specified Cadmus database into a MySql database. If the MySql database does not exist, it will be created; if it exists, it will be cleared if requested.

This requires a plugin with providers for the repository factory and the parts seeders factory. Each project has its own plugin, which must be placed in a subfolder of the tool's `plugins` folder.

```ps1
./cadmus-tool index <DatabaseName> <JsonProfilePath> <RepositoryProviderTag> [-c]
```

- `-c`=clear the target database when it exists.

Sample:

```bash
./cadmus-tool index cadmus-pura ./plugins/Cadmus.Cli.Plugin.Pura/seed-profile.json cli-repository-provider.pura
```

## Seed Database Command

Create a new Cadmus MongoDB database (if the specified database does not already exists), and seed it with a specified number of random items.

```ps1
./cadmus-tool seed <DatabaseName> <JsonProfilePath> <RepositoryProviderTag> <SeedersFactoryProviderTag> [-c count] [-d] [-h]
```

- `-c N`: the number of items to be seeded. Default is 100.
- `-d`: dry run, i.e. create the items and parts, but do not create the database nor store anything into it. This is used to test for seeder issues before actually running it.
- `-h`: add history items and parts together with the seeded items and parts. Default is `false`. In a real-world database you should set this to `true`.

Sample:

```ps1
./cadmus-tool seed cadmus-pura ./plugins/Cadmus.Cli.Plugin.Pura/seed-profile.json cli-repository-provider.pura cli-seeder-factory-provider.pura -c 10 -d
```

## Graph One Command

Map a single item/part into graph.

```ps1
./cadmus-tool graph-one <DatabaseName> <JsonProfilePath> <RepositoryProviderTag> <IdArgument> [-p] [-d]
```

- `-p`: the ID refers to a part rather than to an item.
- `-d`: the ID refers to an item/part which was deleted.

Sample:

```ps1
./cadmus-tool graph-one cadmus-pura ./plugins/Cadmus.Cli.Plugin.Pura/seed-profile.json cli-repository-provider.pura a47e233b-b50c-4110-af5b-343e12decdac
```

## Graph Many Command

Map all the items into graph.

```ps1
./cadmus-tool graph-many <DatabaseName> <JsonProfilePath> <RepositoryProviderTag>
```

Sample:

```ps1
./cadmus-tool graph-many cadmus-pura ./plugins/Cadmus.Cli.Plugin.Pura/seed-profile.json cli-repository-provider.pura
```

## Add Graph Presets Command

Add preset nodes, node mappings, or thesauri class nodes into graph.

```ps1
./cadmus-tool graph-add <JsonFilePath> <DatabaseName> <JsonProfilePath> <RepositoryProviderTag> [-t] [-d] [-r] [-p <Prefix>]
```

- `-t`: data type: `n`odes (default), `m`appings, `t`hesauri.
- `-r`: when importing thesauri, make the thesaurus' ID the root class node.
- `-p <Prefix>`: when importing thesauri, set the prefix to be added to each class node.
- `-d`: dry mode - don't write to database.

Sample:

```ps1
./cadmus-tool graph-add c:/users/dfusi/desktop/nodes.json cadmus-pura ./plugins/Cadmus.Cli.Plugin.Pura/seed-profile.json cli-repository-provider.pura
```

All data files are JSON documents, having their root element as an objects array. Samples for each type follow.

**Nodes** (omit all the properties you don't need):

```json
[
  {
    "uri": "x:alpha",
    "isClass": true,
    "tag": null,
    "label": "Alpha"
  }
]
```

**Mappings** (omit all the properties you don't need):

```json
[
  {
    "id": 3,
    "parentId": 2,
    "name": "A child mapping",
    "ordinal": 0,
    "facetFilter": null,
    "groupFilter": null,
    "flagsFilter": 0,
    "titleFilter": null,
    "partType": null,
    "partRole": null,
    "pinName": null,
    "prefix": null,
    "labelTemplate": null,
    "tripleS": null,
    "tripleP": null,
    "tripleO": null,
    "tripleOPrefix": null,
    "isReversed": false,
    "slot": null,
    "description": null
  }
]
```

Note that mappings have a numeric ID which is used to allow children mappings reference their parent. Yet, each mapping will get a new ID when injected in the database.

**Thesauri**:

```json
[
  {
    "id": "languages@en",
    "entries": [
      {
        "id": "eng",
        "value": "English"
      },
      {
        "id": "fre",
        "value": "French"
      }
    ]
  }
]
```

## Update Node Classes Command

Update the index of nodes classes in the index database. This is a potentially long task, depending on the number of nodes and the depth of class hierarchies.

```ps1
./cadmus-tool graph-cls <DatabaseName> <ProfilePath>
```

Sample:

```ps1
./cadmus-tool graph-cls cadmus-pura ./plugins/Cadmus.Cli.Plugin.Pura/seed-profile.json
```

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

```ps1
./cadmus-tool import-lex <lexDirectory> <databaseName> <profileXmlFilePath> [-p|--preflight]
```

The profile JSON file defines items facets and flags. You can find a sample in `cadmus-tool/Assets/Profile-lex.json`. Note that this profile is used only to provide a better editing experience, and does not reflect a real limitation for allowed parts in the database.

```ps1
./cadmus-tool import-lex c:\users\dfusi\desktop\lex cadmuslex c:\users\dfusi\desktop\Profile.json -p
```

### Legacy Seed

Seed a Cadmus database (creating it if it does not exist) with a specified number of random items with their parts.

```ps1
./cadmus-tool seed <databaseName> <profileXmlFilePath> <facetsCsvList> [-c|--count itemCount]
```

The profile JSON file defines items facets and flags. You can find a sample in `cadmus-tool/Assets/Profile.json`. Note that this profile is used only to provide a better editing experience, and does not reflect a real limitation for allowed parts in the database.

The items count defaults to 100. Example:

```ps1
./cadmus-tool seed cadmus \Projects\Core20\CadmusApi\cadmus-tool\Assets\Profile.json facet-default -c 100
```
