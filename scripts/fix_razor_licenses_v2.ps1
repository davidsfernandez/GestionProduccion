# Robust Fix License Headers for Razor Files

$razorHeader = @"
@*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 *@
"@ + [System.Environment]::NewLine + [System.Environment]::NewLine

# Get all .razor files
$files = Get-ChildItem -Recurse -Include "*.razor" | Where-Object { 
    $_.FullName -notmatch "bin|obj|node_modules|.git|.gemini"
}

foreach ($file in $files) {
    Write-Host "Processing $($file.FullName)..."
    $content = Get-Content -Path $file.FullName -Raw
    
    # Use regex to remove the first block comment if it matches our license pattern
    $regex = "(?s)^\s*/\*.*?Copyright \(c) 2026 David Fernandez Garzon.*?\*/\s*"
    if ($content -match $regex) {
        Write-Host "Found wrong header in $($file.Name), replacing..."
        $content = $content -replace $regex, ""
    }
    
    # Add the correct Razor style header if not already present
    if ($content -notmatch "@\*.*?Copyright \(c) 2026 David Fernandez Garzon.*?@\*") {
        $newContent = $razorHeader + $content.TrimStart()
        Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8
    }
}

Write-Host "All Razor license headers fixed."
