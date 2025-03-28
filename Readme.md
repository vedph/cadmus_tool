# Cadmus Tool

- [Cadmus Tool](#cadmus-tool)
  - [Plugin Architecture](#plugin-architecture)
  - [Setup](#setup)
  - [Commands](#commands)
    - [Build SQL Command](#build-sql-command)
    - [Create Database Command](#create-database-command)
    - [Get Object Command](#get-object-command)
    - [Index Database Command](#index-database-command)
    - [Seed Database Command](#seed-database-command)
    - [Graph Dereference Mappings](#graph-dereference-mappings)
    - [Graph Import Command](#graph-import-command)
    - [Graph One Command](#graph-one-command)
    - [Graph Many Command](#graph-many-command)
    - [Update Graph Classes Command](#update-graph-classes-command)
    - [Thesaurus Import Command](#thesaurus-import-command)
      - [File Format](#file-format)
    - [Run Mongo Command](#run-mongo-command)
  - [History](#history)
    - [10.0.2](#1002)
    - [10.0.1](#1001)
    - [10.0.0](#1000)
    - [9.0.5](#905)
    - [9.0.4](#904)
    - [9.0.3](#903)
    - [9.0.2](#902)
    - [9.0.1](#901)
    - [9.0.0](#900)
    - [8.0.10](#8010)
    - [8.0.9](#809)
    - [8.0.8](#808)
    - [8.0.6](#806)
    - [8.0.4](#804)
    - [8.0.3](#803)
    - [8.0.2](#802)
    - [8.0.1](#801)
    - [8.0.0](#800)
    - [7.0.0](#700)
    - [6.1.2](#612)
    - [6.1.1](#611)
    - [6.1.0](#610)
    - [6.0.1](#601)
    - [6.0.0](#600)
    - [5.1.0](#510)
    - [5.0.0](#500)
    - [2.1.2](#212)
    - [2.1.1](#211)
    - [2.1.0](#210)

Cadmus configuration and utility tool.

## Plugin Architecture

Since version 2, this tool requires plugin providers under its `plugins` folder. The plugin architecture makes the tool independent from Cadmus projects, each having its own models. Otherwise, every Cadmus project should be included as a dependency in the CLI tool, thus defeating the purpose of a generic and universal tool.

Plugins are used to get Cadmus factory providers. A Cadmus factory provider plugin acts as a hub entry point for all the components to be packed in the CLI tool for a specific project.

>You can build your plugin with all its dependencies by publishing the library you wish to use as the import target. For instance, if you are going to use library `Cadmus.Itinera.Services` as a plugin, publish it and then copy published files into the corresponding plugin folder.

The tool is like an empty shell, where project-dependent components are demanded to plugins under its `plugins` folder. The commands requiring plugins are those used to build a full Cadmus index from its Mongo database, or to seed a Mongo Cadmus database with mock data. To this end, the CLI tool requires two factory objects: one for the repository, acting as the hub for all its parts and fragments; and another for the part and fragment seeders.

These providers glue together the composable Cadmus parts, and as such are surface components laid on top of each Cadmus solution, just like services in the web APIs. Usually they are located in the `Cadmus.PRJ.Services` (where `PRJ` is your project name) library of your project.Plugins are an easy solution for the CLI tool because runtime binding via reflection there is a viable option, which instead is not the case for the API backend (which gets packed into a different Docker image for each solution).

To add a plugin:

1. create a subfolder of this folder, named after the DLL plugin filename (usually `Cadmus.PRJ.Services`, where `PRJ` is your project name). For instance, the plugin `Cadmus.Tgr.Services.dll` should be placed in a subfolder of this folder named `Cadmus.Tgr.Services`.
2. copy the plugin files including all its dependencies in this folder.
3. it is also useful to copy the project configuration file (`seed-profile.json`) in this folder, so you can have it at hand when required.

## Setup

In this sample I setup the tool with a plugin in an Ubuntu server.

(1) download the tool (change the version to the latest one):

```bash
wget https://github.com/vedph/cadmus_tool/releases/download/v.8.0.8/App-v.8.0.8-linux-x64.tar.gz
```

(2) unzip it and remove the archive:

```bash
tar -xf App-v.8.0.8-linux-x64.tar.gz
rm App-v.8.0.8-linux-x64.tar.gz
```

(3) rename the folder and grant permissions to the tool:

```bash
mv App-v.8.0.8-linux-x64 cadmus-tool
cd cadmus-tool
chmod +x cadmus-tool
```

(4) get the plugin and unzip it:

```bash
cd plugins
wget http://www.fusisoft.it/xfer/cadmus/cli/plugins/Cadmus.Itinera.Services.zip
unzip Cadmus.Itinera.Services.zip
rm Cadmus.Itinera.Services.zip
```

To run the tool, enter its folder and run:

```bash
./cadmus-tool
```

## Commands

### Build SQL Command

🎯 Build SQL code for querying the Cadmus index database, once or interactively.

```ps1
./cadmus-tool build-sql [-q query] [-l legacy]
```

- `-q`: the query (for non-interactive mode).
- `-l`: use legacy syntax for the query. Default is `false`.

This allows you to interactively build SQL code. Otherwise, add your query after a `-q` option, e.g.:

```bash
./cadmus-tool build-sql [dsc*=even]
```

### Create Database Command

🎯 Create an index or graph database with its own schema.

```ps1
./cadmus-tool create-db <index|graph> <DatabaseName>
```

Sample:

```ps1
./cadmus-tool create-db index cadmus-itinera

./cadmus-tool create-db graph cadmus-itinera-graph
```

### Get Object Command

🎯 Get the JSON code representing an item or a part's content, optionally also converted in XML.

```ps1
./cadmus-tool get-obj <DatabaseName> <ID> <OutputDir> [-g <RepositoryPluginTag>] [-p] [-x]
```

- `p`: the ID refers to a part rather than to an item.
- `x`: also write an XML version of the result.

Sample:

```ps1
./cadmus-tool get-obj cadmus 8e5d5b5d-4b27-4d00-9038-f611a8e199b9 c:\users\dfusi\desktop\ -g repository-provider.itinera -p -x
```

### Index Database Command

🎯 Index the specified Cadmus database. If the index database does not exist, it will be created; if it exists, it will be cleared if requested.

This requires a plugin with providers for the repository factory and the parts seeders factory. Each project has its own plugin, which must be placed in a subfolder of the tool's `plugins` folder.

```ps1
./cadmus-tool index <DatabaseName> <JsonProfilePath> [-g <RepositoryPluginTag>] [-c]
```

- `-g`: the target repository provider plugin tag (e.g. `repository-provider.itinera`).
- `-c`=clear the target database when it exists.

Sample:

```bash
./cadmus-tool index cadmus-itinera ./plugins/Cadmus.Itinera.Services/seed-profile.json -g repository-provider.itinera
```

### Seed Database Command

🎯 Create a new Cadmus MongoDB database (if the specified database does not already exists), and seed it with a specified number of random items.

```ps1
./cadmus-tool seed <DatabaseName> <JsonProfilePath> [-g <RepositoryPluginTag>] [-s <SeedersFactoryPluginTag>] [-c count] [-d] [-h]
```

- `-c N`: the number of items to be seeded. Default is 100.
- `-d`: dry run, i.e. create the items and parts, but do not create the database nor store anything into it. This is used to test for seeder issues before actually running the command.
- `-h`: add history items and parts together with the seeded items and parts. Default is `false`. In a real-world database you should set this to `true`.

Sample:

```ps1
./cadmus-tool seed cadmus-itinera ./plugins/Cadmus.Itinera.Services/seed-profile.json -g repository-provider.itinera -s seeder-factory-provider.itinera -c 10 -d
```

### Graph Dereference Mappings

🎯 Dereference mappings in a JSON mappings file by outputting a fully dereferenced list of mappings into another file. This can then be imported via [graph-import](#import-graph-presets-command).

```ps1
./cadmus-tool graph-deref <InputPath> <OutputPath>
```

Sample:

```ps1
./cadmus-tool graph-deref c:/users/dfusi/desktop/mappings.json c:/users/dfusi/desktop/mappings-d.json
```

💡 A mappings document can be used to avoid repeating the same mappings in different places as children mappings. In this JSON document, you have a dictionary of named mappings, where each mapping is keyed under an ID; and a list of document mappings, including all the mappings you want to use. Inside them, you can either specify an inline mapping via `Value`, or reference it via `ReferenceId`. For instance:

```json
{
  "NamedMappings": {
    "event_note": {
      "name": "event's note",
      "source": "note",
      "sid": "{$eid-sid}/note",
      "output": {
        "nodes": {
          "note": "x:notes/n"
        },
        "triples": ["{?event} crm:P3_has_note \"{$.}\""]
      }
    }
  },
  "DocumentMappings": [
    {
      "name": "birth",
      "sourceType": 2,
      "facetFilter": "person",
      "partTypeFilter": "it.vedph.historical-events",
      "description": "Map birth event",
      "source": "events[?type=='person.birth']",
      "output": {
        "metadata": {
          "eid-sid": "{$part-id}/{@eid}"
        }
      },
      "children": [
        {
          "Value": {
            "name": "birth event - eid",
            "source": "eid",
            "sid": "{$eid-sid}",
            "output": {
              "nodes": {
                "event": "x:events/{$.}"
              },
              "triples": [
                "{?event} a crm:E67_Birth",
                "{?event} crm:P98_brought_into_life {$item-uri}"
              ]
            }
          }
        },
        { "ReferenceId": "event_note" }
      ]
    }
  ]
}
```

### Graph Import Command

🎯 Import preset nodes, triples, node mappings, or thesauri class nodes into graph.

```ps1
./cadmus-tool graph-import <SourcePath> <DatabaseName> [-g <RepositoryPluginTag>] [-m <ImportMode>] [-d] [-r] [-p <ThesaurusIdPrefix>]
```

- `-m`: import mode: `n`odes (default), `t`riples, `m`appings, t`h`esauri. Mappings are imported by their _name_, so if you import a mapping with a name equal to one already present in the database, the old one will be updated.
- `-r`: when importing thesauri, make the thesaurus' ID the root class node.
- `-p <ThesaurusIdPrefix>`: when importing thesauri, set the prefix to be added to each class node.
- `-d`: dry mode - don't write to database.

Sample:

```ps1
./cadmus-tool graph-import c:/users/dfusi/desktop/nodes.json cadmus-itinera -g repository-provider.itinera
```

>Note: if you are importing mappings, ensure that the JSON document has a root array property including mappings. When working with a compact mappings document using cross-references, dereference all the referenced mappings via the [apposite command](#graph-dereference-mappings) before importing.

All data files are JSON documents, having as their root element an **array** of objects. For instance:

- **node** (omit all the properties you don't need):

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

- **triple** with non-literal object:

```json
[
  {
    "subjectUri": "x:beta",
    "predicateUri": "rdfs:subClassOf",
    "objectUri": "x:alpha",
    "tag": null
  }
]
```

- **triple** with literal object:

```json
[
  {
    "subjectUri": "x:alpha",
    "predicateUri": "rdf:label",
    "objectLiteral": "Alpha",
    "objectLiteralIx": "alpha",
    "literalType": "xs:string",
    "literalLanguage": "en",
    "literalNumber": null,
    "tag": null
  }
]
```

- **thesaurus**:

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

### Graph One Command

🎯 Map a single item/part into graph.

```ps1
./cadmus-tool graph-one <DatabaseName> <Id> [-g <RepositoryPluginTag>] [-p] [-d]
```

- `-g`: the target repository provider plugin tag (e.g. `repository-provider.itinera`).
- `-p`: the ID refers to a part rather than to an item.
- `-d`: the ID refers to an item/part which was deleted.
- `-x`: explain the update without actually performing it.

Sample:

```ps1
./cadmus-tool graph-one cadmus-itinera 4a0ce97e-84d1-417d-9fb0-a91d9dfc4da7 -g repository-provider.itinera -p -x
```

### Graph Many Command

🎯 Map all the items into graph.

```ps1
./cadmus-tool graph-many <DatabaseName> [-g <RepositoryPluginTag>]
```

- `-g`: the target repository provider plugin tag (e.g. `repository-provider.itinera`).

Sample:

```ps1
./cadmus-tool graph-many cadmus-itinera repository-provider.itinera
```

### Update Graph Classes Command

🎯 Update the index of nodes classes in the index database. This is a potentially long task, depending on the number of nodes and the depth of class hierarchies.

```ps1
./cadmus-tool graph-cls <DatabaseName> <ProfilePath>
```

Sample:

```ps1
./cadmus-tool graph-cls cadmus-itinera ./plugins/Cadmus.Itinera.Services/seed-profile.json
```

### Thesaurus Import Command

🎯 Import one or more thesauri from one or more file(s) into a Cadmus database. Files can be JSON, CSV, XLS, XLSX and are selected according to their extension. Any unknown extension is treated as a JSON source.

```ps1
./cadmus-tool thes-import <InputFileMask> <DatabaseName> [-m <R|P|S>] [-d]
```

- `-m`: the import mode, specifying how to deal when importing onto existing thesauri:
  - `R` = replace (default): if the imported thesaurus already exists, it is fully replaced by the new one.
  - `P` = patch: the existing thesaurus is patched with the imported one: any existing entry has its value overwritten; any non existing entry is just added.
  - `S` = synch: the existing thesaurus is synched with the imported one: this is equal to patch, with the addition that any existing entry not found in the imported thesaurus is removed.
- `-d`: dry run (don't write to database).
- `-s`: for Excel sources, the ordinal number of the sheet to read data from (1-N; default=1).
- `-r`: for Excel sources, the ordinal number of the first row to read data from (1-N; default=1).
- `-c`: for Excel sources, the ordinal number of the first column to read data from (1-N; default=1).

Sample:

```ps1
./cadmus-tool thes-import c:/users/dfusi/desktop/thesauri/*.json cadmus-itinera -d
```

#### File Format

- **JSON**: a single thesaurus as an _object_, or a list of thesauri as an _array of objects_. Each object is encoded like in this sample:

```json
{
  "id": "colors@en",
  "entries": [
    {
      "id": "r",
      "value": "red"
    },
    {
      "id": "g",
      "value": "green"
    },
    {
      "id": "b",
      "value": "blue"
    },
  ]
}
```

An alias thesaurus is encoded like:

```json
{
  "id": "colours@en",
  "targetId": "colors"
}
```

- **CSV**: a comma-delimited UTF8 text file, like in this sample:

```csv
thesaurusId,id,value,targetId
colors@en,r,red,
colors@en,g,green,
colors@en,b,blue,
shapes@en,trg,triangle,
shapes@en,rct,rectangle,
```

You can omit the thesaurus ID if equal to the previous row, e.g.:

```csv
thesaurusId,id,value,targetId
colors@en,r,red,
,g,green,
,b,blue,
shapes@en,trg,triangle,
,rct,rectangle,
```

You must include the header row as the first row of the file. This allows changing the column order at will, as they will be identified by their name.

- **Excel**: XLSX or XLS files. It is assumed that your columns are in this order:

1. thesaurus
2. id
3. value
4. target

You can add a header row or not, and use whatever name you want, as columns get identified by their order. You can anyway specify the sheet number, the first row number, and the first column number.

### Run Mongo Command

🎯 Run Mongo a script targeting the specified database. This is an experimental feature. Note that not all the commands are supported.

```ps1
./cadmus-tool run-mongo <DatabaseName> [-s <Script>] [-f <ScriptFilePath>]
```

- `-s`: the script to run.
- `-f`: the file with the script to run.

Sample:

```ps1
./cadmus-tool run-mongo cadmus-vela -s "db.getCollection(flags).updateOne({ _id: 4 },{ $set:{isAdmin: true }})"
```

👉 Supported commands:

1. Collection Operations:
   - data manipulation: `find`, `findOne`, `insertOne`, `insertMany`, `updateOne`, `updateMany`, `replaceOne`, `deleteOne`, `deleteMany`.
   - collection administration: `drop`, `createIndex`, `dropIndex`, `countDocuments`.
2. Database Operations:
   - administration: `createCollection`, `dropDatabase`, `getCollectionNames`, `listCollections`.
   - information: `stats`, `runCommand`.
3. Connection Operations:
   - `use [database]` - switches to the specified database.
4. Collection Access Methods:
   - `db.collection` - directly access a collection with dot notation.
   - `db.getCollection("name")` - access a collection by name.

## History

### 10.0.2

- 2025-03-26:
  - ➕ added `run-mongo` command.
  - updated packages.

### 10.0.1

- 2025-03-15: updated packages.

### 10.0.0

- 2025-02-01:
  - ⚠️ upgraded to NET 9.
  - removed legacy MySql support.
- 2024-07-26: updated packages.

### 9.0.5

- 2024-04-16: updated packages.

### 9.0.4

- 2024-02-11:
  - updated packages.
  - handle inner exception in CLI commands.

### 9.0.3

- 2024-01-31: updated packages.

### 9.0.2

- 2024-01-31: catch errors in commands.

### 9.0.1

- 2024-01-31: updated packages.

### 9.0.0

- 2023-11-21: ⚠️ upgraded to NET 8.

### 8.0.10

- 2023-10-06: added `create-db` command to CLI to create index or graph databases.

### 8.0.9

- 2023-10-05: updated packages (new graph DB schema).

### 8.0.8

- 2023-09-25: updated packages.

### 8.0.6

- 2023-08-30:
  - minor aesthetic changes.
  - trying to fix GitHub action.

### 8.0.4

- 2023-08-09:
  - fix to triples import in command.
  - updated packages.

### 8.0.3

- 2023-08-03: updated packages.

### 8.0.2

- 2023-07-26: updated packages.

### 8.0.1

- 2023-07-17: updated packages.
- 2023-07-11: use `AddMappingByName` in mappings import.

### 8.0.0

- 2023-06-20: added Thesaurus import command.
- 2023-06-16: included EF-based PgSql/MySql components to update to [RDBMS refactoring](https://myrmex.github.io/overview/cadmus/dev/history/b-rdbms/).
- 2023-05-29: updated packages.

### 7.0.0

- 2023-05-27: updated packages (breaking changes for `AssertedCompositeId`).

### 6.1.2

- 2023-05-16:
  - updated packages.
  - removed legacy code dependencies.
  - updated GitHub actions in script.
- 2023-05-15:
  - updated packages.
  - more information in graph one command explanation.

### 6.1.1

- 2023-05-15: added hydration to triples import command.
- 2023-05-13: updated packages.
- 2023-05-12: updated packages.
- 2023-04-28: updated packages.
- 2023-04-26: added `item-eid` to metadata in graph commands.

### 6.1.0

- 2023-04-10: refactored CLI infrastructure to use [Spectre.Console](https://spectreconsole.net).

### 6.0.1

- 2023-04-10:
  - updated packages and action.
  - added build script.

### 6.0.0

- 2023-02-05: migrated to new components factory. This is a breaking change for backend components, please see [this page](https://myrmex.github.io/overview/cadmus/dev/history/#2023-02-01---backend-infrastructure-upgrade). Anyway, in the end you just have to update your libraries and a single namespace reference. Benefits include:
  - more streamlined component instantiation.
  - more functionality in components factory, including DI.
  - dropped third party dependencies.
  - adopted standard MS technologies for DI.

### 5.1.0

- 2023-01-10: refactored CLI infrastructure.

### 5.0.0

- 2022-11-10: upgraded to NET 7.
- 2022-11-04: updated packages (nullability enabled in Cadmus core).
- 2022-10-14:
  - `PluginFactoryProvider`: allow custom directory.
  - enable nullability.
- 2022-10-12: moved `Cadmus.Cli.Core` from Cadmus core solution to this solution.
- 2022-10-10: updated packages and injection for new `IRepositoryProvider`. This makes CLI-specific providers for repository and seeders factory obsolete.

### 2.1.2

- 2022-10-10: updated packages.

### 2.1.1

- 2022-09-09: updated packages.

### 2.1.0

- 2022-08-14: updated packages, refactored graph related commands, added get object command (for preview).
