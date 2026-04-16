# Keystone Insurance — Development Plan

> **Version:** 1.0  
> **Created:** 2026-04-16  
> **Target:** .NET 9 · Azure Container Apps · Azure SQL Database  
> **Methodology:** 2-week sprints, SQUAD agent-based parallel development

---

## 1. Executive Summary

This plan covers the full modernization of the Keystone Insurance commercial property platform from .NET Framework 4.6.1 to .NET 9, organized into **6 sprints** (12 weeks). Work is distributed across three SQUAD agents — **Backend**, **Frontend** (API layer in this context, since the app is API-first), and **DevOps** — to maximize parallel execution while respecting dependencies.

---

## 2. SQUAD Agent Assignments

### 2.1 Backend Agent — Core Services & Business Logic

| Area | Responsibilities |
|---|---|
| Domain Layer | Entities, value objects, enums, domain rules (`KeystoneInsurance.Domain`) |
| Business Logic | QuotingEngine, PremiumCalculator, UnderwritingService, ComplianceService, EndorsementService, RenewalService, PolicyService (`KeystoneInsurance.Business`) |
| Application Layer | CQRS commands/queries with MediatR handlers (`KeystoneInsurance.Application`) |
| Data Access | EF Core DbContext, Fluent API configurations, migrations, audit interceptor (`KeystoneInsurance.Infrastructure.Persistence`) |
| Unit Tests | Domain tests, business logic tests (`KeystoneInsurance.Domain.Tests`, `KeystoneInsurance.Business.Tests`) |

### 2.2 Frontend Agent — API Layer & Contracts

| Area | Responsibilities |
|---|---|
| API Endpoints | Minimal API endpoints for Quotes, Policies, Underwriting, Endorsements, Renewals, Reports (`KeystoneInsurance.Api`) |
| Middleware | Exception handling, correlation ID middleware |
| Contracts | Request/response DTOs, event contracts (`KeystoneInsurance.Contracts`) |
| Auth & AuthZ | Entra ID JWT bearer configuration, role-based authorization policies |
| API Tests | Endpoint integration tests (`KeystoneInsurance.Api.Tests`) |
| Health Checks | `/health/ready` and `/health/live` endpoints |

### 2.3 DevOps Agent — Infrastructure & Integrations

| Area | Responsibilities |
|---|---|
| Azure IaC | Bicep templates for Container Apps, Azure SQL, Service Bus, Blob Storage, Key Vault |
| CI/CD | GitHub Actions workflows (build, test, deploy) |
| Containerization | Multi-stage Dockerfile |
| Messaging | Azure Service Bus publisher/consumer, background workers (`KeystoneInsurance.Infrastructure.Messaging`) |
| External Integrations | Reinsurance API client, Regulatory Reporting client with Polly resilience (`KeystoneInsurance.Infrastructure.Integration`) |
| Document Generation | QuestPDF implementation, Blob Storage service (`KeystoneInsurance.Infrastructure.Documents`, `KeystoneInsurance.Infrastructure.Storage`) |
| Observability | OpenTelemetry, Application Insights, structured logging configuration |
| Integration Tests | End-to-end integration tests (`KeystoneInsurance.Integration.Tests`) |

---

## 3. Sprint Breakdown

### Sprint 1 — Foundation (Weeks 1–2)

**Goal:** Solution scaffold, domain model, database layer, CI pipeline.

