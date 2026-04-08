---
title: "Legacy App to Agentic Development Pipeline"
description: "End-to-end capstone lab demonstrating Spec2Cloud analysis of a legacy insurance application and SQUAD-driven modernization to cloud-native .NET"
category: "Cross-Cutting"
priority: "P1"
languages: ["C#", ".NET", "Python"]
technologies: ["ASP.NET MVC", ".NET Framework", "Entity Framework", "Azure", "Copilot", "Spec2Cloud", "SQUAD"]
duration: "8-12 hours"
difficulty: "Advanced"
prerequisites: ["Spec2Cloud Introduction", "Getting Started with SQUAD", "C# and .NET experience"]
learning_objectives:
  - "Execute the complete Spec2Cloud → SQUAD pipeline on complex enterprise application"
  - "Reverse-engineer insurance domain business rules using Spec2Cloud"
  - "Configure SQUAD agents to work from formal specifications"
  - "Coordinate multi-agent implementation of domain-rich modernization"
  - "Validate modernized application against Spec2Cloud specifications"
tags: ["capstone", "spec2cloud", "squad", "modernization", "insurance", "enterprise", "agentic"]
---

# Legacy App to Agentic Development Pipeline

## Overview

This capstone lab demonstrates the complete journey of modernizing a legacy enterprise application using GitHub Copilot's agentic tools: **Spec2Cloud** for reverse engineering and **SQUAD** for AI-driven development. You'll work with a realistic commercial property insurance application that encodes 15 years of business rules, state-specific compliance requirements, and complex actuarial calculations.

The lab showcases the full power of the agentic application enablement pipeline:
1. **Spec2Cloud** analyzes the legacy ASP.NET MVC codebase and generates comprehensive specifications
2. **SQUAD Brain** creates a development plan from the specifications
3. **SQUAD Hands** implements the modernized .NET 9 application
4. **SQUAD Eyes** reviews code against specifications
5. **SQUAD Mouth** generates documentation

This is a real-world scenario that demonstrates how AI agents can tackle the most challenging aspect of modernization: understanding and preserving complex business logic while transitioning to modern architectures.

## Business Context: Keystone Insurance

**Keystone Insurance** is a commercial property insurer that has been operating since 2009. Their core quoting and policy management system is built on:
- ASP.NET MVC 4 on .NET Framework 4.6.1
- Entity Framework 5 (Database-First)
- 200+ SQL Server stored procedures
- WCF services for reinsurance integration
- Crystal Reports for policy documents
- MSMQ for async processing

The system handles:
- **Quote Generation**: Complex actuarial premium calculations with 40+ rating factors
- **Underwriting**: Risk assessment across 50 states with varying regulations
- **Policy Management**: Issuance, endorsements, cancellations, and renewals
- **Compliance**: State-specific regulatory requirements and reporting
- **Integration**: Reinsurance treaty management and regulatory submissions

The application contains extensive business logic that cannot be lost during modernization. State-specific rating rules, compliance requirements, and actuarial formulas represent years of domain expertise encoded in C# and SQL.

## Architecture: Legacy vs. Modern

### Legacy Architecture (ASP.NET MVC 4)
```
┌─────────────────────────────────────────────┐
│  ASP.NET MVC 4 Web Application              │
│  ├─ Controllers (Quote, Policy, UW)         │
│  ├─ Views (Razor + jQuery UI)               │
│  └─ Models (ViewModels)                     │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│  Business Logic Layer (.NET Framework)      │
│  ├─ QuotingEngine (2000+ lines)             │
│  ├─ UnderwritingService                     │
│  ├─ PolicyService                           │
│  ├─ ComplianceService (50-state rules)      │
│  └─ PremiumCalculator                       │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│  Data Access Layer (EF 5 Database-First)    │
│  ├─ KeystoneModel.edmx (100+ entities)      │
│  └─ 200+ Stored Procedures                  │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│  SQL Server 2012                             │
└──────────────────────────────────────────────┘

Integration Points:
├─ WCF Service → Reinsurance Partners
├─ Crystal Reports → Policy Documents
└─ MSMQ → Async Policy Processing
```

