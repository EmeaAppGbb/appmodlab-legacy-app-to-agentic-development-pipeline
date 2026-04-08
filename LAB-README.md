# Legacy App to Agentic Development Pipeline - Lab Files

This repository contains the **Legacy App to Agentic Development Pipeline** capstone lab for the App Modernization GBB training series.

## What's in This Repository

This is a **demonstration legacy enterprise application** used to teach the Spec2Cloud → SQUAD modernization pipeline. The application is a realistic commercial property insurance system with:

- **2000+ lines** of complex business logic in the QuotingEngine
- **40+ rating factors** for premium calculation
- **50-state** regulatory compliance requirements
- **200+ stored procedures** in SQL Server
- **Multiple integration points** (WCF, MSMQ, Crystal Reports)

## Purpose

This codebase serves as the **starting point** for the capstone lab where participants:

1. Use **Spec2Cloud** to analyze and extract business rules
2. Configure **SQUAD** to work from the generated specifications
3. Have **SQUAD agents** modernize the application to .NET 9 and Azure
4. Deploy the cloud-native modernized application

## Documentation

See **[APPMODLAB.md](./APPMODLAB.md)** for the complete lab guide.

## Quick Links

- **Lab Instructions**: [APPMODLAB.md](./APPMODLAB.md)
- **Legacy App Setup**: [LEGACY-README.md](./LEGACY-README.md)
- **Database Schema**: [database/schema.sql](./database/schema.sql)

## Lab Structure

This repository uses branches to show progression:

- `main` - Lab documentation (this file and APPMODLAB.md)
- `legacy` - The complete ASP.NET MVC 4 legacy application (starting point)
- `specs` - Spec2Cloud-generated specifications
- `solution` - SQUAD-built modernized .NET 9 application
- `step-*` - Incremental lab steps

## Getting Started

To begin the lab:

```bash
# Clone this repository
git clone https://github.com/EmeaAppGbb/appmodlab-legacy-app-to-agentic-development-pipeline.git

# Read the lab guide
cd appmodlab-legacy-app-to-agentic-development-pipeline
cat APPMODLAB.md

# Checkout the legacy application
git checkout legacy
```

## Prerequisites

Before starting:
- Complete "Spec2Cloud Introduction" lab
- Complete "Getting Started with SQUAD" lab
- Have Visual Studio 2022 installed
- Have .NET Framework 4.6.1 SDK
- Have SQL Server (LocalDB or full instance)
- Have GitHub Copilot access

## Support

For issues or questions:
- Create an issue in this repository
- Contact: appmod-labs@microsoft.com

---

**Lab Difficulty**: Advanced  
**Estimated Time**: 8-12 hours  
**Category**: Capstone / End-to-End
