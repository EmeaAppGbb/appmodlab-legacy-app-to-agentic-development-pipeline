# Keystone Insurance — REST API Contracts

> **Version:** 1.0  
> **Generated:** 2026-04-16  
> **Base URL:** `https://{host}/api/v1`  
> **Auth:** Bearer JWT (Microsoft Entra ID)

---

## 1. Quotes API

### POST /quotes

Create a new quote with premium calculation.

**Request:**
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

**Error `400 Bad Request` (compliance failure):**
```json
{
  "type": "compliance",
  "title": "State compliance check failed",
  "detail": "Texas: Minimum 1% wind/hail deductible required"
}
```

---

### GET /quotes/{quoteId}

Retrieve a quote with its premium breakdown.

**Response `200 OK`:**
```json
{
  "quoteId": 42,
  "quoteNumber": "Q20260416-A1B2C3D4",
  "clientId": 1,
  "status": "Draft",
  "createdDate": "2026-04-16T20:00:00Z",
  "expirationDate": "2026-05-16T20:00:00Z",
  "property": {
    "address": "123 Industrial Pkwy",
    "city": "Chicago",
    "stateCode": "IL",
    "zipCode": "60601",
    "propertyValue": 1500000.00,
    "constructionType": "Non-Combustible",
    "occupancyType": "Manufacturing-Light",
    "yearBuilt": 2010,
    "squareFootage": 25000,
    "numberOfStories": 2,
    "roofType": "TPO/EPDM",
    "roofAge": 5
  },
  "protection": {
    "sprinklersInstalled": true,
    "alarmSystemInstalled": true
  },
  "coverage": {
    "coverageLimit": 1500000.00,
    "deductible": 25000.00,
    "businessInterruptionCoverage": true,
    "businessInterruptionLimit": 500000.00,
    "equipmentBreakdownCoverage": false,
    "floodCoverage": false,
    "earthquakeCoverage": false
  },
  "lossHistory": {
    "priorClaimsCount": 0,
    "priorClaimsTotalAmount": 0.00
  },
  "premium": {
    "basePremium": 8250.00,
    "totalPremium": 9875.50,
    "calculationDetails": "Base Rate: $703.00\n..."
  },
  "underwritingDecision": null
}
```

---

### GET /quotes?clientId={id}&stateCode={code}&status={status}&fromDate={date}&toDate={date}&page={n}&pageSize={n}