### Target Modern Architecture (.NET 9)
```
┌─────────────────────────────────────────────┐
│  Blazor Server / React Frontend             │
│  └─ Modern SPA with real-time updates       │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│  ASP.NET Core 9 Web API                     │
│  ├─ Controllers (REST endpoints)            │
│  ├─ Minimal APIs                            │
│  └─ SignalR (real-time updates)             │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│  Domain Services (.NET 9)                   │
│  ├─ QuotingEngine (refactored)              │
│  ├─ UnderwritingService                     │
│  ├─ PolicyService                           │
│  ├─ ComplianceService                       │
│  └─ Business Rules Engine (externalized)    │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│  Data Layer (EF Core 9 Code-First)          │
│  └─ Repository Pattern + Unit of Work       │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│  Azure SQL Database                         │
└──────────────────────────────────────────────┘

Cloud Services:
├─ Azure Service Bus → Async Processing
├─ QuestPDF → Policy Documents
├─ HttpClient + Polly → Partner APIs
├─ Azure Blob Storage → Documents
└─ Application Insights → Monitoring
```

## Lab Structure

This repository uses a branching strategy to show the progression:

| Branch | Purpose | Contents |
|--------|---------|----------|
| `main` | Lab documentation | This APPMODLAB.md file |
| `legacy` | Starting point | Complete ASP.NET MVC 4 application |
| `specs` | Spec2Cloud output | Generated specifications and architecture docs |
| `solution` | Final modernized app | SQUAD-built .NET 9 application |
| `step-1-spec2cloud-analysis` | Exercise 1 | Run Spec2Cloud analysis |
| `step-2-squad-setup` | Exercise 2 | Configure SQUAD, Brain creates plan |
| `step-3-core-services` | Exercise 3 | Hands builds core services |
| `step-4-integrations` | Exercise 4 | Hands builds integrations |
| `step-5-review-and-docs` | Exercise 5 | Eyes reviews, Mouth documents |

## Prerequisites

Before starting this lab, ensure you have:

- ✅ Completed **Spec2Cloud Introduction** lab
- ✅ Completed **Getting Started with SQUAD** lab
- ✅ Visual Studio 2022 (17.8 or later)
- ✅ .NET Framework 4.6.1 SDK (for legacy app)
- ✅ .NET 9 SDK (for modern app)
- ✅ SQL Server 2019+ (LocalDB or full instance)
- ✅ Azure subscription with Contributor access
- ✅ GitHub Copilot access
- ✅ Docker Desktop (for containerization)
- ✅ Git client
- ✅ Understanding of C# and ASP.NET
- ✅ Basic knowledge of insurance concepts (helpful but not required)

## Learning Objectives

By the end of this lab, you will be able to:

1. **Execute Spec2Cloud Analysis** - Run Spec2Cloud on a complex legacy codebase and interpret the generated specifications
2. **Understand Business Rules Extraction** - See how Spec2Cloud identifies and documents domain-specific business logic
3. **Configure SQUAD for Specification-Driven Development** - Set up SQUAD agents to work from Spec2Cloud output
4. **Coordinate Multi-Agent Development** - Use SQUAD Brain to create plans, Hands to implement, Eyes to review
5. **Preserve Business Logic During Modernization** - Ensure complex calculations and rules survive the transition
6. **Validate Against Specifications** - Use Spec2Cloud specs as acceptance criteria
7. **Deploy Modern Cloud-Native Application** - Take the modernized app to Azure Container Apps

---

## Exercise 1: Explore and Analyze the Legacy Application

**Duration:** 60-90 minutes

### 1.1 Clone and Set Up the Legacy Application

```bash
# Clone the repository
git clone https://github.com/EmeaAppGbb/appmodlab-legacy-app-to-agentic-development-pipeline.git
cd appmodlab-legacy-app-to-agentic-development-pipeline

# Switch to legacy branch
git checkout legacy

# Set up the database
sqlcmd -S localhost -i database\schema.sql
sqlcmd -S localhost -i database\seed-data.sql
sqlcmd -S localhost -i database\stored-procedures.sql
```

### 1.2 Explore the Codebase

Open `KeystoneInsurance.sln` in Visual Studio and explore:

- **KeystoneInsurance.Core** - Business logic layer
  - `Services\QuotingEngine.cs` - 2000+ line premium calculation engine
  - `Services\UnderwritingService.cs` - Risk assessment logic
  - `Services\ComplianceService.cs` - 50-state regulatory compliance
  
