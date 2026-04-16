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

---

## Solution Walkthrough

The `solution-final` branch contains the complete end-to-end modernization pipeline executed using **GitHub Copilot CLI** and **Squad CLI**. Each step is tagged for easy navigation.

### Steps Completed

| Step | Tag | Tool | Description |
|------|-----|------|-------------|
| 01 | `step-01-explore-legacy-app` | Copilot CLI | Analyzed the legacy ASP.NET MVC 4 Keystone Insurance codebase — architecture, complexity areas, technology dependencies |
| 02 | `step-02-spec2cloud-analysis` | Copilot CLI | Generated 5 Spec2Cloud specification documents: architecture, API contracts, data model, business rules catalog, integration specs |
| 03 | `step-03-review-specifications` | Copilot CLI | Reviewed and validated specifications for completeness, consistency, and feasibility |
| 04 | `step-04-squad-setup` | Squad CLI | Initialized SQUAD workspace with `squad init` — agents, skills, workflows, identity, and GitHub Actions |
| 05 | `step-05-brain-planning` | Squad CLI + Copilot CLI | Hired team roles (`squad hire`) and created development plan from specifications |
| 06 | `step-06-core-services` | Copilot CLI | Built .NET 9 core services: QuotingEngine, PremiumCalculator, UnderwritingService, PolicyService, ComplianceService with EF Core and REST API controllers |
| 07 | `step-07-integrations` | Copilot CLI | Added Azure Service Bus messaging, HttpClient+Polly reinsurance client, regulatory reporting, QuestPDF document generation |
| 08 | `step-08-frontend` | Copilot CLI | Created Blazor Server pages for quote management, policy management, and underwriting review |
| 09 | `step-09-eyes-review` | Copilot CLI | Spec-based code review validating architecture compliance, API completeness, business rule fidelity |
| 10 | `step-10-mouth-documentation` | Copilot CLI | Generated deployment guide, API reference, migration guide, and project README |
| 11 | `step-11-deploy-validate` | Copilot CLI | Created Bicep IaC templates, Dockerfile, and GitHub Actions CI/CD for Azure Container Apps deployment |

### How to Navigate

```bash
# View the complete solution
git checkout solution-final

# Jump to a specific step
git checkout step-06-core-services

# View step diff
git diff step-05-brain-planning..step-06-core-services

# View all outputs
ls assets/outputs/
```

### Key Artifacts

- **specs/** — 6 Spec2Cloud specification documents (architecture, API, data model, business rules, integrations, development plan)
- **src/KeystoneInsurance.Modern/** — Complete .NET 9 modernized application with Blazor Server UI
- **infra/** — Azure Bicep templates for Container Apps, SQL, Service Bus, Storage, Redis, App Insights
- **docs/** — Deployment guide, API reference, migration guide
- **assets/outputs/** — Raw CLI output from every step
- **.squad/** — SQUAD team workspace with agents, skills, and workflows
- **.github/workflows/** — CI/CD and SQUAD automation workflows
