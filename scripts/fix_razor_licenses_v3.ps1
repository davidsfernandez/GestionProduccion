# Simple Fix License Headers for Razor Files

$razorHeader = "@*`r`n * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.`r`n * `r`n * This software and its associated documentation files are the exclusive property `r`n * of David Fernandez Garzon. Unauthorized copying, modification, distribution, `r`n * or use of this software, via any medium, is strictly prohibited. `r`n * `r`n * Proprietary and Confidential.`r`n *@`r`n`r`n"

$wrongHeaderStart = "/*`r`n * Copyright (c) 2026 David Fernandez Garzon"

# Get all .razor files
$files = Get-ChildItem -Recurse -Include "*.razor" | Where-Object { 
    $_.FullName -notmatch "bin|obj|node_modules|.git|.gemini"
}

foreach ($file in $files) {
    Write-Host "Processing $($file.FullName)..."
    $content = Get-Content -Path $file.FullName -Raw
    
    # Remove wrong header if it exists (very simple check)
    if ($content.StartsWith("/*") -and $content.Contains("Copyright (c) 2026 David Fernandez Garzon")) {
        $endIdx = $content.IndexOf("*/")
        if ($endIdx -ge 0) {
            Write-Host "Removing wrong header from $($file.Name)"
            $content = $content.Substring($endIdx + 2).TrimStart()
        }
    }
    
    # Add correct header if not present
    if (-not $content.StartsWith("@*`r`n * Copyright (c) 2026 David Fernandez Garzon")) {
        Write-Host "Adding correct header to $($file.Name)"
        $newContent = $razorHeader + $content
        Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8
    }
}

Write-Host "Done."
