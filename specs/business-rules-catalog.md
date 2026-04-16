# Keystone Insurance — Business Rules Catalog

> **Version:** 1.0  
> **Generated:** 2026-04-16  
> **Source Services:** `QuotingEngine`, `UnderwritingService`, `ComplianceService`, `PremiumCalculator`, `EndorsementService`, `RenewalService`, `PolicyService`  
> **Source Rules:** `RatingRules`, `UnderwritingRules`, `ComplianceRules`

---

## 1. Premium Calculation Rules (QuotingEngine)

### 1.1 Base Rate Determination

| ID | Rule | Source |
|---|---|---|
| QE-001 | Base rate is state-specific (20 states mapped, default $700) | `QuotingEngine.GetBaseRate()` |
| QE-002 | Base rate adjusted by construction type multiplier at lookup time | `QuotingEngine.GetBaseRate()` |
| QE-003 | Frame construction: base rate × 1.25 | `QuotingEngine.GetBaseRate()` |
| QE-004 | Joisted Masonry: base rate × 1.10 | `QuotingEngine.GetBaseRate()` |
| QE-005 | Non-Combustible: base rate × 0.95 | `QuotingEngine.GetBaseRate()` |
| QE-006 | Masonry Non-Combustible: base rate × 0.85 | `QuotingEngine.GetBaseRate()` |
| QE-007 | Modified Fire Resistive: base rate × 0.75 | `QuotingEngine.GetBaseRate()` |
| QE-008 | Fire Resistive: base rate × 0.65 | `QuotingEngine.GetBaseRate()` |

**State base rates:**

| State | Rate | State | Rate | State | Rate | State | Rate |
|---|---|---|---|---|---|---|---|
| CA | $850 | TX | $720 | FL | $980 | NY | $890 |
| IL | $740 | PA | $710 | OH | $680 | GA | $700 |
| NC | $690 | MI | $730 | NJ | $870 | VA | $695 |
| WA | $750 | AZ | $780 | MA | $860 | TN | $670 |
| IN | $665 | MO | $675 | MD | $820 | WI | $720 |

---

### 1.2 Property Value Factor

| ID | Rule | Factor |
|---|---|---|
| QE-010 | Property value < $100K | 0.0025 |
| QE-011 | $100K – $250K | 0.0023 |
| QE-012 | $250K – $500K | 0.0021 |
| QE-013 | $500K – $1M | 0.0019 |
| QE-014 | $1M – $2.5M | 0.0017 |
| QE-015 | $2.5M – $5M | 0.0015 |
| QE-016 | $5M+ | 0.0013 |

---

### 1.3 Construction Type Factor

| ID | Construction Type | Factor |
|---|---|---|
| QE-020 | Frame | 1.45 |
| QE-021 | Joisted Masonry | 1.25 |
| QE-022 | Non-Combustible | 1.10 |
| QE-023 | Masonry Non-Combustible | 0.95 |
| QE-024 | Modified Fire Resistive | 0.80 |
| QE-025 | Fire Resistive | 0.65 |

---

### 1.4 Building Age Factor

| ID | Age Range | Factor |
|---|---|---|
| QE-030 | < 5 years | 0.90 (new construction credit) |
| QE-031 | 5–9 years | 0.95 |
| QE-032 | 10–19 years | 1.00 |
| QE-033 | 20–29 years | 1.05 |
| QE-034 | 30–39 years | 1.15 |
| QE-035 | 40–49 years | 1.25 |
| QE-036 | 50–74 years | 1.40 |
| QE-037 | 75+ years | 1.60 (historic building surcharge) |

---

### 1.5 Occupancy Classification Factor