| ID | Work Item | Agent | Story Points | Priority |
|---|---|---|---|---|
| S1-01 | Create .NET 9 solution structure (6 projects + 4 test projects) | DevOps | 3 | P0 |
| S1-02 | Configure `Directory.Build.props`, NuGet packages, global usings | DevOps | 2 | P0 |
| S1-03 | Implement domain entities: Client, Quote, Policy, Endorsement, UnderwritingDecision, RateFactor, CoverageOption, AuditLog | Backend | 8 | P0 |
| S1-04 | Implement value objects: Money, Address, PolicyNumber | Backend | 3 | P0 |
| S1-05 | Implement enums: QuoteStatus, PolicyStatus, ConstructionType, OccupancyType, EndorsementType, UnderwritingDecisionType | Backend | 2 | P0 |
| S1-06 | Implement KeystoneDbContext with all DbSets | Backend | 3 | P0 |
| S1-07 | Implement all Fluent API configurations (Client, Quote, Policy, Endorsement, UnderwritingDecision, RateFactor, CoverageOption, AuditLog) | Backend | 5 | P0 |
| S1-08 | Implement AuditSaveChangesInterceptor | Backend | 3 | P1 |
| S1-09 | Generate initial EF Core migration | Backend | 2 | P0 |
| S1-10 | Create request/response DTOs for Quotes API | Frontend | 5 | P0 |
| S1-11 | Create request/response DTOs for Policies API | Frontend | 3 | P0 |
| S1-12 | Create shared event contracts (PolicyIssuedMessage, EndorsementRequestedMessage, RenewalDueMessage) | Frontend | 3 | P0 |
| S1-13 | Create Dockerfile (multi-stage build) | DevOps | 3 | P1 |
| S1-14 | Create GitHub Actions CI workflow (build + test) | DevOps | 5 | P1 |
| S1-15 | Write domain entity unit tests | Backend | 5 | P0 |

**Sprint 1 Dependencies:**
```
S1-03 → S1-06 → S1-07 → S1-09  (entities before DbContext before config before migration)
S1-01 → all other items            (solution scaffold first)
S1-03 → S1-10, S1-11              (entities inform DTO design)
```

---

### Sprint 2 — Core Business Logic (Weeks 3–4)

**Goal:** Premium calculation engine, underwriting rules, compliance validation.

| ID | Work Item | Agent | Story Points | Priority |
|---|---|---|---|---|
| S2-01 | Implement QuotingEngine — base rate determination (20 states, 6 construction multipliers) | Backend | 8 | P0 |
| S2-02 | Implement QuotingEngine — all rating factors (property value, construction, age, occupancy, protection, territory, catastrophe, roof, sqft, stories, loss history, deductible credit) | Backend | 13 | P0 |
| S2-03 | Implement QuotingEngine — optional coverage premiums (BI, equipment, flood, earthquake) | Backend | 5 | P0 |
| S2-04 | Implement QuotingEngine — state adjustments, surcharges/taxes, minimums | Backend | 5 | P0 |
| S2-05 | Implement QuotingEngine — quote validation rules (QE-200 through QE-208) | Backend | 3 | P0 |
| S2-06 | Implement PremiumCalculator — prorated premium, return premium (ProRata/ShortRate/Flat), installment calculations | Backend | 5 | P0 |
| S2-07 | Implement UnderwritingService — risk score calculation (19 criteria, base 50, clamp 0–100) | Backend | 8 | P0 |
| S2-08 | Implement UnderwritingService — decision thresholds, PML calculation, component ratings | Backend | 5 | P0 |
| S2-09 | Implement UnderwritingRules — auto-decline, senior referral, property inspection rules | Backend | 3 | P0 |
| S2-10 | Implement ComplianceService — CA, FL, TX, NY, LA, general state rules | Backend | 8 | P0 |
| S2-11 | Implement ComplianceRules — flood disclosure, earthquake offer, cancellation notice days | Backend | 3 | P1 |
| S2-12 | Create DTOs for Underwriting, Endorsements, Renewals, Reports APIs | Frontend | 5 | P0 |
| S2-13 | Implement RFC 7807 error response model and validation error formatting | Frontend | 3 | P0 |
| S2-14 | Begin Bicep templates — Resource Group, Azure SQL, Container Apps Environment | DevOps | 8 | P1 |
| S2-15 | Write comprehensive unit tests for QuotingEngine (all factor calculations) | Backend | 8 | P0 |
| S2-16 | Write unit tests for UnderwritingService (all decision paths) | Backend | 5 | P0 |
| S2-17 | Write unit tests for ComplianceService (all state rules) | Backend | 5 | P0 |

