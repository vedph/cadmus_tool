# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [11.0.0] - 2025-11-24

- ⚠️ Upgraded to NET 10.

## [10.0.2] - 2025-03-26

### Added

- `run-mongo` command

### Changed

- Updated packages

## [10.0.1] - 2025-03-15

### Changed

- Updated packages

## [10.0.0] - 2025-02-01

### Changed

- ⚠️ Upgraded to NET 9
- Updated packages

### Removed

- Legacy MySql support

## [9.0.5] - 2024-04-16

### Changed

- Updated packages

## [9.0.4] - 2024-02-11

### Changed

- Updated packages

### Fixed
- Handle inner exception in CLI commands

## [9.0.3] - 2024-01-31

### Changed

- Updated packages

## [9.0.2] - 2024-01-31

### Fixed

- Catch errors in commands

## [9.0.1] - 2024-01-31

### Changed

- Updated packages

## [9.0.0] - 2023-11-21

### Changed

- ⚠️ Upgraded to NET 8

## [8.0.10] - 2023-10-06

### Added

- `create-db` command to CLI to create index or graph databases

## [8.0.9] - 2023-10-05

### Changed

- Updated packages (new graph DB schema)

## [8.0.8] - 2023-09-25

### Changed

- Updated packages

## [8.0.6] - 2023-08-30

### Changed

- Minor aesthetic changes
- Trying to fix GitHub action

## [8.0.4] - 2023-08-09

### Changed

- Updated packages

### Fixed

- Fix to triples import in command

## [8.0.3] - 2023-08-03

### Changed

- Updated packages

## [8.0.2] - 2023-07-26

### Changed

- Updated packages

## [8.0.1] - 2023-07-17

### Changed

- Updated packages
- Use `AddMappingByName` in mappings import

## [8.0.0] - 2023-06-20

### Added

- Thesaurus import command

### Changed

- Included EF-based PgSql/MySql components to update to [RDBMS refactoring](https://myrmex.github.io/overview/cadmus/dev/history/b-rdbms/)
- Updated packages

## [7.0.0] - 2023-05-27

### Changed

- Updated packages (breaking changes for `AssertedCompositeId`)

## [6.1.2] - 2023-05-16

### Changed

- Updated packages
- More information in graph one command explanation
- Updated GitHub actions in script

### Removed

- Legacy code dependencies

## [6.1.1] - 2023-05-15

### Added

- Hydration to triples import command
- `item-eid` to metadata in graph commands

### Changed

- Updated packages

## [6.1.0] - 2023-04-10

### Changed

- Refactored CLI infrastructure to use [Spectre.Console](https://spectreconsole.net)

## [6.0.1] - 2023-04-10

### Added

- Build script

### Changed

- Updated packages and action

## [6.0.0] - 2023-02-05

### Changed

- Migrated to new components factory. This is a breaking change for backend components, please see [this page](https://myrmex.github.io/overview/cadmus/dev/history/#2023-02-01---backend-infrastructure-upgrade). Anyway, in the end you just have to update your libraries and a single namespace reference. Benefits include:
  - More streamlined component instantiation
  - More functionality in components factory, including DI
  - Dropped third party dependencies
  - Adopted standard MS technologies for DI

## [5.1.0] - 2023-01-10

### Changed

- Refactored CLI infrastructure

## [5.0.0] - 2022-11-10

### Added

- Enable nullability

### Changed

- Upgraded to NET 7
- Updated packages (nullability enabled in Cadmus core)
- `PluginFactoryProvider`: allow custom directory
- Moved `Cadmus.Cli.Core` from Cadmus core solution to this solution
- Updated packages and injection for new `IRepositoryProvider`. This makes CLI-specific providers for repository and seeders factory obsolete

## [2.1.2] - 2022-10-10

### Changed

- Updated packages

## [2.1.1] - 2022-09-09

### Changed

- Updated packages

## [2.1.0] - 2022-08-14

### Added

- Get object command (for preview)

### Changed

- Updated packages
- Refactored graph related commands