| ID | Occupancy Type | Factor | Notes |
|---|---|---|---|
| QE-040 | Office | 0.85 | |
| QE-041 | Retail | 1.00 | × 1.05 in FL (hurricane exposure) |
| QE-042 | Restaurant | 1.35 | × 1.10 in CA (higher liability) |
| QE-043 | Manufacturing-Light | 1.15 | |
| QE-044 | Manufacturing-Heavy | 1.50 | |
| QE-045 | Warehouse | 0.95 | |
| QE-046 | Mixed-Use | 1.10 | |
| QE-047 | Medical | 1.05 | |
| QE-048 | Educational | 0.80 | |
| QE-049 | Hotel | 1.25 | × 1.10 in CA (higher liability) |
| QE-050 | Apartment | 0.90 | |

---

### 1.6 Protection Factor

| ID | Rule | Factor |
|---|---|---|
| QE-060 | Sprinklers installed | × 0.75 (25% credit) |
| QE-061 | Alarm system installed | × 0.90 (10% credit) |
| QE-062 | Both sprinklers AND alarm | Additional × 0.95 (5% combo bonus) |
| QE-063 | No protection systems | 1.00 (no credit) |

---

### 1.7 Territory/Location Factor

| ID | Rule | Factor |
|---|---|---|
| QE-070 | High-risk ZIP prefixes (90, 33, 07, 10, 11, 77, 94) | 1.25 |
| QE-071 | Medium-risk states (CA, FL, TX, NY, NJ) | 1.10 |
| QE-072 | All other territories | 1.00 |

---

### 1.8 Catastrophe Zone Factor

| ID | Rule | Factor |
|---|---|---|
| QE-080 | Hurricane states (FL, LA, MS, AL, TX, NC, SC, GA) | × 1.35 |
| QE-081 | Coastal ZIP prefixes in hurricane states (33, 34, 32, 70, 39, 77, 28, 29) | Additional × 1.20 |
| QE-082 | Earthquake states (CA, AK, WA, OR, NV) | × 1.25 |
| QE-083 | Tornado alley states (OK, KS, NE, TX, SD) | × 1.15 |

---

### 1.9 Roof Factor

| ID | Roof Type | Factor |
|---|---|---|
| QE-090 | Asphalt Shingle | 1.10 |
| QE-091 | Metal | 0.85 |
| QE-092 | Tile | 0.90 |
| QE-093 | Slate | 0.80 |
| QE-094 | Flat/Built-Up | 1.15 |
| QE-095 | TPO/EPDM | 0.95 |

**Roof age surcharges:**

| ID | Age | Multiplier |
|---|---|---|
| QE-096 | < 3 years | × 0.95 (new roof credit) |
| QE-097 | 3–10 years | × 1.00 |
| QE-098 | 11–15 years | × 1.10 |
| QE-099 | 16–20 years | × 1.25 |
| QE-100 | 20+ years | × 1.40 |

---

### 1.10 Square Footage Factor

| ID | Range | Factor |
|---|---|---|
| QE-110 | < 5,000 sq ft | 1.15 |
| QE-111 | 5,000–9,999 | 1.05 |
| QE-112 | 10,000–24,999 | 1.00 |
| QE-113 | 25,000–49,999 | 0.95 |
| QE-114 | 50,000–99,999 | 0.90 |
| QE-115 | 100,000+ | 0.85 |

---

### 1.11 Number of Stories Factor

| ID | Stories | Factor |
|---|---|---|
| QE-120 | 1 story | 0.95 |
| QE-121 | 2 stories | 1.00 |
| QE-122 | 3–4 stories | 1.10 |
| QE-123 | 5–6 stories | 1.20 |
| QE-124 | 7–10 stories | 1.35 |
| QE-125 | 11+ stories | 1.50 (high-rise surcharge) |

---

### 1.12 Loss History Factor

| ID | Rule | Factor |
|---|---|---|
| QE-130 | 0 prior claims | 0.90 (loss-free credit) |
| QE-131 | 1 prior claim | × 1.15 |
| QE-132 | 2 prior claims | × 1.30 |
| QE-133 | 3+ prior claims | × 1.50 |
| QE-134 | Claims total > $500K | Additional × 1.40 |
| QE-135 | Claims total $250K–$500K | Additional × 1.25 |
| QE-136 | Claims total $100K–$250K | Additional × 1.15 |
| QE-137 | Claims total $50K–$100K | Additional × 1.10 |