**Sprint 2 Dependencies:**
```
S1-03, S1-05 → S2-01              (entities and enums before business logic)
S2-01 → S2-02 → S2-03 → S2-04    (rating engine built incrementally)
S2-01 → S2-06                      (calculator depends on quoting engine patterns)
S1-03 → S2-07                      (entities before underwriting)
S2-02 → S2-15                      (implement before testing)
```

---

### Sprint 3 — API Layer & Application Services (Weeks 5–6)

**Goal:** CQRS handlers, API endpoints, authentication, endorsement & renewal logic.

| ID | Work Item | Agent | Story Points | Priority |
|---|---|---|---|---|
| S3-01 | Implement CQRS handlers — Quotes (CreateQuoteCommand, RecalculateQuoteCommand, GetQuoteByIdQuery, SearchQuotesQuery) | Backend | 8 | P0 |
| S3-02 | Implement CQRS handlers — Policies (IssuePolicyCommand, GetPolicyByIdQuery, CancelPolicyCommand) | Backend | 5 | P0 |
| S3-03 | Implement CQRS handlers — Underwriting (EvaluateQuoteCommand) | Backend | 3 | P0 |
| S3-04 | Implement EndorsementService — coverage change, deductible change, cancellation endorsement | Backend | 8 | P0 |
| S3-05 | Implement RenewalService — renewal quote generation, trend factors, inflation adjustments | Backend | 8 | P0 |
| S3-06 | Implement PolicyService — issue policy, cancel policy, reinsurance cession logic | Backend | 5 | P0 |
| S3-07 | Implement Quotes API endpoints (POST, GET by ID, GET search, PUT recalculate) | Frontend | 8 | P0 |
| S3-08 | Implement Policies API endpoints (POST issue, GET by ID, POST cancel) | Frontend | 5 | P0 |
| S3-09 | Implement Underwriting API endpoint (POST evaluate) | Frontend | 3 | P0 |
| S3-10 | Implement Endorsements API endpoints (POST coverage-change, POST deductible-change, POST cancellation, GET list) | Frontend | 5 | P0 |
| S3-11 | Implement Renewals API endpoints (POST generate, GET expiring) | Frontend | 3 | P0 |
| S3-12 | Implement Reports API endpoint (GET premium-summary) | Frontend | 3 | P1 |
| S3-13 | Configure Entra ID JWT bearer authentication in Program.cs | Frontend | 5 | P0 |
| S3-14 | Implement role-based authorization policies (Agent, Underwriter, SeniorUnderwriter, Admin) | Frontend | 3 | P0 |
| S3-15 | Implement ExceptionHandlingMiddleware (RFC 7807 responses) | Frontend | 3 | P0 |
| S3-16 | Implement CorrelationIdMiddleware | Frontend | 2 | P1 |
| S3-17 | Write unit tests for EndorsementService and RenewalService | Backend | 5 | P0 |
| S3-18 | Write unit tests for PolicyService | Backend | 3 | P0 |

**Sprint 3 Dependencies:**
```
S2-01..S2-06 → S3-01              (business logic before CQRS handlers)
S2-07..S2-09 → S3-03              (underwriting logic before handler)
S1-10..S1-12, S2-12 → S3-07..S3-12 (DTOs before endpoints)
S3-01..S3-06 → S3-07..S3-12       (handlers before endpoints)
S3-13 → S3-14                      (auth before authz)
```

---

### Sprint 4 — Integrations & Messaging (Weeks 7–8)

**Goal:** Service Bus messaging, external API clients, PDF generation, blob storage.

