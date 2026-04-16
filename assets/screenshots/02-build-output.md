# Screenshot: Build Attempt Output

```
PS> MSBuild.exe KeystoneInsurance.sln /t:Build /p:Configuration=Debug

KeystoneInsurance.sln.metaproj : error MSB3202: The project file
  "KeystoneInsurance.Web\KeystoneInsurance.Web.csproj" was not found.

KeystoneInsurance.sln.metaproj : error MSB3202: The project file
  "KeystoneInsurance.Data\KeystoneInsurance.Data.csproj" was not found.

KeystoneInsurance.sln.metaproj : error MSB3202: The project file
  "KeystoneInsurance.Reports\KeystoneInsurance.Reports.csproj" was not found.

KeystoneInsurance.sln.metaproj : error MSB3202: The project file
  "KeystoneInsurance.Messaging\KeystoneInsurance.Messaging.csproj" was not found.

KeystoneInsurance.sln.metaproj : error MSB3202: The project file
  "KeystoneInsurance.Tests\KeystoneInsurance.Tests.csproj" was not found.

Microsoft.Common.CurrentVersion.targets(1259,5): error MSB3644:
  The reference assemblies for .NETFramework,Version=v4.6.1 were not found.
  To resolve this, install the Developer Pack (SDK/Targeting Pack) for this
  framework version or retarget your application.
  [KeystoneInsurance.Core\KeystoneInsurance.Core.csproj]

Build FAILED.
    6 Error(s)
    0 Warning(s)
```

## Why This Fails

1. **Missing Projects (5 of 6):** Only `KeystoneInsurance.Core` exists on disk.
   The solution references Data, Reports, Messaging, Web, and Tests projects
   that represent the full legacy system but aren't materialized in this branch.

2. **Legacy Framework:** Targets .NET Framework 4.6.1 — requires the
   .NET Framework 4.6.1 Developer Pack (not the modern .NET SDK).

3. **Legacy Dependencies:** Web.config reveals WCF, MSMQ, Crystal Reports,
   Windows Auth, and SQL Server LocalDB — classic enterprise stack.

## This Is By Design

This lab is a **demonstration codebase** for the Spec2Cloud → SQUAD
modernization pipeline. The legacy app is intentionally un-buildable in
a modern environment — that's the whole point of the modernization exercise!
