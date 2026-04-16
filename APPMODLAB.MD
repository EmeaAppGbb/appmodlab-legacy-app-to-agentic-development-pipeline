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

## Solution Steps (solution-final branch)

The `solution-final` branch contains the completed lab walkthrough with 6 steps,
each committed and tagged. Switch to it with `git checkout solution-final`.

### Step 01: Explore the Legacy Application
**Tag:** `step-01-explore-legacy-application`

Analyze the Keystone Insurance monolith: solution structure, MSBuild attempt (fails with
6 errors — 5 missing projects + .NET 4.6.1 SDK), architecture layers, technology stack,
and database schema.

**Output:** [assets/outputs/step-01-explore-legacy-application.txt](assets/outputs/step-01-explore-legacy-application.txt)

<details>
<summary>Key findings</summary>

- 2,913 lines across 27 source files
- 6 projects in .sln (only Core present with full source)
- QuotingEngine.cs (685 lines) = 23.5% of codebase
- 8 database tables, 200+ stored procedures
- Tech: .NET 4.6.1, MVC 4, EF5 EDMX, WCF, MSMQ, Crystal Reports, Windows Auth
</details>

### Step 02: Analyze Modernization Candidates
**Tag:** `step-02-modernization-candidates`

Score 9 components on complexity, agentic benefit, risk, and test coverage need.
Identify 5 repetitive patterns ideal for agent exploitation.

**Output:** [assets/outputs/step-02-modernization-candidates.txt](assets/outputs/step-02-modernization-candidates.txt)

<details>
<summary>Top 3 candidates</summary>

| Candidate | Agentic Benefit | Why |
|-----------|----------------|-----|
| QuotingEngine.cs (685 lines) | ★★★★★ | 40+ factors, 50 state rules — repetitive generation |
| UnderwritingService.cs (296 lines) | ★★★★★ | Testable boundaries, decision tree automation |
| ComplianceService.cs (201 lines) | ★★★★★ | 50 state variations — bulk config generation |
</details>

### Step 03: Design Agentic Pipeline
**Tag:** `step-03-agentic-pipeline`

Create YAML pipeline configuration (`agentic-pipeline.yaml`) with 8 stages from legacy
analysis through deployment, defining agent assignments and quality gates.

**Output:** [assets/outputs/step-03-agentic-pipeline-design.txt](assets/outputs/step-03-agentic-pipeline-design.txt)

<details>
<summary>Pipeline stages</summary>

1. Legacy Analysis (Spec2Cloud) — 30 min
2. Specification Generation (Spec2Cloud) — 45 min
3. Development Planning (SQUAD Brain) — 20 min
4. Core Service Implementation (SQUAD Hands) — 120 min
5. Integration Implementation (SQUAD Hands) — 60 min
6. Quality Review (SQUAD Eyes) — 30 min
7. Documentation (SQUAD Mouth) — 20 min
8. Deployment & Validation — 45 min
</details>

### Step 04: Configure Agent Specializations
**Tag:** `step-04-agent-specializations`

Define 10 specialized agent roles in `agent-specializations.yaml` with scope,
context requirements, task assignments, and success criteria.

**Output:** [assets/outputs/step-04-agent-specializations.txt](assets/outputs/step-04-agent-specializations.txt)

<details>
<summary>Agent roster</summary>

| Agent | Role | Specialization |
|-------|------|---------------|
| Spec2Cloud Analyzer | Reverse-engineer legacy code | Architecture, rules, data |
| Spec2Cloud Architect | Design modern architecture | Services, APIs, migration |
| SQUAD Brain | Development planning | Backlog, risks, phases |
| SQUAD Hands (Coder) | C#/.NET implementation | Business logic, refactoring |
| SQUAD Hands (Data) | Database migration | EF Core, schema, LINQ |
| SQUAD Hands (Infra) | Infrastructure | Bicep, CI/CD, containers |
| SQUAD Eyes (Reviewer) | Code review | Spec compliance, patterns |
| SQUAD Eyes (Security) | Security review | Auth, secrets, validation |
| SQUAD Eyes (QA) | Validation | Premium parity, E2E tests |
| SQUAD Mouth | Documentation | API docs, ADRs, guides |
</details>

### Step 05: Create Assessment Report
**Tag:** `step-05-assessment-report`

Generate comprehensive modernization assessment (`assessment-report.md`) covering
architecture, complexity hotspots, technology debt, and agentic development ROI.

**Output:** [assets/outputs/step-05-assessment-report.txt](assets/outputs/step-05-assessment-report.txt)

<details>
<summary>Assessment verdict</summary>

**STRONG CANDIDATE for agentic modernization.**
- Estimated effort reduction: 60-65% (2-3 weeks vs 6-8 weeks manual)
- Key driver: Repetitive patterns (50 state rules, 40+ rating factors)
- Top risk: Premium calculation parity — mitigated by golden test suite
</details>

### Step 06: Define Execution Plan
**Tag:** `step-06-execution-plan`

Create 5-phase execution plan (`execution-plan.md`) with agent assignments,
deliverables, success criteria, and 8-12 hour timeline.

**Output:** [assets/outputs/step-06-execution-plan.txt](assets/outputs/step-06-execution-plan.txt)

<details>
<summary>Phase timeline</summary>

| Phase | Duration | Focus |
|-------|----------|-------|
| 1. Analysis & Specs | 1.5 hours | Spec2Cloud reverse-engineering |
| 2. Foundation & Core | 4 hours | .NET 9 scaffold + QuotingEngine pipeline |
| 3. Integrations | 2 hours | Service Bus, REST, QuestPDF |
| 4. Quality & Docs | 2 hours | Review, security, parity tests, ADRs |
| 5. Deploy & Validate | 2 hours | Azure Container Apps + E2E |
</details>