---

### 1.13 Deductible Credit

| ID | Deductible (% of value) | Credit Factor |
|---|---|---|
| QE-140 | ≥ 5% | 0.70 (30% credit) |
| QE-141 | 3–5% | 0.80 |
| QE-142 | 2–3% | 0.85 |
| QE-143 | 1–2% | 0.90 |
| QE-144 | 0.5–1% | 0.95 |
| QE-145 | < 0.5% | 1.00 (no credit) |

---

### 1.14 Optional Coverage Premiums

| ID | Coverage | Calculation |
|---|---|---|
| QE-150 | Business Interruption – Office | limit × 0.0025 |
| QE-151 | Business Interruption – Restaurant/Retail | limit × 0.0045 |
| QE-152 | Business Interruption – Mfg-Heavy | limit × 0.0050 |
| QE-153 | Business Interruption – Default | limit × 0.0035 |
| QE-154 | Equipment Breakdown | propertyValue × 0.0008 |
| QE-155 | Flood – high-risk states (FL, LA, TX, NC, SC) | coverageLimit × 0.0055 |
| QE-156 | Flood – other states | coverageLimit × 0.0025 |
| QE-157 | Earthquake – CA | coverageLimit × 0.0085 |
| QE-158 | Earthquake – AK | coverageLimit × 0.0070 |
| QE-159 | Earthquake – WA, OR | coverageLimit × 0.0045 |
| QE-160 | Earthquake – other states | coverageLimit × 0.0010 |

---

### 1.15 State Adjustment Factors

| ID | State | Factor | Reason |
|---|---|---|---|
| QE-170 | CA | 1.15 | High regulation and litigation |
| QE-171 | FL | 1.20 | Hurricane exposure, AOB fraud |
| QE-172 | NY | 1.12 | High regulatory requirements |
| QE-173 | TX | 1.05 | Hail and wind exposure |
| QE-174 | LA | 1.18 | Hurricane and flood exposure |
| QE-175 | NJ | 1.10 | High cost of business |
| QE-176 | IL | 1.03 | Moderate |
| QE-177 | PA | 1.02 | Moderate |
| QE-178 | Default | 1.00 | No adjustment |

---

### 1.16 State Minimum Premiums

| ID | State | Minimum |
|---|---|---|
| QE-180 | CA | $750 |
| QE-181 | FL | $800 |
| QE-182 | NY | $725 |
| QE-183 | TX | $650 |
| QE-184 | LA | $700 |
| QE-185 | Default | $500 |

---

### 1.17 Surcharges and Taxes

| ID | Rule | Amount |
|---|---|---|
| QE-190 | FL state tax | 1.75% of premium |
| QE-191 | TX state tax | 1.85% of premium |
| QE-192 | CA state tax | 2.30% of premium |
| QE-193 | NY state tax | 3.00% of premium |
| QE-194 | LA state tax | 4.75% of premium |
| QE-195 | FL Hurricane Catastrophe Fund surcharge | 2.00% of premium |
| QE-196 | Fire marshal fee (all states) | $25.00 flat |
| QE-197 | TRIA terrorism surcharge (all states) | 0.15% of premium |

---

### 1.18 Premium Formula

```
BasePremium = BaseRate × PropertyValueFactor × ConstructionFactor × AgeFactor
             × OccupancyFactor × ProtectionFactor × TerritoryFactor
             × CatastropheZoneFactor × RoofFactor × SqFootageFactor
             × StoriesFactor × LossHistoryFactor × DeductibleCredit

TotalPremium = (BasePremium + OptionalCoverages) × StateAdjustmentFactor
             + SurchargesAndTaxes

Enforced: TotalPremium ≥ StateMinimumPremium
```

