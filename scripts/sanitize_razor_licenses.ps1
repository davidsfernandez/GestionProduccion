# Final Robust Razor License Fix

$razorHeader = "@*`r`n * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.`r`n * `r`n * This software and its associated documentation files are the exclusive property `r`n * of David Fernandez Garzon. Unauthorized copying, modification, distribution, `r`n * or use of this software, via any medium, is strictly prohibited. `r`n * `r`n * Proprietary and Confidential.`r`n *@`r`n`r`n"

# Get all .razor files
$files = Get-ChildItem -Recurse -Include "*.razor" | Where-Object { 
    $_.FullName -notmatch "bin|obj|node_modules|.git|.gemini"
}

foreach ($file in $files) {
    Write-Host "Cleaning and fixing $($file.Name)..."
    $content = Get-Content -Path $file.FullName -Raw
    
    # Remove ANY leading block comments /* ... */ if they contain the license string
    while ($content.Trim().StartsWith("/*") -and $content.Contains("David Fernandez Garzon")) {
        $endIdx = $content.IndexOf("*/")
        if ($endIdx -ge 0) {
            $content = $content.Substring($endIdx + 2).TrimStart()
        } else {
            break
        }
    }

    # Remove ANY leading Razor comments @* ... *@ if they contain the license string (to avoid double headers)
    while ($content.Trim().StartsWith("@*") -and $content.Contains("David Fernandez Garzon")) {
        $endIdx = $content.IndexOf("*@")
        if ($endIdx -ge 0) {
            $content = $content.Substring($endIdx + 2).TrimStart()
        } else {
            break
        }
    }
    
    # Always prepend the clean Razor header
    $newContent = $razorHeader + $content.TrimStart()
    Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8
}

Write-Host "All Razor files sanitized."
