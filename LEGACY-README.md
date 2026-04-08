# Keystone Insurance - Legacy Application

## Quick Start Guide

### Prerequisites
- Visual Studio 2022
- .NET Framework 4.6.1
- SQL Server 2012 or later (LocalDB works)

### Setup Steps

1. **Database Setup**
   ```bash
   cd database
   sqlcmd -S (localdb)\mssqllocaldb -i schema.sql
   sqlcmd -S (localdb)\mssqllocaldb -i seed-data.sql
   sqlcmd -S (localdb)\mssqllocaldb -i stored-procedures.sql
   ```

2. **Build Solution**
   ```bash
   # Open in Visual Studio
   # Build > Build Solution (Ctrl+Shift+B)
   ```

3. **Run Application**
   ```bash
   # Press F5 in Visual Studio
   # Application will open at http://localhost:port
   ```

### Key Features to Explore

1. **Quote Generation**
   - Navigate to Quotes > New Quote
   - Enter property details (Address, Construction Type, Occupancy, etc.)
   - System calculates premium using 40+ rating factors
   - Review premium calculation breakdown

2. **Underwriting**
   - View pending quotes
   - See risk score calculation
   - Approve or decline quotes based on guidelines

3. **Policy Issuance**
   - Bind approved quote to policy
   - Select payment plan
   - System queues async document generation

### Sample Data

The seed data includes:
- 5 sample clients (Manufacturing, Office, Retail, Tech, Restaurant)
- Rate factors for all 50 states
- Construction type factors
- Occupancy class factors
- Sample quote with underwriting decision

### Testing Premium Calculations

Try these scenarios to see the complexity:

1. **California Earthquake Risk**
   - State: CA, ZIP: 94102
   - Construction: Frame
   - Property Value: $1,500,000
   - Expected: High premium due to EQ exposure

2. **Florida Hurricane Risk**
   - State: FL, ZIP: 33139
   - Construction: Masonry Non-Combustible
   - Sprinklers: Yes
   - Expected: Hurricane surcharge, sprinkler credit

3. **Texas Hail Zone**
   - State: TX, ZIP: 78701
   - Roof Age: 18 years
   - Expected: Roof age surcharge

### Business Rules to Understand

1. **Premium Calculation Factors** (40+)
   - Property value tiers
   - Construction type (6 classes)
   - Building age (8 age bands)
   - Occupancy classification
   - Protection class (sprinklers, alarms)
   - Territory and catastrophe zones
   - Roof type and age
   - Square footage economies of scale
   - Number of stories
   - Loss history (claims frequency and severity)
   - Deductible credits
   - State-specific adjustments
   - Minimum premiums by state
   - Surcharges and taxes

2. **State-Specific Compliance**
   - California: Proposition 103, earthquake disclosure
   - Florida: Wind mitigation, roof age restrictions
   - Texas: Wind/hail deductible minimums
   - New York: Coverage to value requirements
   - Louisiana: Coastal parish regulations

3. **Underwriting Guidelines**
   - Risk score calculation (0-100 scale)
   - Auto-decline thresholds
   - Senior underwriter referral criteria
   - Required inspections
   - Catastrophe exposure limits (PML)

### Architecture Notes

This is a classic legacy enterprise application with:
- **Tight coupling** between layers
- **Database-first** EF approach with 100+ entity EDMX
- **Stored procedures** containing business logic
- **WCF services** for integration
- **Crystal Reports** for document generation
- **MSMQ** for async processing
- **Windows Authentication**
- **jQuery UI** for client-side interactivity

These are the exact challenges that Spec2Cloud + SQUAD are designed to address!

### For the Lab

When you run Spec2Cloud against this codebase, it should identify:
- ~150+ business rules in the QuotingEngine alone
- 50 state-specific compliance variations
- Complex actuarial formulas
- Integration points with external systems
- Workflow state machines (Quote → UW → Policy → Endorsement)

This makes it an ideal candidate for the capstone modernization lab.