| ID | Work Item | Agent | Story Points | Priority |
|---|---|---|---|---|
| S4-01 | Implement IMessagePublisher / ServiceBusPublisher (policy-issued, endorsement-requested, renewal-due, compliance-events) | DevOps | 8 | P0 |
| S4-02 | Implement PolicyDocumentWorker (BackgroundService consuming policy-issued queue) | DevOps | 5 | P0 |
| S4-03 | Implement EndorsementProcessorWorker (BackgroundService consuming endorsement-requested queue) | DevOps | 5 | P0 |
| S4-04 | Implement RenewalProcessorWorker (BackgroundService consuming renewal-due queue) | DevOps | 5 | P1 |
| S4-05 | Implement QuestPDF PolicyDocumentGenerator — policy declaration, endorsement document, quote summary, renewal offer | DevOps | 13 | P0 |
| S4-06 | Implement BlobStorageService — upload, download, container management | DevOps | 5 | P0 |
| S4-07 | Implement ReinsuranceApiClient with HttpClient + Polly (retry, circuit breaker, timeout) | DevOps | 5 | P0 |
| S4-08 | Implement RegulatoryReportingClient with HttpClient + Polly | DevOps | 5 | P1 |
| S4-09 | Wire up message publishing in PolicyService.IssuePolicy() and EndorsementService | Backend | 3 | P0 |
| S4-10 | Wire up reinsurance cession in PolicyService for properties > $2M | Backend | 3 | P0 |
| S4-11 | Implement pagination helpers (PagedResult<T>) for all list endpoints | Frontend | 3 | P0 |
| S4-12 | Bicep templates — Service Bus (queues + topics), Blob Storage (containers), Key Vault | DevOps | 8 | P1 |
| S4-13 | Write unit tests for ServiceBusPublisher (mocked ServiceBusClient) | DevOps | 3 | P0 |
| S4-14 | Write unit tests for QuestPDF document generation | DevOps | 5 | P0 |

**Sprint 4 Dependencies:**
```
S1-12 → S4-01                      (event contracts before publisher)
S4-01 → S4-02, S4-03, S4-04       (publisher before consumers)
S4-06 → S4-05                      (blob storage before doc generator uses it)
S3-06 → S4-09, S4-10              (PolicyService before wiring messaging/reinsurance)
```

---

### Sprint 5 — Observability, Testing & Hardening (Weeks 9–10)

**Goal:** Comprehensive testing, observability, health checks, security hardening.

| ID | Work Item | Agent | Story Points | Priority |
|---|---|---|---|---|
| S5-01 | Configure OpenTelemetry — distributed tracing to Application Insights | DevOps | 5 | P0 |
| S5-02 | Configure structured logging — Serilog with Application Insights sink | DevOps | 3 | P0 |
| S5-03 | Add custom metrics — premium calculation duration, UW decision counts | DevOps | 3 | P1 |
| S5-04 | Implement health check endpoints (`/health/ready`, `/health/live`) — DB, Service Bus, Blob Storage probes | Frontend | 5 | P0 |
| S5-05 | Write API integration tests — full quote-to-policy lifecycle | Frontend | 8 | P0 |
| S5-06 | Write API integration tests — underwriting decision paths (all 4 outcomes) | Frontend | 5 | P0 |
| S5-07 | Write API integration tests — endorsement workflows (coverage, deductible, cancellation) | Frontend | 5 | P0 |
| S5-08 | Write API integration tests — renewal generation with trend factors | Frontend | 3 | P0 |
| S5-09 | Write API integration tests — authentication and authorization (role-based access) | Frontend | 5 | P0 |
| S5-10 | Write integration tests — Service Bus message publish/consume round-trip | DevOps | 5 | P1 |
| S5-11 | Write integration tests — external API clients with WireMock/mock servers | DevOps | 5 | P1 |
| S5-12 | Security review — input validation, SQL injection (parameterized queries via EF), JWT validation | Backend | 5 | P0 |
| S5-13 | Performance testing — premium calculation benchmarks, DB query performance | Backend | 3 | P1 |
| S5-14 | Implement rate limiting on API endpoints | Frontend | 3 | P1 |
| S5-15 | Add Swagger/OpenAPI documentation (Swashbuckle or NSwag) | Frontend | 3 | P1 |

