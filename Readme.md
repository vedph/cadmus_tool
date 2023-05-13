# Cadmus Tool

- [Cadmus Tool](#cadmus-tool)
  - [Plugin Architecture](#plugin-architecture)
  - [Commands](#commands)
    - [Build SQL Command](#build-sql-command)
    - [Get Object Command](#get-object-command)
    - [Index Database Command](#index-database-command)
    - [Seed Database Command](#seed-database-command)
    - [Graph Dereference Mappings](#graph-dereference-mappings)
    - [Graph Import Command](#graph-import-command)
    - [Graph One Command](#graph-one-command)
    - [Graph Many Command](#graph-many-command)
    - [Update Graph Classes Command](#update-graph-classes-command)
  - [History](#history)
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

The tool is like an empty shell, where project-dependent components are demanded to plugins under its `plugins` folder. The commands requiring plugins are those used to build a full Cadmus MySql index from its Mongo database, or to seed a Mongo Cadmus database with mock data. To this end, the CLI tool requires two factory objects: one for the repository, acting as the hub for all its parts and fragments; and another for the part and fragment seeders.

These providers glue together the composable Cadmus parts, and as such are surface components laid on top of each Cadmus solution, just like services in the web APIs. Usually they are located in the `Cadmus.PRJ.Services` (where `PRJ` is your project name) library of your project.Plugins are an easy solution for the CLI tool because runtime binding via reflection there is a viable option, which instead is not the case for the API backend (which gets packed into a different Docker image for each solution).

To add a plugin:

1. create a subfolder of this folder, named after the DLL plugin filename (usually `Cadmus.PRJ.Services`, where `PRJ` is your project name). For instance, the plugin `Cadmus.Tgr.Services.dll` should be placed in a subfolder of this folder named `Cadmus.Tgr.Services`.
2. copy the plugin files including all its dependencies in this folder.
3. it is also useful to copy the project configuration file (`seed-profile.json`) in this folder, so you can have it at hand when required.

## Commands

### Build SQL Command

🎯 Build SQL code for querying the Cadmus index database, once or interactively.

```ps1
./cadmus-tool build-sql [-q query]
```

- `-q`: the query (for non-interactive mode).

This allows you to interactively build SQL code. Otherwise, add your query after a `-q` option, e.g.:

```bash
./cadmus-tool build-sql [dsc*=even]
```

### Get Object Command

🎯 Get the JSON code representing an item or a part's content, optionally also converted in XML.

```ps1
./cadmus-tool get-obj <DatabaseName> <ID> <OutputDir> [-t <RepositoryPluginTag>] [-p] [-x]
```

- `p`: the ID refers to a part rather than to an item.
- `x`: also write an XML version of the result.

Sample:

```ps1
./cadmus-tool get-obj cadmus 8e5d5b5d-4b27-4d00-9038-f611a8e199b9 c:\users\dfusi\desktop\ -t repository-provider.itinera -p -x
```

### Index Database Command

🎯 Index the specified Cadmus database into a MySql database. If the MySql database does not exist, it will be created; if it exists, it will be cleared if requested.

This requires a plugin with providers for the repository factory and the parts seeders factory. Each project has its own plugin, which must be placed in a subfolder of the tool's `plugins` folder.

```ps1
./cadmus-tool index <DatabaseName> <JsonProfilePath> [-t <RepositoryPluginTag>] [-c]
```

- `-c`=clear the target database when it exists.

Sample:

```bash
./cadmus-tool index cadmus-itinera ./plugins/Cadmus.Itinera.Services/seed-profile.json -t repository-provider.itinera
```

### Seed Database Command

🎯 Create a new Cadmus MongoDB database (if the specified database does not already exists), and seed it with a specified number of random items.

```ps1
./cadmus-tool seed <DatabaseName> <JsonProfilePath> [-t <RepositoryPluginTag>] [-s <SeedersFactoryPluginTag>] [-c count] [-d] [-h]
```

- `-c N`: the number of items to be seeded. Default is 100.
- `-d`: dry run, i.e. create the items and parts, but do not create the database nor store anything into it. This is used to test for seeder issues before actually running it.
- `-h`: add history items and parts together with the seeded items and parts. Default is `false`. In a real-world database you should set this to `true`.

Sample:

```ps1
./cadmus-tool seed cadmus-itinera ./plugins/Cadmus.Itinera.Services/seed-profile.json -t repository-provider.itinera -s cli-seeder-factory-provider.itinera -c 10 -d
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
./cadmus-tool graph-import <SourcePath> <DatabaseName> [-t <RepositoryPluginTag>] [-m <ImportMode>] [-d] [-r] [-p <ThesaurusIdPrefix>]
```

- `-m`: import mode: `n`odes (default), `t`riples, `m`appings, t`h`esauri.
- `-r`: when importing thesauri, make the thesaurus' ID the root class node.
- `-p <ThesaurusIdPrefix>`: when importing thesauri, set the prefix to be added to each class node.
- `-d`: dry mode - don't write to database.

Sample:

```ps1
./cadmus-tool graph-import c:/users/dfusi/desktop/nodes.json cadmus-itinera -t repository-provider.itinera
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
./cadmus-tool graph-one <DatabaseName> <Id> [-t <RepositoryPluginTag>] [-p] [-d]
```

- `-p`: the ID refers to a part rather than to an item.
- `-d`: the ID refers to an item/part which was deleted.

Sample:

```ps1
./cadmus-tool graph-one cadmus-itinera a47e233b-b50c-4110-af5b-343e12decdac -t repository-provider.itinera
```

### Graph Many Command

🎯 Map all the items into graph.

```ps1
./cadmus-tool graph-many <DatabaseName> [-t <RepositoryPluginTag>]
```

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

## History

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