Source: `QuotingEngine.CalculatePremium()` lines 48–174

---

### 1.19 Quote Validation Rules

| ID | Rule | Source |
|---|---|---|
| QE-200 | State code is required | `QuotingEngine.ValidateQuote()` |
| QE-201 | Property value must be > 0 | `QuotingEngine.ValidateQuote()` |
| QE-202 | Coverage limit must be > 0 | `QuotingEngine.ValidateQuote()` |
| QE-203 | Coverage limit cannot exceed 150% of property value | `QuotingEngine.ValidateQuote()` |
| QE-204 | Minimum deductible is $500 | `QuotingEngine.ValidateQuote()` |
| QE-205 | Construction type is required | `QuotingEngine.ValidateQuote()` |
| QE-206 | Occupancy type is required | `QuotingEngine.ValidateQuote()` |
| QE-207 | Year built must be 1800–current year | `QuotingEngine.ValidateQuote()` |
| QE-208 | Square footage must be > 0 | `QuotingEngine.ValidateQuote()` |

---

## 2. Underwriting Rules (UnderwritingService)

### 2.1 Risk Score Calculation

Base score: 50. Adjustments:

| ID | Criterion | Impact |
|---|---|---|
| UW-001 | Frame construction | +15 |
| UW-002 | Joisted Masonry | +10 |
| UW-003 | Non-Combustible | +5 |
| UW-004 | Fire Resistive | −5 |
| UW-005 | Building age > 50 years | +15 |
| UW-006 | Building age 31–50 | +10 |
| UW-007 | Building age 21–30 | +5 |
| UW-008 | Building age < 5 | −5 |
| UW-009 | Restaurant occupancy | +12 |
| UW-010 | Manufacturing-Heavy occupancy | +15 |
| UW-011 | Office occupancy | −5 |
| UW-012 | Sprinklers installed | −10 |
| UW-013 | Alarm system installed | −5 |
| UW-014 | Per prior claim | +8 per claim |
| UW-015 | Prior claims total > $100K | +10 |
| UW-016 | High-cat state (FL, CA, LA, TX) | +10 |
| UW-017 | Roof age > 20 years | +12 |
| UW-018 | Roof age < 5 years | −3 |
| UW-019 | Property value > $5M | +8 |

Score range: clamped to 0–100.

---

### 2.2 Decision Thresholds

| ID | Rule | Decision |
|---|---|---|
| UW-020 | Risk score > 85 | **Declined** |
| UW-021 | High cat exposure (PML > 50% of property value) | **Declined** |
| UW-022 | Risk score > 70 | **ReferToSenior** |
| UW-023 | Prior claims ≥ 3 | **ReferToSenior** |
| UW-024 | Risk score > 60 | **RequestMoreInfo** |
| UW-025 | Missing critical information (see below) | **RequestMoreInfo** |
| UW-026 | Risk score ≤ 60, no issues | **Approved** |

---

### 2.3 Missing Information Triggers

| ID | Condition | Source |
|---|---|---|
| UW-030 | Roof age > 20 and roof type is null | `NeedsMoreInformation()` |
| UW-031 | Prior claims > 0 but total amount = 0 | `NeedsMoreInformation()` |
| UW-032 | Property value > $2M and square footage = 0 | `NeedsMoreInformation()` |

---

### 2.4 PML (Probable Maximum Loss) Calculation

| ID | Rule | PML |
|---|---|---|
| UW-040 | Default PML | 25% of property value |
| UW-041 | FL or LA | 60% of property value |
| UW-042 | CA | 50% of property value |
| UW-043 | Sprinklers reduce PML | PML × 0.70 |

---

### 2.5 Risk Component Ratings

**Construction Rating:**

