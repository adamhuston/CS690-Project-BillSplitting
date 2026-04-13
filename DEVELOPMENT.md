# Development & Release Guide

## Building the Solution

```powershell
dotnet build
dotnet run --project BillSplittingUI
```

## Creating a Release

1. **Publish the application**:
   ```powershell
   dotnet publish --configuration Release --output ./publish
   ```

2. **On GitHub**:
   - Go to Releases → Draft a new release
   - Create a tag (e.g., `v1.0.0`)
   - Upload the contents of the `publish` folder
   - Peers can download and run the `.exe` directly without needing .NET

## Git Workflow

Before committing, ensure `.gitignore` excludes:
- `bin/`
- `obj/`
- `publish/`
- `.vs/`
- `*.user`

Commit only source code files (`.sln`, `.csproj`, `.cs`, etc.)
