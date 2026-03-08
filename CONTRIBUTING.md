# Contributing to Yort.Hexy

Thank you for your interest in contributing! This document provides guidelines and instructions for contributing.

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/YOUR-USERNAME/Yort.Hexy.git`
3. Create a branch: `git checkout -b feature/my-feature`
4. Make your changes
5. Run tests: `dotnet test`
6. Commit and push
7. Open a Pull Request

## Development Requirements

- .NET 9.0 SDK or later (for building all targets)
- An IDE with C# support (Visual Studio, Rider, VS Code + C# Dev Kit)

## Building

```bash
dotnet build
```

## Running Tests

```bash
dotnet test
```

Tests run against .NET 9.0 and .NET 10.0. The library itself also targets .NET Standard 2.0 but tests are run on modern runtimes.

## Running Benchmarks

```bash
cd benchmarks/Yort.Hexy.Benchmarks
dotnet run -c Release --framework net10.0 -- --filter "*"
```

## Coding Standards

- Follow the `.editorconfig` styles enforced by the project
- All public APIs must have XML documentation comments (see Section 17 of the design plan for quality standards)
- All public APIs must have corresponding unit tests
- Use `TreatWarningsAsErrors` — the build will fail on any warning
- Use conditional compilation (`#if NET9_0_OR_GREATER`) for platform-specific optimizations
- Keep `#if` blocks focused and minimal
- Prefer `readonly struct` semantics — never expose internal mutable state

## Pull Request Process

1. Ensure all tests pass
2. Update XML documentation for any new or changed public APIs
3. Add tests for new functionality
4. Update CHANGELOG.md with a summary of changes
5. PRs require review before merging

## Reporting Issues

- Use GitHub Issues
- Include the .NET version, OS, and a minimal reproduction if possible
- Check existing issues before creating a new one

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md).