| ID | Condition | Rating |
|---|---|---|
| UW-050 | Fire Resistive + age < 20 | Excellent |
| UW-051 | Frame + age > 50 | Poor |
| UW-052 | Any type + age < 10 | Good |
| UW-053 | Any type + age > 40 | Fair |
| UW-054 | Default | Average |

**Occupancy Rating:**

| ID | Occupancy | Rating |
|---|---|---|
| UW-055 | Office, Educational, Warehouse | Low Risk |
| UW-056 | Restaurant, Manufacturing-Heavy, Hotel | High Risk |
| UW-057 | All others | Average Risk |

**Protection Rating:**

| ID | Condition | Rating |
|---|---|---|
| UW-058 | Sprinklers + Alarm | Superior |
| UW-059 | Sprinklers OR Alarm | Good |
| UW-060 | Neither | Basic |

**Loss History Rating:**

| ID | Condition | Rating |
|---|---|---|
| UW-061 | 0 claims | Loss Free |
| UW-062 | 1 claim < $25K | Favorable |
| UW-063 | ≤ 2 claims < $100K | Average |
| UW-064 | ≥ 3 claims OR > $250K | Poor |
| UW-065 | Other | Below Average |

**Catastrophe Zone Rating:**

| ID | State | Rating |
|---|---|---|
| UW-066 | FL, LA | Extreme |
| UW-067 | CA, TX, NC, SC | High |
| UW-068 | All others | Moderate |

---

### 2.6 Approval Conditions

| ID | Trigger | Condition Applied |
|---|---|---|
| UW-070 | Roof age > 15 years | Roof inspection within 30 days of binding |
| UW-071 | Risk score > 55 | Annual property inspections required |
| UW-072 | Property value > $1M | Agreed value settlement basis |
| UW-073 | No conditions triggered | "Standard terms apply" |

---

### 2.7 Auto-Decline Rules (UnderwritingRules static)

| ID | Rule | Source |
|---|---|---|
| UW-080 | Building age > 75 AND Frame AND ≥ 3 prior claims | `UnderwritingRules.IsAutoDecline()` |
| UW-081 | Catastrophe PML > 60% of property value | `UnderwritingRules.IsAutoDecline()` |
| UW-082 | More than 5 prior claims | `UnderwritingRules.IsAutoDecline()` |

---

### 2.8 Senior Underwriter Referral Rules (UnderwritingRules static)

| ID | Rule | Source |
|---|---|---|
| UW-085 | Property value > $5M | `UnderwritingRules.RequiresSeniorUnderwriterReview()` |
| UW-086 | Building age > 100 | `UnderwritingRules.RequiresSeniorUnderwriterReview()` |
| UW-087 | Manufacturing-Heavy or Restaurant occupancy | `UnderwritingRules.RequiresSeniorUnderwriterReview()` |
| UW-088 | Risk score > 70 | `UnderwritingRules.RequiresSeniorUnderwriterReview()` |

---

### 2.9 Property Inspection Rules (UnderwritingRules static)

| ID | Rule | Source |
|---|---|---|
| UW-090 | Building age > 30 | `UnderwritingRules.RequiresPropertyInspection()` |
| UW-091 | Roof age > 15 | `UnderwritingRules.RequiresPropertyInspection()` |
| UW-092 | Property value > $3M | `UnderwritingRules.RequiresPropertyInspection()` |
| UW-093 | Prior claims ≥ 2 | `UnderwritingRules.RequiresPropertyInspection()` |

---

## 3. Compliance Rules (ComplianceService)

### 3.1 California

| ID | Rule | Source |
|---|---|---|
| CS-001 | Premium below $500 → non-compliant (Prop 103) | `ValidateCaliforniaCompliance()` |
| CS-002 | ZIP 90–95: EQ coverage should be offered (warning) | `ValidateCaliforniaCompliance()` |
| CS-003 | Property value > $3M: may require FAIR Plan excess (warning) | `ValidateCaliforniaCompliance()` |

### 3.2 Florida