- **Database Schema** - 200+ stored procedures representing years of logic

**Key Areas to Understand:**
1. How does the `QuotingEngine` calculate premiums? (40+ factors)
2. What state-specific rules exist in `ComplianceService`?
3. How are underwriting decisions made in `UnderwritingService`?

### 1.3 Run Spec2Cloud Analysis

```bash
# Ensure you're in the legacy branch
git checkout legacy

# Run Spec2Cloud (this will take 10-15 minutes for a codebase this size)
spec2cloud analyze . --output ../specs --deep-analysis
```

Spec2Cloud will generate:
- ✅ **Architecture Overview** - High-level system design
- ✅ **Business Rules Catalog** - Extracted business logic
- ✅ **API Specifications** - REST API contracts
- ✅ **Data Models** - Entity definitions
- ✅ **Integration Specifications** - External system contracts
- ✅ **State Machine Diagrams** - Workflow representations

### 1.4 Review Spec2Cloud Output

```bash
# Switch to specs branch to see the generated specifications
git checkout specs

# Review key specification files
cat specs/architecture-overview.md
cat specs/business-rules-catalog.md
cat specs/api-specifications.yaml
```

**Questions to Answer:**
1. How many business rules did Spec2Cloud identify?
2. What are the key state-specific variations?
3. Which premium calculation factors are considered?

---

## Exercise 2: Configure SQUAD and Create Development Plan

**Duration:** 45-60 minutes

### 2.1 Initialize SQUAD

```bash
# Create new branch for modernization
git checkout -b modernization main

# Initialize SQUAD
squad init --specs ../specs --language csharp --target-framework net9.0
```

### 2.2 Configure SQUAD Brain with Specifications

Create `squad-config.yaml`:

```yaml
squad:
  name: "Keystone Modernization Team"
  specifications: "../specs"
  
agents:
  brain:
    role: "Architect and Planner"
    context:
      - "Spec2Cloud specifications in ../specs"
      - "Must preserve all business rules from legacy system"
      - "Target: .NET 9, Azure Container Apps"
      
  hands:
    roles:
      - name: "Core Services Developer"
        focus: "QuotingEngine, UnderwritingService, PolicyService"
      - name: "Integration Developer"
        focus: "Azure Service Bus, QuestPDF, HttpClient integrations"
      - name: "Frontend Developer"
        focus: "Blazor Server UI"
        
  eyes:
    role: "Code Reviewer"
    validation:
      - "Business rules match specs"
      - "Premium calculations are accurate"
      - "State-specific compliance preserved"
      
  mouth:
    role: "Technical Writer"
    outputs:
      - "API documentation"
      - "Deployment guides"
      - "Migration runbooks"
```

### 2.3 SQUAD Brain Creates Development Plan

```bash
# Ask Brain to create a plan from specifications
squad brain plan --from-specs ../specs
```

Brain will analyze the specifications and create a prioritized backlog:

**Expected Output:**
```
SQUAD BRAIN - Development Plan
================================

Phase 1: Foundation (Week 1-2)
- Set up .NET 9 solution structure
- Implement data layer with EF Core 9
- Migrate core domain entities
- Set up Azure SQL Database

Phase 2: Core Business Logic (Week 3-5)
- Migrate QuotingEngine with all 40+ rating factors
- Implement UnderwritingService with risk scoring
- Build PolicyService for lifecycle management
- Implement ComplianceService with state-specific rules
- CRITICAL: Validate premium calculations match legacy

Phase 3: Integration Layer (Week 6-7)
- Replace WCF with HttpClient + Polly
- Replace MSMQ with Azure Service Bus
- Replace Crystal Reports with QuestPDF
- Implement document generation

Phase 4: API Layer (Week 8)
- Build REST API controllers
- Implement authentication/authorization
- Add API documentation (Swagger)

Phase 5: Frontend (Week 9-10)
- Build Blazor Server UI
- Implement quote generation workflow
- Implement policy management screens

Phase 6: Deployment (Week 11-12)
- Containerize application
- Set up Azure Container Apps
- Implement CI/CD pipeline
- Migration and cutover
```

### 2.4 Review and Refine Plan

Use Brain interactively to refine the plan:

