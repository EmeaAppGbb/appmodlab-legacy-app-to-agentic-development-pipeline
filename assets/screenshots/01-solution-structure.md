# Screenshot: Solution Structure

```
KeystoneInsurance.sln                        (.NET Framework 4.6.1 Solution)
│
├── KeystoneInsurance.Web/                   (ASP.NET MVC 4 Web App)
│   └── Web.config                           (IIS/WCF/MSMQ configuration)
│
├── KeystoneInsurance.Core/                  (Business Logic Library)
│   ├── Domain/
│   │   ├── Entities/
│   │   │   ├── Quote.cs                     (55 lines - 30+ fields)
│   │   │   ├── Policy.cs                    (53 lines)
│   │   │   ├── Property.cs                  (51 lines)
│   │   │   ├── Client.cs                    (36 lines)
│   │   │   ├── UnderwritingDecision.cs      (39 lines)
│   │   │   ├── Endorsement.cs               (30 lines)
│   │   │   ├── Coverage.cs                  (21 lines)
│   │   │   └── RateFactor.cs                (17 lines)
│   │   └── Rules/
│   │       ├── ComplianceRules.cs           (63 lines)
│   │       ├── UnderwritingRules.cs         (61 lines)
│   │       └── RatingRules.cs               (35 lines)
│   ├── Services/
│   │   ├── QuotingEngine.cs                 (685 lines - MAIN COMPLEXITY)
│   │   ├── UnderwritingService.cs           (296 lines)
│   │   ├── ComplianceService.cs             (201 lines)
│   │   ├── PolicyService.cs                 (149 lines)
│   │   ├── EndorsementService.cs            (132 lines)
│   │   ├── RenewalService.cs                (106 lines)
│   │   └── PremiumCalculator.cs             (56 lines)
│   ├── Integration/
│   │   ├── RegulatoryReporter.cs            (142 lines)
│   │   └── ReinsuranceClient.cs             (113 lines)
│   └── KeystoneInsurance.Core.csproj        (.NET Framework 4.6.1)
│
├── KeystoneInsurance.Data/                  (Referenced but MISSING)
├── KeystoneInsurance.Reports/               (Referenced but MISSING)
├── KeystoneInsurance.Messaging/             (Referenced but MISSING)
├── KeystoneInsurance.Tests/                 (Referenced but MISSING)
│
├── database/
│   ├── schema.sql                           (263 lines - 8 tables)
│   ├── seed-data.sql                        (65 lines)
│   └── stored-procedures.sql                (158 lines)
│
├── APPMODLAB.MD                             (Lab metadata)
├── README.md                                (Full lab guide)
├── LAB-README.md                            (Lab file index)
├── LEGACY-README.md                         (Legacy app quick-start)
└── SPEC2CLOUD.MD                            (Spec2Cloud metadata)
```

**Total Lines of Code:** 2,913 across 24 files
**Primary Language:** C# (.NET Framework 4.6.1)
**Key Challenge:** QuotingEngine.cs alone is 685 lines with 40+ rating factors
