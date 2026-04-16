# Keystone Insurance — API Reference

> **Base URL:** `https://{host}/api/v1`  
> **Authentication:** Bearer JWT (Microsoft Entra ID)  
> **Content-Type:** `application/json`

---

## Table of Contents

- [Authentication](#authentication)
- [Common Patterns](#common-patterns)
- [1. Quotes API](#1-quotes-api)
- [2. Policies API](#2-policies-api)
- [3. Underwriting API](#3-underwriting-api)
- [4. Endorsements API](#4-endorsements-api)
- [5. Renewals API](#5-renewals-api)
- [6. Reports API](#6-reports-api)
- [7. Health Checks](#7-health-checks)

---

## Authentication

All API endpoints require a valid JWT Bearer token issued by Microsoft Entra ID.

```http
Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIs...
```

### Roles

| Role | Allowed Operations |
|---|---|
| `Agent` | Create/read quotes, read policies |
| `Underwriter` | All Agent ops + evaluate underwriting, approve endorsements |
| `SeniorUnderwriter` | All Underwriter ops + referral decisions, high-value approvals |
| `Admin` | Full access including reports, cancellations, renewals |

> **Workshop Note:** In development mode, authentication is bypassed. The underwriter ID defaults to `1` if not provided.

---

## Common Patterns

### Pagination

All list endpoints accept pagination parameters:

| Parameter | Type | Default | Max | Description |
|---|---|---|---|---|
| `page` | int | 1 | — | Page number (1-based) |
| `pageSize` | int | 20 | 100 | Results per page |

**Paginated Response Shape:**

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 42
}
```

### Error Responses (RFC 7807)

All errors follow the [RFC 7807 Problem Details](https://tools.ietf.org/html/rfc7807) format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Error title",
  "status": 400,
  "detail": "Specific detail message",
  "instance": "/api/v1/quotes/42",
  "errors": {}
}
```

### HTTP Status Codes

| Code | Meaning |
|---|---|
| `200 OK` | Successful retrieval or update |
| `201 Created` | Resource created (includes `Location` header) |
| `400 Bad Request` | Business rule violation |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Insufficient role for this operation |
| `404 Not Found` | Resource does not exist |
| `422 Unprocessable Entity` | Validation errors (field-level) |
| `500 Internal Server Error` | Unexpected server failure |

---

## 1. Quotes API

### POST /api/v1/quotes

Create a new quote with automatic premium calculation.

**Request Body:**

```json
{
  "clientId": 1,
  "propertyAddress": "123 Industrial Pkwy",
  "city": "Chicago",
  "stateCode": "IL",
  "zipCode": "60601",
  "propertyValue": 1500000.00,
  "constructionType": "Non-Combustible",
  "occupancyType": "Manufacturing-Light",
  "yearBuilt": 2010,
  "squareFootage": 25000,
  "numberOfStories": 2,
  "sprinklersInstalled": true,
  "alarmSystemInstalled": true,
  "roofType": "TPO/EPDM",
  "roofAge": 5,
  "coverageLimit": 1500000.00,
  "deductible": 25000.00,
  "businessInterruptionCoverage": true,
  "businessInterruptionLimit": 500000.00,
  "equipmentBreakdownCoverage": false,
  "floodCoverage": false,
  "earthquakeCoverage": false,
  "priorClaimsCount": 0,
  "priorClaimsTotalAmount": 0.00
}
```

**Request Fields:**

| Field | Type | Required | Description |
|---|---|---|---|
| `clientId` | int | ✅ | ID of the client |
| `propertyAddress` | string | ✅ | Street address of insured property |
| `city` | string | ✅ | City |
| `stateCode` | string | ✅ | 2-letter US state code |
| `zipCode` | string | ✅ | 5-digit ZIP code |
| `propertyValue` | decimal | ✅ | Market value of the property |
| `constructionType` | string | ✅ | See [Construction Types](#construction-types) |
| `occupancyType` | string | ✅ | See [Occupancy Types](#occupancy-types) |
| `yearBuilt` | int | ✅ | Year the building was constructed |
| `squareFootage` | int | ✅ | Total building square footage |
| `numberOfStories` | int | ✅ | Number of floors |
| `sprinklersInstalled` | bool | ✅ | Fire sprinkler system present |
| `alarmSystemInstalled` | bool | ✅ | Security/fire alarm present |
| `roofType` | string | ❌ | Roof material type |
| `roofAge` | int | ✅ | Age of roof in years |
| `coverageLimit` | decimal | ✅ | Maximum coverage amount |
| `deductible` | decimal | ✅ | Policy deductible amount |
| `businessInterruptionCoverage` | bool | ✅ | Include BI coverage |
| `businessInterruptionLimit` | decimal | ❌ | BI coverage limit (if enabled) |
| `equipmentBreakdownCoverage` | bool | ✅ | Include equipment breakdown |
| `floodCoverage` | bool | ✅ | Include flood coverage |
| `earthquakeCoverage` | bool | ✅ | Include earthquake coverage |
| `priorClaimsCount` | int | ✅ | Number of prior claims (last 5 years) |
| `priorClaimsTotalAmount` | decimal | ✅ | Total prior claims amount |

**Response `201 Created`:**

```json
{
  "quoteId": 42,
  "quoteNumber": "Q20260416-A1B2C3D4",
  "status": "Draft",
  "createdDate": "2026-04-16T20:00:00Z",
  "expirationDate": "2026-05-16T20:00:00Z",
  "basePremium": 8250.00,
  "totalPremium": 9875.50,
  "premiumCalculationDetails": "Base Rate: $703.00\nProperty Value Factor: 0.0017\n...",
  "complianceWarnings": []
}
```

**Error `422 Unprocessable Entity`:**

```json
{
  "type": "validation",
  "title": "Quote validation failed",
  "errors": {
    "stateCode": ["State code is required"],
    "propertyValue": ["Property value must be greater than zero"]
  }
}
```

**Error `400 Bad Request` (compliance):**

```json
{
  "type": "compliance",
  "title": "State compliance check failed",
  "detail": "Texas: Minimum 1% wind/hail deductible required"
}
```

---

### GET /api/v1/quotes/{quoteId}

Retrieve a quote by ID with full details including client and underwriting decision.

**Path Parameters:**

| Parameter | Type | Description |
|---|---|---|
| `quoteId` | int | Quote ID |

**Response `200 OK`:**

```json
{
  "quoteId": 42,
  "quoteNumber": "Q20260416-A1B2C3D4",
  "clientId": 1,
  "status": "Draft",
  "createdDate": "2026-04-16T20:00:00Z",
  "expirationDate": "2026-05-16T20:00:00Z",
  "propertyAddress": "123 Industrial Pkwy",
  "city": "Chicago",
  "stateCode": "IL",
  "zipCode": "60601",
  "propertyValue": 1500000.00,
  "constructionType": "Non-Combustible",
  "occupancyType": "Manufacturing-Light",
  "yearBuilt": 2010,
  "squareFootage": 25000,
  "numberOfStories": 2,
  "sprinklersInstalled": true,
  "alarmSystemInstalled": true,
  "roofType": "TPO/EPDM",
  "roofAge": 5,
  "coverageLimit": 1500000.00,
  "deductible": 25000.00,
  "businessInterruptionCoverage": true,
  "businessInterruptionLimit": 500000.00,
  "equipmentBreakdownCoverage": false,
  "floodCoverage": false,
  "earthquakeCoverage": false,
  "priorClaimsCount": 0,
  "priorClaimsTotalAmount": 0.00,
  "basePremium": 8250.00,
  "totalPremium": 9875.50,
  "premiumCalculationDetails": "Base Rate: $703.00\n...",
  "client": {
    "clientId": 1,
    "businessName": "Acme Manufacturing Corp",
    "contactFirstName": "John",
    "contactLastName": "Smith"
  },
  "underwritingDecision": null
}
```

**Response `404 Not Found`:** Quote does not exist.

---

### GET /api/v1/quotes

Search and filter quotes with pagination.

**Query Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `clientId` | int | ❌ | Filter by client |
| `stateCode` | string | ❌ | Filter by state (e.g., `IL`) |
| `status` | string | ❌ | Filter by status (`Draft`, `Approved`, `Declined`) |
| `fromDate` | datetime | ❌ | Created on or after (ISO 8601) |
| `toDate` | datetime | ❌ | Created on or before (ISO 8601) |
| `page` | int | ❌ | Page number (default: 1) |
| `pageSize` | int | ❌ | Results per page (default: 20, max: 100) |

**Example:**

```http
GET /api/v1/quotes?stateCode=IL&status=Draft&page=1&pageSize=10
```

**Response `200 OK`:**

```json
{
  "items": [
    {
      "quoteId": 42,
      "quoteNumber": "Q20260416-A1B2C3D4",
      "status": "Draft",
      "createdDate": "2026-04-16T20:00:00Z",
      "propertyAddress": "123 Industrial Pkwy",
      "city": "Chicago",
      "stateCode": "IL",
      "totalPremium": 9875.50,
      "clientName": "Acme Manufacturing Corp"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1
}
```

---

### PUT /api/v1/quotes/{quoteId}/recalculate

Recalculate premium for an existing quote (e.g., after property data changes).

**Path Parameters:**

| Parameter | Type | Description |
|---|---|---|
| `quoteId` | int | Quote ID |

**Response `200 OK`:**

```json
{
  "quoteId": 42,
  "quoteNumber": "Q20260416-A1B2C3D4",
  "status": "Draft",
  "createdDate": "2026-04-16T20:00:00Z",
  "expirationDate": "2026-05-16T20:00:00Z",
  "basePremium": 8250.00,
  "totalPremium": 9875.50,
  "premiumCalculationDetails": "Base Rate: $703.00\n...",
  "complianceWarnings": []
}
```

---

## 2. Policies API

### POST /api/v1/policies

Issue (bind) a policy from an approved quote.

**Request Body:**

```json
{
  "quoteId": 42,
  "effectiveDate": "2026-05-01",
  "paymentPlan": "Quarterly"
}
```

**Request Fields:**

| Field | Type | Required | Default | Description |
|---|---|---|---|---|
| `quoteId` | int | ✅ | — | ID of the approved quote |
| `effectiveDate` | date | ✅ | — | Policy effective date |
| `paymentPlan` | string | ❌ | `"Annual"` | Payment frequency |

**Payment Plans:**

| Plan | Installments | Calculation |
|---|---|---|
| `Annual` | 1 | Full premium |
| `Semi-Annual` | 2 | Premium ÷ 2, +2% surcharge |
| `Quarterly` | 4 | Premium ÷ 4, +4% surcharge |
| `Monthly` | 12 | Premium ÷ 12, +6% surcharge |

**Response `201 Created`:**

```json
{
  "policyId": 10,
  "policyNumber": "KIP20260501-A1B2C3D4E5",
  "effectiveDate": "2026-05-01",
  "expirationDate": "2027-05-01",
  "issueDate": "2026-04-16T20:00:00Z",
  "status": "Active",
  "annualPremium": 9875.50,
  "paymentPlan": "Quarterly",
  "installmentAmount": 2594.82,
  "nextPaymentDue": "2026-08-01",
  "coverageLimit": 1500000.00,
  "deductible": 25000.00,
  "reinsuranceCeded": false,
  "cededPremium": 0.00
}
```

**Error `400 Bad Request`:**

```json
{
  "detail": "Only approved quotes can be bound to policies"
}
```

---

### GET /api/v1/policies/{policyId}

Retrieve full policy details including coverage and endorsements.

**Response `200 OK`:**

```json
{
  "policyId": 10,
  "policyNumber": "KIP20260501-A1B2C3D4E5",
  "quoteId": 42,
  "effectiveDate": "2026-05-01",
  "expirationDate": "2027-05-01",
  "issueDate": "2026-04-16T20:00:00Z",
  "status": "Active",
  "annualPremium": 9875.50,
  "paymentPlan": "Quarterly",
  "installmentAmount": 2594.82,
  "nextPaymentDue": "2026-08-01",
  "coverageLimit": 1500000.00,
  "deductible": 25000.00,
  "coverageType": "Commercial Property",
  "businessInterruptionCoverage": true,
  "businessInterruptionLimit": 500000.00,
  "equipmentBreakdownCoverage": false,
  "floodCoverage": false,
  "earthquakeCoverage": false,
  "reinsuranceCeded": false,
  "cededPremium": 0.00,
  "endorsements": []
}
```

---

### POST /api/v1/policies/{policyId}/cancel

Cancel an active policy with return premium calculation.

**Request Body:**

```json
{
  "cancellationDate": "2026-08-15",
  "cancellationReason": "Insured Request",
  "cancellationType": "ShortRate"
}
```

**Request Fields:**

| Field | Type | Required | Default | Description |
|---|---|---|---|---|
| `cancellationDate` | date | ✅ | — | Effective cancellation date |
| `cancellationReason` | string | ✅ | — | Reason for cancellation |
| `cancellationType` | string | ❌ | `"ProRata"` | Return premium method |

**Cancellation Types:**

| Type | Return Premium | Description |
|---|---|---|
| `ProRata` | Full unused premium returned | Standard for insurer-initiated |
| `ShortRate` | Unused premium minus 10% penalty | Standard for insured-initiated |
| `Flat` | No return premium | Policy issued but never effective |

**Response `200 OK`:**

```json
{
  "policyId": 10,
  "status": "Cancelled",
  "cancellationDate": "2026-08-15",
  "cancellationReason": "Insured Request",
  "returnPremium": 4785.23
}
```

---

## 3. Underwriting API

### POST /api/v1/underwriting/evaluate

Evaluate a quote for an underwriting decision. The system automatically calculates a risk score and renders a decision.

**Request Body:**

```json
{
  "quoteId": 42,
  "underwriterId": 1
}
```

**Request Fields:**

| Field | Type | Required | Default | Description |
|---|---|---|---|---|
| `quoteId` | int | ✅ | — | Quote to evaluate |
| `underwriterId` | int | ❌ | `1` | Underwriter performing evaluation |

**Response `200 OK`:**

```json
{
  "uwId": 5,
  "quoteId": 42,
  "decisionDate": "2026-04-16T20:10:00Z",
  "decision": "Approved",
  "riskScore": 45.50,
  "ratings": {
    "constructionRating": "Good",
    "occupancyRating": "Average Risk",
    "protectionRating": "Superior",
    "lossHistoryRating": "Loss Free",
    "catastropheZoneRating": "Moderate"
  },
  "catastropheExposure": {
    "highCatExposure": false,
    "catastrophePML": 375000.00
  },
  "approvalConditions": "Standard terms apply",
  "declineReason": null,
  "referredToSeniorUnderwriter": false,
  "referralReason": null,
  "additionalInformationRequired": null,
  "notes": "Risk Score: 45.50. Evaluated 4/16/2026 8:10 PM"
}
```

### Decision Matrix

| Risk Score | Prior Claims ≥ 3 | High Cat Exposure (PML > 50%) | Decision |
|---|---|---|---|
| > 85 | — | — | `Declined` |
| — | — | Yes | `Declined` |
| > 70 | — | — | `ReferToSenior` |
| — | Yes | — | `ReferToSenior` |
| > 60 | — | — | `RequestMoreInfo` |
| ≤ 60 | No | No | `Approved` |

### Risk Rating Values

**Construction Rating:**

| Type | Rating |
|---|---|
| Fire Resistive | Excellent |
| Modified Fire Resistive | Good |
| Masonry Non-Combustible | Good |
| Non-Combustible | Average |
| Joisted Masonry | Below Average |
| Frame | Poor |

**Protection Rating:**

| Configuration | Rating |
|---|---|
| Sprinklers + Alarm | Superior |
| Sprinklers only | Good |
| Alarm only | Average |
| Neither | Below Average |

**Loss History Rating:**

| Claims Count | Rating |
|---|---|
| 0 | Loss Free |
| 1 | Minimal |
| 2 | Moderate |
| 3+ | Poor |

---

## 4. Endorsements API

### POST /api/v1/endorsements/coverage-change

Create a coverage limit change endorsement with pro-rated premium adjustment.

**Request Body:**

```json
{
  "policyId": 10,
  "newCoverageLimit": 2000000.00,
  "effectiveDate": "2026-07-01"
}
```

**Request Fields:**

| Field | Type | Required | Description |
|---|---|---|---|
| `policyId` | int | ✅ | Policy to endorse |
| `newCoverageLimit` | decimal | ✅ | New coverage limit amount |
| `effectiveDate` | date | ✅ | Endorsement effective date |

**Response `201 Created`:**

```json
{
  "endorsementId": 1,
  "endorsementNumber": "KIP20260501-A1B2C3D4E5-END20260416201000",
  "policyId": 10,
  "endorsementType": "CoverageChange",
  "status": "Pending",
  "effectiveDate": "2026-07-01",
  "changeDescription": "Coverage limit change from $1,500,000 to $2,000,000",
  "premiumChange": 1234.56,
  "newCoverageLimit": 2000000.00
}
```

**Premium Change Calculation:**

```
premiumChange = annualPremium × (newLimit / currentLimit - 1) × daysRemaining / 365
```

---

### POST /api/v1/endorsements/deductible-change

Create a deductible change endorsement with premium adjustment.

**Request Body:**

```json
{
  "policyId": 10,
  "newDeductible": 50000.00,
  "effectiveDate": "2026-07-01"
}
```

**Request Fields:**

| Field | Type | Required | Description |
|---|---|---|---|
| `policyId` | int | ✅ | Policy to endorse |
| `newDeductible` | decimal | ✅ | New deductible amount |
| `effectiveDate` | date | ✅ | Endorsement effective date |

**Response `201 Created`:**

```json
{
  "endorsementId": 2,
  "endorsementNumber": "KIP20260501-A1B2C3D4E5-END20260416201500",
  "policyId": 10,
  "endorsementType": "CoverageChange",
  "status": "Pending",
  "effectiveDate": "2026-07-01",
  "changeDescription": "Deductible change from $25,000 to $50,000",
  "premiumChange": -876.54,
  "newDeductible": 50000.00
}
```

> **Note:** Increasing the deductible results in a negative premium change (credit).

---

### POST /api/v1/endorsements/cancellation

Create a cancellation endorsement.

**Request Body:**

```json
{
  "policyId": 10,
  "cancellationDate": "2026-08-15",
  "reason": "Insured Request"
}
```

**Request Fields:**

| Field | Type | Required | Description |
|---|---|---|---|
| `policyId` | int | ✅ | Policy to cancel |
| `cancellationDate` | date | ✅ | Effective cancellation date |
| `reason` | string | ✅ | Cancellation reason |

**Response `201 Created`:**

```json
{
  "endorsementId": 3,
  "endorsementNumber": "KIP20260501-A1B2C3D4E5-END20260416202000",
  "policyId": 10,
  "endorsementType": "Cancellation",
  "status": "Pending",
  "effectiveDate": "2026-08-15",
  "changeDescription": "Policy cancellation effective 8/15/2026. Reason: Insured Request",
  "premiumChange": -4785.23
}
```

**Cancellation Premium Logic:**

| Reason | Method | Formula |
|---|---|---|
| `"Insured Request"` | ShortRate | `annualPremium × daysRemaining / 365 × 0.90` |
| All other reasons | ProRata | `annualPremium × daysRemaining / 365` |

---

### GET /api/v1/endorsements

List endorsements with optional filters.

**Query Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `policyId` | int | ❌ | Filter by policy |
| `status` | string | ❌ | Filter by status (`Pending`, `Approved`, `Applied`) |

**Example:**

```http
GET /api/v1/endorsements?policyId=10&status=Pending
```

**Response `200 OK`:**

```json
[
  {
    "endorsementId": 1,
    "policyId": 10,
    "endorsementNumber": "KIP20260501-A1B2C3D4E5-END20260416201000",
    "effectiveDate": "2026-07-01",
    "requestDate": "2026-04-16T20:10:00Z",
    "endorsementType": "CoverageChange",
    "status": "Pending",
    "changeDescription": "Coverage limit change from $1,500,000 to $2,000,000",
    "premiumChange": 1234.56,
    "newCoverageLimit": 2000000.00,
    "createdDate": "2026-04-16T20:10:00Z"
  }
]
```

---

## 5. Renewals API

### POST /api/v1/renewals/generate

Generate a renewal quote for an expiring policy with inflation and trend adjustments.

**Request Body:**

```json
{
  "policyId": 10
}
```

**Response `201 Created`:**

```json
{
  "renewalQuoteId": 55,
  "quoteNumber": "RNW-KIP20260501-A1B2C3D4E5",
  "status": "Draft",
  "createdDate": "2026-04-16T20:15:00Z",
  "expirationDate": "2026-05-16T20:15:00Z",
  "renewalFactors": {
    "inflationAdjustment": 1.03,
    "stateTrendFactor": 1.03,
    "roofAgeIncrement": 1,
    "propertyValueAdjusted": 1545000.00,
    "coverageLimitAdjusted": 1545000.00
  },
  "basePremium": 8500.00,
  "totalPremium": 10150.75
}
```

### State Trend Factors

| State | Trend Factor | Reason |
|---|---|---|
| FL | +8% (1.08) | Hurricane activity |
| CA | +12% (1.12) | Wildfire risk |
| TX | +5% (1.05) | Hail losses |
| LA | +10% (1.10) | Hurricane exposure |
| All others | +3% (1.03) | General inflation |

---

### GET /api/v1/renewals/expiring

List policies expiring within a specified number of days.

**Query Parameters:**

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `daysAhead` | int | ❌ | 60 | Look-ahead window in days |

**Example:**

```http
GET /api/v1/renewals/expiring?daysAhead=90
```

**Response `200 OK`:**

```json
{
  "items": [
    {
      "policyId": 10,
      "policyNumber": "KIP20260501-A1B2C3D4E5",
      "expirationDate": "2027-05-01",
      "annualPremium": 9875.50,
      "clientName": "Acme Manufacturing Corp",
      "clientEmail": "john.smith@acme.com",
      "propertyAddress": "123 Industrial Pkwy",
      "stateCode": "IL"
    }
  ],
  "totalCount": 1
}
```

---

## 6. Reports API

### GET /api/v1/reports/premium-summary

Premium summary aggregated by state for a given year.

**Query Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `year` | int | ✅ | Reporting year |

**Example:**

```http
GET /api/v1/reports/premium-summary?year=2026
```

**Response `200 OK`:**

```json
{
  "year": 2026,
  "states": [
    {
      "stateCode": "IL",
      "quoteCount": 45,
      "totalPremium": 425000.00,
      "averagePremium": 9444.44,
      "minPremium": 5200.00,
      "maxPremium": 18750.00
    },
    {
      "stateCode": "CA",
      "quoteCount": 32,
      "totalPremium": 385000.00,
      "averagePremium": 12031.25,
      "minPremium": 6800.00,
      "maxPremium": 25000.00
    }
  ]
}
```

---

## 7. Health Checks

### GET /health/ready

Readiness probe. Checks database connectivity via EF Core health check.

**Response `200 OK`:**

```json
{
  "status": "Healthy"
}
```

**Response `503 Service Unavailable`:** Database is unreachable.

---

### GET /health/live

Liveness probe. Confirms the application process is running.

**Response `200 OK`:**

```json
{
  "status": "Healthy"
}
```

---

## Appendix: Reference Values

### Construction Types

| Value | Premium Impact |
|---|---|
| `Frame` | Highest (factor: 1.45) |
| `Joisted Masonry` | High (factor: 1.25) |
| `Non-Combustible` | Medium (factor: 1.10) |
| `Masonry Non-Combustible` | Low (factor: 0.95) |
| `Modified Fire Resistive` | Lower (factor: 0.80) |
| `Fire Resistive` | Lowest (factor: 0.65) |

### Occupancy Types

| Value | Premium Impact |
|---|---|
| `Office` | Low (factor: 0.85) |
| `Educational` | Low (factor: 0.80) |
| `Apartment` | Low (factor: 0.90) |
| `Warehouse` | Low (factor: 0.95) |
| `Retail` | Medium (factor: 1.00) |
| `Medical` | Medium (factor: 1.05) |
| `Mixed-Use` | Medium (factor: 1.10) |
| `Manufacturing-Light` | High (factor: 1.15) |
| `Hotel` | High (factor: 1.25) |
| `Restaurant` | High (factor: 1.35) |
| `Manufacturing-Heavy` | Highest (factor: 1.50) |

### Quote Statuses

| Status | Description |
|---|---|
| `Draft` | Initial state after creation |
| `Approved` | Underwriting approved; ready to bind |
| `Declined` | Underwriting declined |
| `Referred` | Referred to senior underwriter |
| `Expired` | Past expiration date without binding |
| `Bound` | Converted to a policy |

### Policy Statuses

| Status | Description |
|---|---|
| `Active` | Policy is in-force |
| `Cancelled` | Policy has been cancelled |
| `Expired` | Policy term has ended |
| `NonRenewal` | Insurer elected not to renew |

### Endorsement Types

| Type | Description |
|---|---|
| `CoverageChange` | Coverage limit or deductible modification |
| `Cancellation` | Policy cancellation |
| `Additional Insured` | Add named insured |
| `LocationChange` | Property address update |