```bash
squad brain chat

> "Show me the state-specific compliance rules that must be migrated"
> "What's the complexity of the QuotingEngine migration?"
> "List all external integrations and their modern equivalents"
```

---

## Exercise 3: SQUAD Hands Implements Core Services

**Duration:** 2-3 hours (in a real project, this would be weeks)

### 3.1 Hands: Migrate QuotingEngine

```bash
# Assign task to Core Services Developer
squad hands assign "Migrate QuotingEngine from legacy to .NET 9" \
  --agent "Core Services Developer" \
  --specs "../specs/business-rules-catalog.md" \
  --reference-code "../legacy/KeystoneInsurance.Core/Services/QuotingEngine.cs"
```

The Hands agent will:
1. Analyze the legacy QuotingEngine (2000+ lines)
2. Reference the business rules catalog from Spec2Cloud
3. Implement modern version preserving all logic
4. Add unit tests to validate calculations

**Key Challenges:**
- 40+ rating factors must be preserved
- State-specific adjustments must be maintained
- Premium calculation must match legacy system exactly

### 3.2 Hands: Implement State Compliance Service

```bash
squad hands assign "Implement ComplianceService with 50-state rules" \
  --agent "Core Services Developer" \
  --specs "../specs/compliance-rules.md"
```

### 3.3 Validate Business Logic Preservation

```bash
# Run validation tests
squad hands validate --legacy-baseline ../legacy

# This compares premium calculations between legacy and modern
# for a set of test scenarios
```

**Expected Output:**
```
VALIDATION RESULTS
==================
✅ California quote - Frame construction: Match (Legacy: $8,245.50, Modern: $8,245.50)
✅ Florida coastal - Hurricane deductible: Match (Legacy: $12,875.25, Modern: $12,875.25)
✅ Texas hail exposure - High value: Match (Legacy: $15,234.75, Modern: $15,234.75)
❌ New York - Older building: MISMATCH (Legacy: $9,125.00, Modern: $9,124.98)
   
Total: 247 scenarios tested, 246 matches, 1 variance (<$1)
```

---

## Exercise 4: SQUAD Hands Implements Integrations

**Duration:** 90 minutes

### 4.1 Replace WCF with Modern HTTP Client

```bash
squad hands assign "Replace WCF ReinsuranceClient with HttpClient + Polly" \
  --agent "Integration Developer" \
  --specs "../specs/reinsurance-integration-spec.md"
```

### 4.2 Migrate to Azure Service Bus

```bash
squad hands assign "Replace MSMQ with Azure Service Bus for policy processing" \
  --agent "Integration Developer"
```

### 4.3 Implement Document Generation with QuestPDF

```bash
squad hands assign "Replace Crystal Reports with QuestPDF for policy documents" \
  --agent "Integration Developer" \
  --reference-legacy "../legacy/KeystoneInsurance.Reports"
```

---

## Exercise 5: SQUAD Eyes Reviews and SQUAD Mouth Documents

**Duration:** 60 minutes

### 5.1 SQUAD Eyes: Specification-Based Review

```bash
# Eyes performs code review against Spec2Cloud specifications
squad eyes review --against-specs ../specs

# Reviews focus on:
# 1. All business rules from specs are implemented
# 2. Premium calculations are accurate
# 3. State-specific compliance is preserved
# 4. API contracts match specifications
```

**Sample Review Output:**
```
SQUAD EYES - Code Review Report
================================

QuotingEngine.cs
✅ All 42 rating factors from spec implemented
✅ State-specific adjustments match spec
✅ Deductible credits match actuarial formulas
⚠️  Consider: Extracting state factors to configuration

ComplianceService.cs
✅ Florida building code requirements implemented
✅ California Prop 103 requirements implemented
⚠️  Missing: Louisiana coastal parish rules (Spec Section 4.2.3)

PolicyService.cs
✅ All policy lifecycle states from spec
✅ Cancellation logic matches spec
✅ Pro-rata calculations correct
```

### 5.2 SQUAD Mouth: Generate Documentation

```bash
# Mouth generates comprehensive documentation
squad mouth document --include-specs
```

Generated documentation includes:
- ✅ API reference with examples
- ✅ Deployment guide for Azure
- ✅ Migration runbook
- ✅ Business logic documentation
- ✅ Comparison: Legacy vs. Modern

