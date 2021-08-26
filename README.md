<img src="https://github.com/dutch-and-bold/flystorage/raw/main/.github/flystorage-logo.png" alt="Flystorage Logo" title="Flystorage" align="right" height="64" srcset="https://github.com/dutch-and-bold/flystorage/raw/master/.github/flystorage-logo@2x.png 2x"/>

# Flystorage

![.NET](https://img.shields.io/badge/.NET-5.0-purple)
[![Restore, Build and Test Solution](https://github.com/dutch-and-bold/flystorage/actions/workflows/restore-build-test-solution.yml/badge.svg)](https://github.com/dutch-and-bold/flystorage/actions/workflows/restore-build-test-solution.yml)

Flystorage is a .NET port of the massively popular file storage library [Flysystem](https://flysystem.thephpleague.com/v2/docs/). It provides the same interface to interact with many different types of filesystems.
When you use Flystorage, you’re not only protected from vendor lock-in, you’ll also have a consistent experience for which ever storage is right for you.

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
- [Support](#support)
- [Contributing](#contributing)

## Installation

WIP

## Usage

WIP

## Flysystem API

However this project is a port of the original Flysystem API, there have been a few design changes to better accomodate for C# and .NET.
These decisions are [described here.](https://github.com/dutch-and-bold/flystorage/blob/main/docs/flysystem-api-changes.md)

## Porting progress

|Adapter         |Available      |
|----------------|---------------|
|Local           |✅ Yes         |
|InMemory        |✅ Yes         |
|AWS S3          |⌛️ In-progress |
|AsyncAws S3     |⌛️ In-progress |
|FTP             |⌛️ In-progress |
|SFTP            |⌛️ In-progress |

## Support

Please [open an issue](https://github.com/dutch-and-bold/flystorage/issues/new) for support.

## Contributing

Please contribute using [Github Flow](https://guides.github.com/introduction/flow/).
Create a branch, add commits, and [open a pull request](https://github.com/dutch-and-bold/moneybird-sdk/compare/).

### Setting up the project

1. Clone the project to a local directory
2. Restore packages with nuget

**Requirements**
* .NET SDK >= 5.0

#### Running tests

The project contains several xUnit unit tests, the tests can be run with `dotnet test` or using your IDE.

### Coding style and rules

This project adopts the Microsoft recommended code quality rules and .NET API usage rules. To adhere to these rules the project uses [Microsoft.CodeAnalysis.NetAnalyzers](https://github.com/dotnet/roslyn-analyzers) package for code analysis in all projects.
On top of that we also do adhere the [Microsoft Naming Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines).

### Versioning

This package adheres the [SemVer v2](https://semver.org) versioning theory.
To automate the versioning process we are using [Nerdbank.Gitversioning](https://github.com/dotnet/Nerdbank.GitVersioning).

When a branch is merged with the main branch, the PATCH version will automatically increment.
Because we use SemVer please increment the minor version in the `version.json` file when your branch adds a new feature.
Please be aware that each package has it's own `version.json` file.