| ID | Rule | Source |
|---|---|---|
| CS-010 | Building age > 30 AND roof age > 15 → non-compliant | `ValidateFloridaCompliance()` |
| CS-011 | Coastal ZIP (32, 33, 34): wind mitigation inspection required (warning) | `ValidateFloridaCompliance()` |
| CS-012 | Coastal + value > $500K + no sprinklers → non-compliant | `ValidateFloridaCompliance()` |
| CS-013 | Coastal + coverage > $700K: Citizens participation (warning) | `ValidateFloridaCompliance()` |

### 3.3 Texas

| ID | Rule | Source |
|---|---|---|
| CS-020 | Asphalt shingle + built after 2015: Class 4 shingles discount (warning) | `ValidateTexasCompliance()` |
| CS-021 | Deductible < 1% of property value → non-compliant | `ValidateTexasCompliance()` |

### 3.4 New York

| ID | Rule | Source |
|---|---|---|
| CS-030 | Value > $1M + missing address → non-compliant | `ValidateNewYorkCompliance()` |
| CS-031 | Coverage < 80% of replacement cost → non-compliant | `ValidateNewYorkCompliance()` |

### 3.5 Louisiana

| ID | Rule | Source |
|---|---|---|
| CS-040 | Coastal parish (ZIP 70, 71): Citizens participation (warning) | `ValidateLouisianaCompliance()` |
| CS-041 | Coastal + deductible < 2% of value → non-compliant | `ValidateLouisianaCompliance()` |

### 3.6 General (All States)

| ID | Rule | Source |
|---|---|---|
| CS-050 | Coverage limit < $100K → non-compliant | `ValidateGeneralCompliance()` |
| CS-051 | Deductible > 10% of property value → non-compliant | `ValidateGeneralCompliance()` |

---

### 3.7 State Regulatory Requirements (ComplianceRules static)

| ID | Rule | Source |
|---|---|---|
| CS-060 | Flood insurance disclosure required: FL, LA, TX, NC, SC, GA, AL, MS | `ComplianceRules.RequiresFloodInsuranceDisclosure()` |
| CS-061 | Earthquake insurance must be offered: CA, WA, OR, AK | `ComplianceRules.RequiresEarthquakeInsuranceOffer()` |
| CS-062 | Cancellation notice: CA=75 days, NY=60, FL=45, default=30 | `ComplianceRules.GetMaximumCancellationNoticeDays()` |

---

## 4. Premium Calculator Rules (PremiumCalculator)

### 4.1 Prorated Premium

| ID | Rule | Source |
|---|---|---|
| PC-001 | Prorated premium = annualPremium / 365 × days in term | `CalculateProratedPremium()` |

### 4.2 Return Premium (Cancellation)

| ID | Rule | Source |
|---|---|---|
| PC-010 | ProRata: return = annual × (daysRemaining / 365) | `CalculateReturnPremium()` |
| PC-011 | ShortRate: return = proRata amount × 0.90 (10% penalty) | `CalculateReturnPremium()` |
| PC-012 | Flat: return = $0 | `CalculateReturnPremium()` |

### 4.3 Installment Amounts

| ID | Payment Plan | Calculation |
|---|---|---|
| PC-020 | Annual | annualPremium (no fee) |
| PC-021 | Semi-Annual | (annual / 2) × 1.03 (3% fee) |
| PC-022 | Quarterly | (annual / 4) × 1.05 (5% fee) |
| PC-023 | Monthly | (annual / 12) × 1.08 (8% fee) |

---

## 5. Endorsement Rules (EndorsementService)

### 5.1 Coverage Limit Change

| ID | Rule | Source |
|---|---|---|
| EN-001 | Premium change = pro-rated difference based on limit ratio | `CalculatePremiumAdjustment()` |
| EN-002 | Limit ratio = newLimit / currentLimit | `CalculatePremiumAdjustment()` |

### 5.2 Deductible Change