**Sprint 5 Dependencies:**
```
S3-07..S3-12 → S5-05..S5-09       (endpoints before API tests)
S4-01..S4-04 → S5-10              (messaging before messaging tests)
S4-07, S4-08 → S5-11              (clients before client tests)
S5-01 → S5-03                      (telemetry config before custom metrics)
```

---

### Sprint 6 — Deployment, IaC & Go-Live Prep (Weeks 11–12)

**Goal:** Production deployment pipeline, IaC, environment configuration, documentation.

| ID | Work Item | Agent | Story Points | Priority |
|---|---|---|---|---|
| S6-01 | Finalize Bicep templates — complete environment (dev/staging/prod parameters) | DevOps | 8 | P0 |
| S6-02 | Configure Managed Identity for Azure SQL, Service Bus, Blob Storage, Key Vault | DevOps | 5 | P0 |
| S6-03 | GitHub Actions CD workflow — build container → push ACR → deploy Container Apps revision | DevOps | 8 | P0 |
| S6-04 | Configure environment-specific `appsettings.{env}.json` files | DevOps | 3 | P0 |
| S6-05 | Set up Azure Monitor alerts — error rate, latency P99, DLQ depth | DevOps | 5 | P1 |
| S6-06 | Database migration strategy — EF Core migration on deployment | DevOps | 3 | P0 |
| S6-07 | Seed data — reference data for RateFactors, CoverageOptions | Backend | 5 | P0 |
| S6-08 | End-to-end smoke test suite (post-deployment validation) | Frontend | 5 | P0 |
| S6-09 | API documentation finalization | Frontend | 3 | P1 |
| S6-10 | Update README.md with architecture overview, getting started guide, API reference | Frontend | 3 | P1 |
| S6-11 | Load testing — Container Apps scaling validation (50 concurrent requests trigger) | DevOps | 5 | P1 |
| S6-12 | Disaster recovery runbook — DB PITR, Service Bus geo-recovery | DevOps | 3 | P2 |

**Sprint 6 Dependencies:**
```
S2-14, S4-12 → S6-01              (prior Bicep work before finalization)
S6-01 → S6-02 → S6-03             (IaC before identity before CD)
S1-09 → S6-06                      (migrations before deployment strategy)
S6-03 → S6-08                      (deployment before smoke tests)
```

---

## 4. Implementation Priority Order

The critical path follows this sequence:

```
Phase 1 (Sprint 1)        Phase 2 (Sprint 2)           Phase 3 (Sprint 3)
┌──────────────────┐      ┌──────────────────────┐      ┌──────────────────────┐
│ Solution scaffold │─────▶│ QuotingEngine        │─────▶│ CQRS Handlers        │
│ Domain entities   │      │ UnderwritingService  │      │ API Endpoints        │
│ EF Core DbContext │      │ ComplianceService    │      │ Auth & AuthZ         │
│ DTOs & contracts  │      │ PremiumCalculator    │      │ Endorsement/Renewal  │
└──────────────────┘      └──────────────────────┘      └──────────────────────┘
                                                                  │
Phase 6 (Sprint 6)        Phase 5 (Sprint 5)           Phase 4 (Sprint 4)
┌──────────────────┐      ┌──────────────────────┐      ┌──────────────────────┐
│ IaC finalization  │◀─────│ Integration tests    │◀─────│ Service Bus messaging│
│ CD pipeline       │      │ Observability        │      │ QuestPDF documents   │
│ Go-live prep      │      │ Security hardening   │      │ External API clients │
└──────────────────┘      └──────────────────────┘      └──────────────────────┘
```

### Absolute Priority Order (P0 Critical Path)

1. **Solution scaffold & project structure** — everything depends on this
2. **Domain entities & enums** — foundation for all business logic
3. **EF Core DbContext & configurations** — data layer enables testing
4. **QuotingEngine & PremiumCalculator** — core business value
5. **UnderwritingService & ComplianceService** — required for quote lifecycle
6. **Request/Response DTOs** — API contract definition
7. **CQRS command/query handlers** — connects API to business logic
8. **API endpoints** — exposes functionality
9. **Authentication & authorization** — secures the API
10. **Service Bus messaging** — enables async workflows
11. **QuestPDF document generation** — policy issuance completion
12. **Integration tests** — validates end-to-end flows
13. **CI/CD pipeline & IaC** — production deployment