Search and paginate quotes. Replaces `usp_SearchQuotes`.

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
  "pageSize": 20,
  "totalCount": 1
}
```

---

### PUT /quotes/{quoteId}/recalculate

Recalculate premium for an existing quote after property changes.

**Response `200 OK`:** Same shape as `POST /quotes` response.

---

## 2. Policies API

### POST /policies

Bind an approved quote to a policy (issue policy).

**Request:**
```json
{
  "quoteId": 42,
  "effectiveDate": "2026-05-01",
  "paymentPlan": "Quarterly"
}
```

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

**Error `400`:** `"Only approved quotes can be bound to policies"`

---

### GET /policies/{policyId}

Retrieve full policy details.

**Response `200 OK`:**
```json
{
  "policyId": 10,
  "policyNumber": "KIP20260501-A1B2C3D4E5",
  "quoteId": 42,
  "effectiveDate": "2026-05-01",
  "expirationDate": "2027-05-01",
  "status": "Active",
  "annualPremium": 9875.50,
  "paymentPlan": "Quarterly",
  "installmentAmount": 2594.82,
  "nextPaymentDue": "2026-08-01",
  "coverage": {
    "coverageLimit": 1500000.00,
    "deductible": 25000.00,
    "coverageType": "Commercial Property",
    "businessInterruptionCoverage": true,
    "businessInterruptionLimit": 500000.00,
    "equipmentBreakdownCoverage": false,
    "floodCoverage": false,
    "floodLimit": 0.00,
    "earthquakeCoverage": false,
    "earthquakeLimit": 0.00
  },
  "reinsurance": {
    "ceded": false,
    "cededPremium": 0.00,
    "treatyId": null
  },
  "endorsements": []
}
```

---

### POST /policies/{policyId}/cancel

Cancel an active policy.

**Request:**
```json
{
  "cancellationDate": "2026-08-15",
  "cancellationReason": "Insured Request",
  "cancellationType": "ShortRate"
}
```

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

**Cancellation types:** `ProRata` (full unused return), `ShortRate` (10% penalty), `Flat` (no return).

---

## 3. Underwriting API

### POST /underwriting/evaluate

Evaluate a quote for underwriting decision.

**Request:**
```json
{
  "quoteId": 42
}
```

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

**Decision values:** `Approved`, `Declined`, `ReferToSenior`, `RequestMoreInfo`

**Decision thresholds:**

| Risk Score | Claims ≥ 3 | High Cat Exposure | Decision |
|---|---|---|---|
| > 85 | — | — | Declined |
| — | — | PML > 50% of value | Declined |
| > 70 | — | — | ReferToSenior |
| — | Yes | — | ReferToSenior |
| > 60 | — | — | RequestMoreInfo |
| ≤ 60 | No | No | Approved |

---

## 4. Endorsements API

### POST /endorsements/coverage-change

Create a coverage limit change endorsement.

**Request:**
```json
{
  "policyId": 10,
  "newCoverageLimit": 2000000.00,
  "effectiveDate": "2026-07-01"
}
```

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

---

### POST /endorsements/deductible-change

Create a deductible change endorsement.

**Request:**
```json
{
  "policyId": 10,
  "newDeductible": 50000.00,
  "effectiveDate": "2026-07-01"
}
```

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

---

### POST /endorsements/cancellation

Create a cancellation endorsement.

**Request:**
```json
{
  "policyId": 10,
  "cancellationDate": "2026-08-15",
  "reason": "Insured Request"
}
```

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

**Cancellation premium logic:** `Insured Request` → ShortRate (10% penalty); all other reasons → ProRata (full return).

---

### GET /endorsements?policyId={id}&status={status}

List endorsements for a policy.

---

## 5. Renewals API

### POST /renewals/generate

Generate a renewal quote for an expiring policy.

**Request:**
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

**State trend factors applied at renewal:**

| State | Trend Factor | Reason |
|---|---|---|
| FL | +8% | Hurricane activity |
| CA | +12% | Wildfire risk |
| TX | +5% | Hail losses |
| LA | +10% | Hurricane exposure |
| Default | +3% | General inflation |

---

### GET /renewals/expiring?daysAhead={n}

List policies expiring within the next N days. Replaces `usp_GetExpiringPolicies`.

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

## 6. Reporting API

### GET /reports/premium-summary?year={year}

Premium summary by state. Replaces `usp_GetPremiumSummaryByState`.

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
    }
  ]
}
```

---

## 7. Common Response Patterns

### Pagination
All list endpoints accept `page` (default: 1) and `pageSize` (default: 20, max: 100).

### Error Format (RFC 7807)
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

| Code | Usage |
|---|---|
| `200` | Successful retrieval or update |
| `201` | Resource created |
| `400` | Business rule violation |
| `401` | Missing/invalid JWT |
| `403` | Insufficient role |
| `404` | Resource not found |
| `422` | Validation errors |
| `500` | Internal server error |

---

## 8. Authentication & Authorization

All endpoints require a valid JWT Bearer token from Microsoft Entra ID.

| Role | Allowed Operations |
|---|---|
| `Agent` | Create/read quotes, read policies |
| `Underwriter` | All Agent ops + evaluate underwriting, approve endorsements |
| `SeniorUnderwriter` | All Underwriter ops + referral decisions, high-value approvals |
| `Admin` | Full access including reports, cancellations, renewals |