| ID | Rule | Source |
|---|---|---|
| EN-010 | Higher deductible → lower premium; lower → higher | `CalculateDeductibleFactor()` |
| EN-011 | Deductible credit schedule: ≥5%=0.70, ≥3%=0.80, ≥2%=0.85, ≥1%=0.90, ≥0.5%=0.95, <0.5%=1.00 | `GetDeductibleCredit()` |
| EN-012 | Premium change = annual × ((newCredit/currentCredit) − 1), pro-rated | `CreateDeductibleChange()` |

### 5.3 Cancellation Endorsement

| ID | Rule | Source |
|---|---|---|
| EN-020 | Insured Request → ShortRate cancellation (10% penalty) | `CreateCancellationEndorsement()` |
| EN-021 | All other reasons → ProRata cancellation (full return) | `CreateCancellationEndorsement()` |
| EN-022 | Premium change = negative of return premium | `CreateCancellationEndorsement()` |

---

## 6. Policy Rules (PolicyService)

| ID | Rule | Source |
|---|---|---|
| PO-001 | Only approved quotes can be bound to policies | `IssuePolicy()` |
| PO-002 | Policy term = 1 year from effective date | `IssuePolicy()` |
| PO-003 | Only active policies can be cancelled | `CancelPolicy()` |
| PO-004 | Only active policies can be renewed | `RenewPolicy()` |
| PO-005 | Reinsurance ceded if property value > $2M | `DetermineReinsuranceCession()` |
| PO-006 | Ceded premium = 60% of total premium | `CalculateCededPremium()` |
| PO-007 | Old policy status set to "Renewed" on renewal | `RenewPolicy()` |

---

## 7. Renewal Rules (RenewalService)

| ID | Rule | Source |
|---|---|---|
| RN-001 | Roof age incremented by 1 at renewal | `GenerateRenewalQuote()` |
| RN-002 | Property value inflated by 3% at renewal | `ApplyRenewalFactors()` |
| RN-003 | Coverage limit inflated by 3% at renewal | `ApplyRenewalFactors()` |
| RN-004 | FL trend factor: +8% | `GetTrendFactor()` |
| RN-005 | CA trend factor: +12% | `GetTrendFactor()` |
| RN-006 | TX trend factor: +5% | `GetTrendFactor()` |
| RN-007 | LA trend factor: +10% | `GetTrendFactor()` |
| RN-008 | Default trend factor: +3% | `GetTrendFactor()` |
| RN-009 | Renewal quote number = "RNW-{policyNumber}" | `GenerateRenewalQuote()` |
| RN-010 | Renewal quote expiration = 30 days from creation | `GenerateRenewalQuote()` |

---

## 8. Rating Eligibility Rules (RatingRules static)

| ID | Rule | Source |
|---|---|---|
| RR-001 | Preferred rate: building age < 20, Fire/Mod Fire Resistive, sprinklers, 0 claims | `RatingRules.IsEligibleForPreferredRate()` |
| RR-002 | Wind mitigation inspection: FL, LA, TX, NC, SC | `RatingRules.RequiresWindMitigationInspection()` |
| RR-003 | High-value property: value > $2M | `RatingRules.IsHighValueProperty()` |
| RR-004 | Minimum deductible FL > $500K: 2% of value | `RatingRules.GetMinimumDeductible()` |
| RR-005 | Minimum deductible TX: 1% of value | `RatingRules.GetMinimumDeductible()` |
| RR-006 | Default minimum deductible: $1,000 | `RatingRules.GetMinimumDeductible()` |

---

## Summary

| Category | Rule Count |
|---|---|
| Premium Calculation (QuotingEngine) | 85 |
| Underwriting (UnderwritingService + Rules) | 42 |
| Compliance (ComplianceService + Rules) | 18 |
| Premium Calculator | 7 |
| Endorsement | 8 |
| Policy | 7 |
| Renewal | 10 |
| Rating Eligibility | 6 |
| **Total** | **183** |
