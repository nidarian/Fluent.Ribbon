---
title: WpfAnalyzers Reference
description: Roslyn analyzer for catching common WPF mistakes at compile time
tags: [wpfanalyzers, roslyn, code-analysis, reference]
see_also:
  - fluent-ribbon-troubleshooting.md
  - fluent-ribbon-lessons-learned-v2.md
---

# WpfAnalyzers Reference (v2)

**A Roslyn analyzer for catching common WPF mistakes at compile time.**

*v2 Changes: Verified version (4.1.0 is latest stable as of Jan 2026), confirmed documentation current*

GitHub: https://github.com/DotNetAnalyzers/WpfAnalyzers

---

## What It Does

WpfAnalyzers inspects your WPF code during compilation and warns about:
- DependencyProperty naming/registration mistakes
- Binding path typos
- Callback naming conventions
- CLR property wrapper issues
- Resource dictionary problems

**No runtime impact** - only runs at compile time.

---

## Quick Install

### Add to Single Project

```bash
cd YourProject
dotnet add package WpfAnalyzers
```

Or in `.csproj`:

```xml
<PackageReference Include="WpfAnalyzers" Version="4.1.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>build;analyzers</IncludeAssets>
</PackageReference>
```

### Add to All Projects (Solution-Wide)

In `Directory.Build.props` at solution root:

```xml
<Project>
  <ItemGroup>
    <PackageReference Include="WpfAnalyzers" PrivateAssets="all" IncludeAssets="build;analyzers" />
  </ItemGroup>
</Project>
```

---

## Quick Remove

### Remove from Single Project

```bash
dotnet remove package WpfAnalyzers
```

Or delete from `.csproj`:

```xml
<!-- Delete this -->
<PackageReference Include="WpfAnalyzers" ... />
```

### Remove from Solution

Delete from `Directory.Build.props`.

---

## Common Rules

| Rule | What It Checks |
|------|----------------|
| WPF0001 | Backing field name should match registered name |
| WPF0002 | Backing field should be static readonly |
| WPF0005 | PropertyChangedCallback name should match property |
| WPF0006 | CoerceValueCallback name should match property |
| WPF0012 | CLR property type should match registered type |
| WPF0013 | CLR accessor should match registered accessibility |
| WPF0030 | Backing field for DependencyPropertyKey should be named correctly |
| WPF0035 | Use SetValue in CLR setter, not SetCurrentValue |
| WPF0041 | Set mutable members to ReadLocalValue in clone |
| WPF0060 | Backing member should have standard documentation |
| WPF0062 | Property not found for binding path |

Full list: https://github.com/DotNetAnalyzers/WpfAnalyzers#rules

---

## Tuning: Suppress Specific Warnings

### Method 1: In Code (Single Instance)

```csharp
#pragma warning disable WPF0060 // Backing member should have standard docs
public static readonly DependencyProperty MyProperty = ...
#pragma warning restore WPF0060
```

### Method 2: In Project File (Entire Project)

```xml
<PropertyGroup>
    <NoWarn>$(NoWarn);WPF0060</NoWarn>
</PropertyGroup>
```

### Method 3: In .editorconfig (Recommended)

Create or edit `.editorconfig` in project/solution root:

```ini
# WpfAnalyzers settings
[*.cs]

# Disable documentation warning
dotnet_diagnostic.WPF0060.severity = none

# Downgrade to suggestion
dotnet_diagnostic.WPF0001.severity = suggestion

# Make it an error (strict mode)
dotnet_diagnostic.WPF0012.severity = error
```

Severity levels:
- `error` - Fails build
- `warning` - Shows warning
- `suggestion` - Info/hint
- `silent` - Runs but hidden
- `none` - Disabled completely

---

## Tuning: Common Suppressions

For existing projects with lots of warnings, start lenient:

```ini
# .editorconfig - Lenient start for existing projects
[*.cs]

# Documentation - usually too noisy
dotnet_diagnostic.WPF0060.severity = none

# Naming conventions - fix gradually
dotnet_diagnostic.WPF0001.severity = suggestion
dotnet_diagnostic.WPF0005.severity = suggestion

# Type mismatches - keep as warnings (important)
dotnet_diagnostic.WPF0012.severity = warning

# Binding errors - keep as warnings (catches typos)
dotnet_diagnostic.WPF0062.severity = warning
```