---

## Exercise 6: Deploy to Azure

**Duration:** 60-90 minutes

### 6.1 Provision Azure Resources

```bash
# Deploy infrastructure using Bicep
az deployment group create \
  --resource-group rg-keystone-insurance \
  --template-file infrastructure/main.bicep \
  --parameters environment=prod
```

Resources created:
- Azure Container Apps (API + Workers)
- Azure SQL Database
- Azure Service Bus (Premium tier)
- Azure Blob Storage (for documents)
- Application Insights
- Azure Key Vault

### 6.2 Deploy Application

```bash
# Build and push container
docker build -t keystoneinsurance:latest .
az acr build --registry keystoneacr --image keystoneinsurance:latest .

# Deploy to Container Apps
az containerapp update \
  --name keystone-api \
  --resource-group rg-keystone-insurance \
  --image keystoneacr.azurecr.io/keystoneinsurance:latest
```

### 6.3 Smoke Test the Deployed Application

```bash
# Get the application URL
APP_URL=$(az containerapp show \
  --name keystone-api \
  --resource-group rg-keystone-insurance \
  --query properties.configuration.ingress.fqdn -o tsv)

# Test quote generation
curl -X POST https://$APP_URL/api/quotes \
  -H "Content-Type: application/json" \
  -d @test-data/sample-quote.json

# Expected: 200 OK with premium calculation
```

---

## Key Takeaways

### What You Learned

1. **Spec2Cloud is Essential for Complex Modernization**
   - Automated analysis of legacy code reveals business rules
   - Specifications serve as the contract between old and new
   - Domain knowledge is preserved in machine-readable format

2. **SQUAD Accelerates Development**
   - Brain creates realistic development plans from specs
   - Hands implements with business logic preservation
   - Eyes validates against specifications
   - Mouth documents the new system

3. **Business Logic Preservation is Achievable**
   - Premium calculations matched with <$1 variance
   - State-specific rules successfully migrated
   - Complex actuarial formulas preserved

4. **Cloud Migration is Simplified**
   - WCF → HttpClient: Simpler, more maintainable
   - MSMQ → Service Bus: More reliable, cloud-native
   - Crystal Reports → QuestPDF: Modern, containerizable

### Best Practices from This Lab

1. **Start with Spec2Cloud** - Always analyze before modernizing
2. **Use Specifications as Contract** - Validate modern app against specs
3. **Preserve Business Logic First** - Infrastructure can change, but business rules must survive
4. **Validate Continuously** - Test modern vs. legacy for critical calculations
5. **Leverage SQUAD for Coordination** - Let agents handle complexity

---

## Troubleshooting

### Common Issues

**Issue: Premium calculations don't match**
- Solution: Check rate factors are correctly migrated
- Use `squad eyes review --focus calculations` to find discrepancies

**Issue: State-specific rules failing**
- Solution: Review `specs/compliance-rules.md` for missing requirements
- Each state may have unique minimum premiums, deductible rules, etc.

**Issue: Spec2Cloud analysis incomplete**
- Solution: Ensure all legacy code is committed and accessible
- Use `--deep-analysis` flag for thorough analysis
- May take 15-20 minutes for large codebases

---

## Next Steps

After completing this lab:

1. **Try Your Own Legacy App** - Use Spec2Cloud + SQUAD on a real project
2. **Explore Advanced SQUAD Features** - Custom agent personas, parallel development
3. **Implement Full CI/CD** - Automated testing of spec compliance
4. **Add Observability** - Application Insights, custom metrics

---

## Additional Resources

- [Spec2Cloud Documentation](https://docs.github.com/copilot/spec2cloud)
- [SQUAD Guide](https://docs.github.com/copilot/squad)
- [Insurance Domain Reference](./docs/insurance-domain-guide.md)
- [Legacy Code Analysis Best Practices](./docs/analysis-best-practices.md)

---

## Feedback and Contributions

This lab is maintained by the GBB App Innovation team. For issues or suggestions:
- Create an issue in this repository
- Contact: appmod-labs@microsoft.com

---

**Lab Version:** 1.0  
**Last Updated:** 2024  
**Estimated Time:** 8-12 hours  
**Difficulty:** Advanced
