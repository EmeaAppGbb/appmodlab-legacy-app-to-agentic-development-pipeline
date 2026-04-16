---
title: "Legacy App to Agentic Pipeline"
description: "End-to-end modernization: reverse-engineer with Spec2Cloud, implement with SQUAD—capstone lab"
authors: ["marconsilva"]
category: "Agentic Software Development"
industry: "Financial Services"
services: ["Azure Container Apps", "Azure SQL Database", "Azure Service Bus", "Azure Blob Storage"]
languages: ["C#", ".NET"]
frameworks: ["ASP.NET Core", "Entity Framework Core", "Blazor"]
modernizationTools: []
agenticTools: ["Spec2Cloud", "SQUAD"]
tags: ["agentic-development", "spec2cloud", "squad", "end-to-end", "capstone"]
extensions: ["github.copilot"]
thumbnail: ""
video: ""
version: "1.0.0"
---

## Screenshots & Analysis

The following screenshots document the legacy Keystone Insurance codebase structure,
build behavior, and key complexity areas that make this an ideal modernization candidate.

| # | Screenshot | Description |
|---|-----------|-------------|
| 1 | [Solution Structure](assets/screenshots/01-solution-structure.md) | Full project tree with line counts — 2,913 lines across 24 files |
| 2 | [Build Output](assets/screenshots/02-build-output.md) | MSBuild failure showing 5 missing projects + .NET Framework 4.6.1 dependency |
| 3 | [QuotingEngine Complexity](assets/screenshots/03-quoting-engine-complexity.md) | 685-line premium calculator with 40+ rating factors across 50 states |
| 4 | [Legacy Technology Stack](assets/screenshots/04-legacy-technology-stack.md) | WCF, MSMQ, Crystal Reports, Windows Auth — full legacy-to-modern mapping |
| 5 | [Underwriting Workflow](assets/screenshots/05-underwriting-workflow.md) | Risk scoring algorithm and decision tree (Approve/Decline/Refer) |

### Solution Structure

The Keystone Insurance solution spans 2,913 lines across 24 files, organized as an ASP.NET MVC 4 monolith targeting .NET Framework 4.6.1.

[View: Solution Structure](assets/screenshots/01-solution-structure.md)

*Open the Markdown file to view the full project tree with line counts.*

### Build Output

MSBuild fails with 5 missing project references and a .NET Framework 4.6.1 dependency — confirming this app cannot be built without the legacy SDK.

[View: Build Output](assets/screenshots/02-build-output.md)

*Open the Markdown file to view the full build log and error details.*

### QuotingEngine Complexity

The 685-line premium calculator implements 40+ rating factors across all 50 US states — the most complex module and primary modernization challenge.

[View: QuotingEngine Complexity](assets/screenshots/03-quoting-engine-complexity.md)

*Open the Markdown file to view the detailed complexity analysis.*

### Legacy Technology Stack

The application relies on WCF, MSMQ, Crystal Reports, and Windows Authentication — all requiring migration to modern cloud-native equivalents.

[View: Legacy Technology Stack](assets/screenshots/04-legacy-technology-stack.md)

*Open the Markdown file to view the full legacy-to-modern technology mapping.*

### Underwriting Workflow

The risk scoring algorithm implements a decision tree with three outcomes (Approve, Decline, Refer) based on multi-factor analysis.

[View: Underwriting Workflow](assets/screenshots/05-underwriting-workflow.md)

*Open the Markdown file to view the complete workflow and decision tree.*

### Key Findings

- **App Type:** ASP.NET MVC 4 monolith on .NET Framework 4.6.1
- **Cannot Run:** Requires .NET Framework 4.6.1 Developer Pack, SQL Server LocalDB, IIS Express, and Visual Studio — this is intentionally a legacy demo app for the Spec2Cloud → SQUAD modernization pipeline
- **Core Complexity:** The `QuotingEngine.cs` (685 lines) contains 15-step premium calculation with state-specific compliance rules for all 50 US states (see [QuotingEngine Complexity](assets/screenshots/03-quoting-engine-complexity.md) for details)
- **Database:** 8 tables, 200+ stored procedures (referenced), 486 lines of SQL