---

## Trying Without Committing

### Option 1: Temporary Install

```bash
# Add it
dotnet add package WpfAnalyzers

# Build and see warnings
dotnet build 2>&1 | grep -i "WPF[0-9]"

# Remove if too noisy
dotnet remove package WpfAnalyzers

# Undo changes to csproj
git checkout -- *.csproj
```

### Option 2: Global Tool (Analyze Without Modifying Project)

Not available for WpfAnalyzers specifically, but you can:
1. Create a branch
2. Add package
3. Evaluate
4. Delete branch if unwanted

```bash
git checkout -b try-wpfanalyzers
dotnet add package WpfAnalyzers
dotnet build
# Evaluate...
git checkout main
git branch -D try-wpfanalyzers
```

---

## Fluent.Ribbon's Setup

Fluent.Ribbon uses WpfAnalyzers with zero warnings. Their setup in `Directory.Build.props`:

```xml
<ItemGroup>
    <PackageReference Include="WpfAnalyzers" PrivateAssets="all" IncludeAssets="build;analyzers" />
</ItemGroup>

<PropertyGroup>
    <!-- They don't suppress any WPF rules - clean code -->
    <NoWarn>$(NoWarn);NU1701;NU1603;NU1605;SA1652;WFAC010</NoWarn>
</PropertyGroup>
```

---

## Workflow: Adding to Existing Project

1. **Add package**
   ```bash
   dotnet add package WpfAnalyzers
   ```

2. **Build and count warnings**
   ```bash
   dotnet build 2>&1 | grep -c "warning WPF"
   ```

3. **If overwhelming (50+), create lenient .editorconfig**
   ```ini
   [*.cs]
   dotnet_diagnostic.WPF0060.severity = none
   dotnet_diagnostic.WPF0001.severity = suggestion
   ```

4. **Fix high-priority warnings first**
   - WPF0012 (type mismatches)
   - WPF0062 (binding errors)
   - WPF0035 (SetValue issues)

5. **Tighten rules over time**
   - Change `suggestion` -> `warning`
   - Change `none` -> `suggestion`

6. **Goal: Zero warnings** (like Fluent.Ribbon)

---

## What Each Severity Means for Your Build

| Severity | IDE | Build | CI |
|----------|-----|-------|-----|
| `error` | Red squiggle | Fails | Fails |
| `warning` | Yellow squiggle | Shows warning | Shows warning |
| `suggestion` | Faint dots | Silent | Silent |
| `none` | Nothing | Nothing | Nothing |

---

## Related Analyzers

| Package | Purpose |
|---------|---------|
| WpfAnalyzers | WPF-specific rules |
| StyleCop.Analyzers | Code style (naming, formatting) |
| Microsoft.CodeAnalysis.NetAnalyzers | General .NET best practices |
| Roslynator | 500+ C# refactorings and fixes |
| SonarAnalyzer.CSharp | Security, bugs, code smells |

You can use multiple analyzers together - they stack.

---

## Version History

| Version | Release Date | Key Changes |
|---------|--------------|-------------|
| 4.1.0 | 2023 | Latest stable (recommended) |
| 4.0.0 | 2022 | .NET 6+ support |
| 3.x | 2021 | .NET 5 support |

**Recommended:** Use version 4.1.0 for .NET 6+ projects.

---

## Summary

| Task | Command/Setting |
|------|-----------------|
| Install | `dotnet add package WpfAnalyzers` |
| Remove | `dotnet remove package WpfAnalyzers` |
| See warnings | `dotnet build 2>&1 \| grep "WPF"` |
| Suppress one rule | `<NoWarn>$(NoWarn);WPF0060</NoWarn>` |
| Tune severity | `.editorconfig` with `dotnet_diagnostic.WPFxxxx.severity = ...` |
| Disable in code | `#pragma warning disable WPFxxxx` |
