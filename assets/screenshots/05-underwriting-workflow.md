# Screenshot: Underwriting Decision Workflow

## Risk Score Calculation (UnderwritingService.cs — 296 lines)

```
Risk Score: 0 ──────────────────────────────── 100
             │         │         │         │
             Low       Medium    High      Extreme
             (<60)     (60-70)   (70-85)   (>85)
             │         │         │         │
             ▼         ▼         ▼         ▼
          APPROVED  REQUEST   REFER TO   DECLINED
                    MORE INFO  SENIOR UW
```

## Score Modifiers

```
Base Score:                    50 points
─────────────────────────────────────────
Construction Type:
  Frame                       +15
  Joisted Masonry             +10
  Non-Combustible              +5
  Fire Resistive               -5

Building Age:
  > 50 years                  +15
  > 30 years                  +10
  > 20 years                   +5
  < 5 years                    -5

Occupancy:
  Manufacturing-Heavy         +15
  Restaurant                  +12
  Office                       -5

Protection:
  Sprinklers installed        -10
  Alarm system installed       -5

Loss History:
  Each prior claim             +8
  Claims > $100K total        +10

Catastrophe Exposure:
  FL, CA, LA, TX states       +10

Roof Condition:
  Roof age > 20 years         +12
  Roof age < 5 years           -3

High-Value Property:
  Property value > $5M         +8
```

## Decision Outcomes

| Score Range | Decision         | Action Required                    |
|-------------|------------------|------------------------------------|
| 0–60        | **Approved**     | Standard terms, optional conditions|
| 60–70       | **Request Info** | Roof inspection, loss runs, etc.   |
| 70–85       | **Refer Senior** | Senior underwriter must review     |
| 85–100      | **Declined**     | Auto-decline with documented reasons|

## Catastrophe PML (Probable Maximum Loss)

```
Base PML:        25% of property value
FL/LA exposure:  60% of property value
CA exposure:     50% of property value
Sprinkler credit: PML × 0.70
```

High Cat Exposure threshold: PML > 50% of property value → AUTO-DECLINE