---

## 5. Dependency Graph

```
S1-01 (Solution scaffold)
  ├── S1-03 (Domain entities)
  │     ├── S1-04 (Value objects)
  │     ├── S1-05 (Enums)
  │     ├── S1-06 (DbContext)
  │     │     ├── S1-07 (Fluent API configs)
  │     │     │     ├── S1-08 (Audit interceptor)
  │     │     │     └── S1-09 (Initial migration)
  │     │     │           └── S6-06 (Migration strategy)
  │     │     └── S6-07 (Seed data)
  │     ├── S1-10 (Quote DTOs)
  │     ├── S1-11 (Policy DTOs)
  │     ├── S1-12 (Event contracts)
  │     │     └── S4-01 (Message publisher)
  │     │           ├── S4-02 (Policy doc worker)
  │     │           ├── S4-03 (Endorsement worker)
  │     │           └── S4-04 (Renewal worker)
  │     ├── S1-15 (Domain tests)
  │     ├── S2-01..S2-06 (QuotingEngine, PremiumCalculator)
  │     │     ├── S2-15..S2-17 (Business logic tests)
  │     │     ├── S3-01 (Quote CQRS handlers)
  │     │     ├── S3-02 (Policy CQRS handlers)
  │     │     └── S3-04..S3-06 (Endorsement, Renewal, Policy services)
  │     │           ├── S3-17, S3-18 (Service tests)
  │     │           ├── S4-09, S4-10 (Wire messaging & reinsurance)
  │     │           └── S3-07..S3-12 (API endpoints)
  │     │                 ├── S3-13 (JWT auth)
  │     │                 │     └── S3-14 (AuthZ policies)
  │     │                 ├── S3-15 (Exception middleware)
  │     │                 ├── S5-04 (Health checks)
  │     │                 └── S5-05..S5-09 (API integration tests)
  │     └── S2-07..S2-11 (UnderwritingService, ComplianceService)
  │           └── S3-03 (Underwriting CQRS handler)
  │                 └── S3-09 (Underwriting endpoint)
  ├── S1-13 (Dockerfile)
  ├── S1-14 (CI workflow)
  │     └── S6-03 (CD workflow)
  └── S2-14 (Bicep templates begin)
        └── S4-12 (Bicep: Service Bus, Blob, KV)
              └── S6-01 (Bicep finalization)
                    └── S6-02 (Managed Identity)
                          └── S6-03 (CD pipeline)
```

---

## 6. Testing Strategy

### 6.1 Test Pyramid

```
           ┌─────────────────┐
           │  E2E Smoke Tests │  (~10 tests)
           │  (Post-deploy)   │
           ├─────────────────┤
          │ Integration Tests  │  (~40 tests)
          │ (WebApplicationFactory, │
          │  TestContainers)   │
          ├───────────────────┤
        │   Unit Tests           │  (~200+ tests)
        │   (Domain, Business,   │
        │    Application)        │
        └───────────────────────┘
```

### 6.2 Unit Tests (Backend Agent)

| Test Suite | Scope | Key Scenarios | Target Count |
|---|---|---|---|
| `KeystoneInsurance.Domain.Tests` | Entity validation, value objects | Client/Quote/Policy construction, PolicyNumber format, Money arithmetic | 30+ |
| `KeystoneInsurance.Business.Tests` | Business logic | Premium calculation (all factors), UW decision paths, compliance rules, endorsement calculations, renewal factors | 150+ |

**Critical unit test scenarios:**

