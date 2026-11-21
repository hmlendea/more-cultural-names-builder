# Security Policy

This document describes the security policy for the More Cultural Names Mod Builder project, including supported versions, vulnerability reporting procedures, and disclosure expectations.

## 📑 Table of Contents

- [Supported Versions](#supported-versions)
- [Reporting a Vulnerability](#reporting-a-vulnerability)
- [Scope](#scope)
- [Disclosure Policy](#disclosure-policy)
- [Automated Security Scanning](#automated-security-scanning)
- [Safe Harbour](#safe-harbour)
- [Recognition](#recognition)

## 🛡️ Supported Versions

Use this table to indicate which project versions currently receive security maintenance.

| Version | Distribution Channel | Supported |
|---------|--------------------|-----------|
| Latest version | GitHub Releases | ✅ |
| Preceding versions | Any distribution channel | ❌ |

## 🚨 Reporting a Vulnerability

Please do not disclose suspected vulnerabilities publicly before maintainers have had an opportunity to validate and remediate them.

To report a vulnerability:
- [GitHub Security Advisories](https://github.com/hmlendea/more-cultural-names-builder/security/advisories)
- Contact the maintainers directly

## 📌 Scope

The original maintainers are not responsible for changes, modifications, or security issues introduced by downstream forks.

The subsequent report categories are in scope for this repository:
- Code execution vulnerabilities in the mod generation logic
- Path traversal or arbitrary file write in output directory handling
- XML external entity injection in language or location data parsing
- Denial of service through crafted input data

The subsequent categories are out of scope unless explicitly stated to the contrary:
- Vulnerabilities in the target games (CK2, CK3, EU5, HOI4, IR) themselves
- Issues in third-party dependencies not directly exploitable through this tool
- Social engineering or phishing attacks
- Physical security issues

## 📢 Disclosure Policy

This project follows coordinated disclosure:
1. Vulnerabilities are investigated privately.
2. A remediation plan is prepared and validated.
3. Public disclosure is published after a fix, mitigation, or agreed risk decision is available.
4. Credit is attributed in accordance with reporter preference and project policy.

## 🔍 Automated Security Scanning

This repository runs automated security scans on every push to `master`, on every pull request targeting `master`, and on a weekly schedule. The scan includes:
- **Vulnerable package detection** using `dotnet list package --vulnerable --include-transitive`
- **CodeQL static analysis** for C# security issues

Results are published to the [Security](https://github.com/hmlendea/more-cultural-names-builder/security) tab and the [Actions](https://github.com/hmlendea/more-cultural-names-builder/actions/workflows/security.yml) workflow page.

## 🧾 Safe Harbour

If your investigation is conducted in good faith, confined to authorised scope, and disclosed responsibly, the maintainers will not pursue action for policy-compliant activity.

## 🙏 Recognition

We appreciate responsible disclosure. Reporters who desire public attribution may be acknowledged in release notes, advisories, or a dedicated acknowledgements section.