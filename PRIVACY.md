# Privacy and Personal Data

This document describes the personal data handling practices of the More Cultural Names Mod Builder CLI tool. The application is a self-hosted command-line utility that runs locally on the user's machine, reads local XML data files, and writes generated mod files to a local output directory. No personal data is collected, transmitted, or stored by the application itself.

**Information reviewed:** 2026-10-07

## 📑 Table of Contents

- What This Document Covers
- Self-Hosted Deployments
- Data We Handle
- Processing and Use
- Storage, Retention, and Deletion
- External Processing and Integrations
- Data Protection and Security
- Document Changes
- Contact

## 🔎 What This Document Covers

This document describes how More Cultural Names Mod Builder at https://github.com/hmlendea/more-cultural-names-builder handles personal data. It covers the application behaviour and verified integrations described below. Where the software is self-hosted, the instance operator may have separate responsibilities described below.

## 🏠 Self-Hosted Deployments

More Cultural Names Mod Builder is a self-hosted command-line application. Users download, build, and run the tool on their own machines. The project maintainers do not operate any instance of the application on behalf of users.

The instance operator (the user running the tool) controls all aspects of the application's execution, including:
- Configuration via command-line arguments
- Local input file selection (languages XML, locations XML, optional landed titles file)
- Local output directory for generated mod files
- All local storage, logs, backups, access controls, retention, and request handling

No data is sent from the self-hosted instance to the project maintainers or any external services. The application has no telemetry, update checks, crash reporting, email, authentication, reverse-proxy, object-storage, or monitoring integrations.

## 📥 Data We Handle

### Data Provided to the Application

The application reads the following local files provided by the user via command-line arguments:
- **Languages XML file** (`--lang`): Contains language definitions with IDs, names, and game-specific identifiers. No personal data.
- **Locations XML file** (`--loc`): Contains location definitions with IDs, names, adjectives, and game-specific identifiers. No personal data.
- **Optional landed titles file** (`--landed-titles`): An existing game file to patch (CK2/CK3 only). Contains game title definitions. No personal data.

No personal data is requested, collected, or processed from these files. The data consists entirely of game-related reference data (language codes, location names, title identifiers).

### Data Generated or Collected by the Application

The application does not generate or collect any personal data automatically. It produces the following output:
- **Mod descriptor files** (mod metadata: name, version, supported game version, tags)
- **Localisation files** (generated name mappings for each language)
- **Optional patched landed titles file** (CK2/CK3 only)

All output is written to the user-specified local output directory. No logs, telemetry, crash reports, or audit data are created.

### Data Received from Integrations

The application has no built-in integrations with external services and receives no data from third parties.

## 🧭 Processing and Use

The application processes the data described above for these verified functions:
- **Reading input data** — Languages XML, Locations XML, optional Landed Titles file
- **Resolving localisations** — Matching location names to languages via game-specific identifiers
- **Generating mod files** — Creating game-compatible mod descriptor and localisation files
- **Writing output** — Saving generated files to the user-specified output directory

## 🗄️ Storage, Retention, and Deletion

All data handled by the application resides exclusively on the user's local filesystem:
- **Input files**: User-controlled locations, read-only during execution
- **Output files**: User-specified output directory, persisted until manually deleted by the user
- **In-memory data**: Temporary data structures released when the application exits

The project maintainers do not control storage, deletion, or backups. The instance operator (user) has full control over all files and directories used by the application.

## 🔗 External Processing and Integrations

The application has no built-in external data transfers, integrations, or third-party services. All processing occurs locally.

| Service or integration | Purpose | Data involved | Configuration or documentation |
|-----------------------|---------|---------------|--------------------------------|
| None | N/A | N/A | N/A |

## 🛡️ Data Protection and Security

The application itself implements no data protection mechanisms beyond standard filesystem permissions, as it handles no personal data. For self-hosted deployments, the instance operator is responsible for:
- File system permissions on input and output directories
- Securing any sensitive data that may be present in user-provided input files (though the application expects only game reference data)
- Network exposure (the application makes no network connections)
- Log protection (the application produces no logs by default; verbose mode writes to stdout only)
- Updates (the user controls when to rebuild or update the tool)

No secrets, credentials, or authentication mechanisms are used by the application.

## 🔄 Document Changes

Update this document when application data flows, storage, integrations, or deployment responsibilities change. The current version is published at https://github.com/hmlendea/more-cultural-names-builder/blob/master/PRIVACY.md.

## 📬 Contact

For questions about application data handling, contact the project maintainers via the GitHub repository: https://github.com/hmlendea/more-cultural-names-builder. For a self-hosted instance, contact the instance operator, unless the project explicitly handles the request. Include the game target (CK2, CK3, HOI4, IR) and any relevant configuration details; do not send passwords, access tokens, or other secrets.