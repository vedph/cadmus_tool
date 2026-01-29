# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [11.0.2] - 2026-01-29

- updated packages.
- improved build script.
- added seed-users command.

## [11.0.1] - 2026-01-22

- updated packages.

## [11.0.0] - 2025-11-24

- ⚠️ Upgraded to NET 10.

## [10.0.2] - 2025-03-26

- `run-mongo` command
- Updated packages

## [10.0.1] - 2025-03-15

- Updated packages

## [10.0.0] - 2025-02-01

- ⚠️ Upgraded to NET 9
- updated packages
- removed legacy MySql support

## [9.0.5] - 2024-04-16

- Updated packages

## [9.0.4] - 2024-02-11

- updated packages
- handle inner exception in CLI commands

## [9.0.3] - 2024-01-31

- updated packages

## [9.0.2] - 2024-01-31

- Catch errors in commands

## [9.0.1] - 2024-01-31

- Updated packages

## [9.0.0] - 2023-11-21

- ⚠️ Upgraded to NET 8

## [8.0.10] - 2023-10-06

- `create-db` command to CLI to create index or graph databases

## [8.0.9] - 2023-10-05

- Updated packages (new graph DB schema)

## [8.0.8] - 2023-09-25

- Updated packages

## [8.0.6] - 2023-08-30

- Minor aesthetic changes
- Trying to fix GitHub action

## [8.0.4] - 2023-08-09

- Updated packages

- Fix to triples import in command

## [8.0.3] - 2023-08-03

- Updated packages

## [8.0.2] - 2023-07-26

- Updated packages

## [8.0.1] - 2023-07-17

- Updated packages
- Use `AddMappingByName` in mappings import

## [8.0.0] - 2023-06-20

- Thesaurus import command

- Included EF-based PgSql/MySql components to update to [RDBMS refactoring](https://myrmex.github.io/overview/cadmus/dev/history/b-rdbms/)
- Updated packages

## [7.0.0] - 2023-05-27

- Updated packages (breaking changes for `AssertedCompositeId`)

## [6.1.2] - 2023-05-16

- Updated packages
- More information in graph one command explanation
- Updated GitHub actions in script
- Removed Legacy code dependencies

## [6.1.1] - 2023-05-15

- Added hydration to triples import command
- Added `item-eid` to metadata in graph commands
- Updated packages

## [6.1.0] - 2023-04-10

- Refactored CLI infrastructure to use [Spectre.Console](https://spectreconsole.net)

## [6.0.1] - 2023-04-10

- Build script
- Updated packages and action

## [6.0.0] - 2023-02-05

- Migrated to new components factory. This is a breaking change for backend components, please see [this page](https://myrmex.github.io/overview/cadmus/dev/history/#2023-02-01---backend-infrastructure-upgrade). Anyway, in the end you just have to update your libraries and a single namespace reference. Benefits include:
  - More streamlined component instantiation
  - More functionality in components factory, including DI
  - Dropped third party dependencies
  - Adopted standard MS technologies for DI

## [5.1.0] - 2023-01-10

- Refactored CLI infrastructure

## [5.0.0] - 2022-11-10

- Enable nullability
- Upgraded to NET 7
- Updated packages (nullability enabled in Cadmus core)
- `PluginFactoryProvider`: allow custom directory
- Moved `Cadmus.Cli.Core` from Cadmus core solution to this solution
- Updated packages and injection for new `IRepositoryProvider`. This makes CLI-specific providers for repository and seeders factory obsolete

## [2.1.2] - 2022-10-10

- Updated packages

## [2.1.1] - 2022-09-09

- Updated packages

## [2.1.0] - 2022-08-14

- Get object command (for preview)
- Updated packages
- Refactored graph related commands
