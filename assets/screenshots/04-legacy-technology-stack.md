# Screenshot: Legacy Technology Stack Analysis

## Web.config — Legacy Integration Points

```xml
<!-- Target Framework -->
<compilation debug="true" targetFramework="4.6.1" />
<httpRuntime targetFramework="4.6.1" />

<!-- Authentication: Windows (AD-integrated) -->
<authentication mode="Windows" />

<!-- WCF Service Binding (Reinsurance Partner) -->
<system.serviceModel>
  <bindings>
    <basicHttpBinding>
      <binding name="ReinsuranceServiceBinding" maxReceivedMessageSize="2147483647">
        <security mode="Transport" />
      </binding>
    </basicHttpBinding>
  </bindings>
</system.serviceModel>

<!-- MSMQ Queues (async document generation) -->
<add key="PolicyIssuanceQueuePath" value=".\private$\policyissuance" />
<add key="EndorsementQueuePath" value=".\private$\endorsements" />

<!-- SQL Server LocalDB -->
connectionString="Server=(localdb)\mssqllocaldb;Database=KeystoneInsurance;
                  Trusted_Connection=True;"
```

## Legacy Stack Summary

| Component            | Legacy Technology       | Modern Target              |
|----------------------|------------------------|----------------------------|
| Runtime              | .NET Framework 4.6.1   | .NET 9 / ASP.NET Core      |
| Web Framework        | ASP.NET MVC 4          | Blazor / Minimal APIs       |
| Data Access          | Entity Framework (EDMX) | EF Core (Code-First)       |
| Messaging            | MSMQ                   | Azure Service Bus           |
| Services             | WCF / basicHttpBinding | REST APIs / gRPC            |
| Reporting            | Crystal Reports        | Azure Blob + PDF generation |
| Authentication       | Windows Auth (NTLM)    | Azure AD / Entra ID         |
| Database             | SQL Server (LocalDB)   | Azure SQL Database          |
| Hosting              | IIS on Windows Server  | Azure Container Apps        |
| Client-side          | jQuery UI              | Modern JS framework         |

## Database Schema (8 Tables, 486 Lines SQL)

- `Clients` — Business accounts with credit scoring
- `Quotes` — 30+ fields for property insurance quotes
- `UnderwritingDecisions` — Risk assessment and approval workflow
- `Policies` — Active insurance policies with payment plans
- `Endorsements` — Mid-term policy modifications
- `RateFactors` — Premium calculation lookup tables
- `CoverageOptions` — Available coverage types
- `AuditLog` — Change tracking across all entities