- **QuotingEngine:** All 20 state base rates, 6 construction multipliers, 7 property value tiers, 8 building age ranges, 11 occupancy types, protection combinations, territory/catastrophe zones, 6 roof types + age surcharges, square footage tiers, story factors, loss history factors, deductible credits, optional coverages, state adjustments, surcharges/taxes, minimum premiums
- **UnderwritingService:** Risk score from all 19 criteria, 4 decision outcomes, PML calculation per state, 5 component ratings, auto-decline rules, senior referral triggers, missing info triggers
- **ComplianceService:** All rules per state (CA:3, FL:4, TX:2, NY:2, LA:2, General:2)
- **PremiumCalculator:** ProRata/ShortRate/Flat cancellation, 4 installment plan calculations
- **EndorsementService:** Coverage change pro-rata, deductible change factor, cancellation endorsement
- **RenewalService:** Inflation adjustment, 5 state trend factors, roof age increment

### 6.3 Integration Tests (Frontend Agent)

| Test Suite | Scope | Key Scenarios | Target Count |
|---|---|---|---|
| `KeystoneInsurance.Api.Tests` | API endpoints with in-memory DB | Full quote-to-policy lifecycle, authorization by role, validation errors, pagination | 40+ |

**Critical integration test scenarios:**

- **Quote lifecycle:** Create → Recalculate → UW Evaluate → Issue Policy
- **Underwriting paths:** Approved (score ≤ 60), Declined (score > 85), ReferToSenior (score > 70 or claims ≥ 3), RequestMoreInfo (score > 60)
- **Endorsement workflows:** Coverage increase/decrease, deductible change, cancellation (ShortRate vs ProRata)
- **Renewal flow:** Generate renewal for expiring policy with state-specific trend factors
- **Authorization:** Agent can create quotes but cannot evaluate underwriting; Underwriter can evaluate; Admin has full access
- **Error handling:** 422 for validation errors, 400 for compliance failures, 404 for missing resources, 401/403 for auth issues

### 6.4 Integration Tests (DevOps Agent)

| Test Suite | Scope | Key Scenarios | Target Count |
|---|---|---|---|
| `KeystoneInsurance.Integration.Tests` | External dependencies | Service Bus round-trip, HTTP client resilience, Blob Storage upload/download | 15+ |

### 6.5 E2E Smoke Tests (Sprint 6)

Post-deployment validation against staging/production:
- Health check endpoints respond 200
- Create a quote and verify premium calculation
- Issue a policy and verify document generation
- Verify Service Bus message delivery

### 6.6 Test Infrastructure

| Tool | Purpose |
|---|---|
| xUnit | Test framework |
| FluentAssertions | Assertion library |
| NSubstitute or Moq | Mocking framework |
| WebApplicationFactory | API integration testing |
| TestContainers | Integration tests with real SQL, Service Bus |
| WireMock.Net | Mock external HTTP services (reinsurance, regulatory) |
| Bogus | Test data generation |

---

## 7. Risk Assessment

### 7.1 High Risk

| Risk | Impact | Likelihood | Mitigation |
|---|---|---|---|
| **Premium calculation accuracy** — 15+ rating factors with complex interactions; incorrect premium = financial loss + regulatory issues | Critical | Medium | Extensive unit tests for every factor combination; validate against legacy system test cases; property-based testing for edge cases |
| **EF Core migration from EDMX** — 200+ stored procedures being retired; subtle query differences between SQL Server procs and LINQ | High | Medium | Map each stored procedure to equivalent LINQ query; compare query results against legacy DB; use SQL Profiler to verify generated SQL |
| **Compliance rule correctness** — State-specific regulations (CA Prop 103, FL coastal, TX wind/hail); non-compliance = legal liability | Critical | Medium | Unit test every compliance rule with boundary conditions; engage domain experts for rule validation; create compliance test matrix |

### 7.2 Medium Risk

