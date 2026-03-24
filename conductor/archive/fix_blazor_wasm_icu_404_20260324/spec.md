# Specification: Fix Blazor WASM icudt_no_CJK.dat 404/SRI failure

## Objective
Resolve the critical issue preventing access for new devices where `_framework/icudt_no_CJK.dat` fails to load with a 404 error and SRI integrity failure in the production environment.

## Reported Issue
- **Resource:** `https://leveloperacional.cloud/_framework/icudt_no_CJK.dat`
- **Error:** `404 (Not Found)` and `Failed to find a valid digest in the 'integrity' attribute`.
- **Symptoms:** System fails to start on devices that haven't cached the assets previously.

## Analysis
This issue typically occurs in Blazor WebAssembly applications when:
1.  The resource `icudt_no_CJK.dat` is missing from the server's `_framework/` directory.
2.  The `blazor.boot.json` specifies an integrity hash for this file, but the file on the server differs or is blocked.
3.  The application is configured to use globalization data that is not correctly deployed.

## Solution Strategy
1.  **Audit Globalization Configuration:** Check the `.csproj` and `Program.cs` for globalization settings (`BlazorWebAssemblyLoadAllGlobalizationData`, `InvariantGlobalization`, etc.).
2.  **Verify Asset Inclusion:** Ensure the ICU data files are correctly included in the build/publish process.
3.  **SRI Fix:** If the file exists but has the wrong hash, we may need to disable integrity checks for specific assets or ensure they match correctly after deployment.
4.  **Deployment Check:** Verify that the server (Azure/Docker/Nginx) correctly serves `.dat` files with the appropriate MIME types.

## Constraints
- No functional or aesthetic changes.
- Focus solely on resolving the loading failure.