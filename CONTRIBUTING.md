# Contributing to More Cultural Names Mod Builder

This document covers the guidelines and processes for contributing to this project, including how to report issues, suggest enhancements, submit code changes, and follow project standards.

## 📑 Table of Contents

- [How to Contribute](#how-to-contribute)
  - [Reporting Issues](#reporting-issues)
  - [Suggesting Enhancements](#suggesting-enhancements)
  - [Code Contributions](#code-contributions)
    - [Prerequisites](#prerequisites)
    - [Development Setup](#development-setup)
    - [Making Changes](#making-changes)
    - [Pull Request Guidelines](#pull-request-guidelines)
  - [Code Style](#code-style)
  - [Testing](#testing)
  - [Documentation](#documentation)
- [Security](#security)
- [License](#license)
- [Recognition](#recognition)
- [Getting Assistance](#getting-assistance)

## 🤝 How to Contribute

### Reporting Issues

- Search existing issues first.
- Use the issue templates if available.
- Provide clear reproduction steps.
- Include environment details.

### Suggesting Enhancements

- Check the roadmap and existing discussions.
- Explain the use case and expected behaviour.
- Consider implementation complexity.

### Code Contributions

#### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

#### Development Setup

```bash
# Clone the repository
git clone https://github.com/hmlendea/more-cultural-names-builder.git
cd more-cultural-names-builder

# Install dependencies
dotnet restore
```

#### Making Changes

1. Fork the repository.
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Make your changes.
4. Run tests: `dotnet test`
5. Run linters: `dotnet build`
6. Commit with clear and descriptive messages.
7. Push to your fork.
8. Open a Pull Request.

#### Pull Request Guidelines

- Target the `master` branch.
- Keep PRs focused and atomic.
- Revise documentation if applicable.
- Include tests for any new functionality.
- Ensure the CI checks pass.

### Code Style

Follow the project's coding standards:
- C# coding conventions as defined in the repository instructions
- Run `dotnet build` before committing to verify compilation

### Testing

```bash
# Run all tests
dotnet test

# Run specific test suite
dotnet test MoreCulturalNamesBuilder.UnitTests
```

### Documentation

- Revise relevant docs for changes.
- Follow the documentation style guide.
- Preview changes locally if possible.

## 🔒 Security

Report security vulnerabilities per the [Security Policy](SECURITY.md).

## 📄 License

By contributing, you agree that your contributions will be licensed under the GNU General Public License v3.0.

## 🙏 Recognition

Contributors are recognized in the GitHub contributors list.

## ❓ Getting Assistance

- Open an issue on GitHub for questions or problems
- Check existing discussions and documentation first.