| Risk | Impact | Likelihood | Mitigation |
|---|---|---|---|
| **Azure Service Bus reliability** — Message loss during policy issuance = missing documents | High | Low | Dead-letter queue monitoring; idempotent message processing; Azure Monitor alerts on DLQ depth; manual retry tooling |
| **External API availability** — Reinsurance/regulatory APIs may be unreliable | Medium | Medium | Polly resilience (retry + circuit breaker); graceful degradation (queue for later); fallback to manual process |
| **Entra ID integration complexity** — Role mapping from Windows Auth to JWT claims | Medium | Medium | Early spike in Sprint 3; test with multiple role configurations; document role mapping for IT admins |
| **QuestPDF document fidelity** — Crystal Reports replacement must match business expectations | Medium | Medium | Create sample documents early (Sprint 4); get business sign-off on layout; version document templates |

### 7.3 Low Risk

| Risk | Impact | Likelihood | Mitigation |
|---|---|---|---|
| **Container Apps scaling** — May not handle burst traffic | Medium | Low | Load test in Sprint 6; configure scaling rules (50 concurrent requests); monitor with Azure Monitor |
| **Azure SQL serverless cold start** — Auto-pause may cause latency spikes | Low | Medium | Set minimum replicas to 1; configure auto-pause at 1 hour; health check keeps connection warm |
| **NuGet package compatibility** — .NET 9 compatibility for all packages (QuestPDF, Polly, Azure SDKs) | Low | Low | Verify compatibility in Sprint 1 scaffold; pin known-good versions |

### 7.4 Risk Mitigation Schedule

| Sprint | Risk Focus |
|---|---|
| Sprint 1 | NuGet compatibility verification, EF Core migration spike |
| Sprint 2 | Premium calculation accuracy (exhaustive unit tests) |
| Sprint 3 | Entra ID integration spike, compliance rule validation |
| Sprint 4 | Service Bus reliability testing, QuestPDF document review |
| Sprint 5 | End-to-end integration validation, security review |
| Sprint 6 | Load testing, disaster recovery validation |

---

## 8. Definition of Done

Each work item is complete when:

1. ✅ Code implements all specified business rules from the spec documents
2. ✅ Unit tests pass with ≥ 90% code coverage on business logic
3. ✅ Integration tests pass for API endpoints
4. ✅ No compiler warnings
5. ✅ Code follows C# conventions (nullable reference types enabled, async/await patterns)
6. ✅ XML documentation on public APIs
7. ✅ PR reviewed and approved
8. ✅ CI pipeline passes (build + test)

---

## 9. Sprint Velocity Targets

| Sprint | Backend SP | Frontend SP | DevOps SP | Total SP |
|---|---|---|---|---|
| Sprint 1 | 31 | 11 | 13 | 55 |
| Sprint 2 | 71 | 8 | 8 | 87 |
| Sprint 3 | 40 | 40 | 0 | 80 |
| Sprint 4 | 6 | 3 | 54 | 63 |
| Sprint 5 | 8 | 34 | 21 | 63 |
| Sprint 6 | 5 | 11 | 32 | 48 |
| **Total** | **161** | **107** | **128** | **396** |

> Sprint 2 is the heaviest for Backend due to the complex premium calculation engine.  
> Sprint 3 shifts focus to Frontend as the API layer comes online.  
> Sprint 4 is DevOps-heavy with messaging, documents, and infrastructure.

---

## 10. Key Milestones

| Milestone | Sprint | Date (Est.) | Criteria |
|---|---|---|---|
| **M1: Foundation Complete** | Sprint 1 | Week 2 | Solution builds, entities compile, DB migration runs, CI green |
| **M2: Business Logic Complete** | Sprint 2 | Week 4 | All premium/UW/compliance rules implemented with 200+ passing tests |
| **M3: API Functional** | Sprint 3 | Week 6 | All API endpoints operational with auth; quote-to-policy lifecycle works end-to-end |
| **M4: Integrations Complete** | Sprint 4 | Week 8 | Service Bus messaging, PDF generation, external APIs functional |
| **M5: Production Ready** | Sprint 5 | Week 10 | All integration tests pass, observability configured, security reviewed |
| **M6: Go-Live** | Sprint 6 | Week 12 | Deployed to production, smoke tests pass, monitoring active |
