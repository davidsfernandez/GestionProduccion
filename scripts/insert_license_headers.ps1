# Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
# 
# This software and its associated documentation files are the exclusive property 
# of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
# or use of this software, via any medium, is strictly prohibited.
# 
# Proprietary and Confidential.

# License Header to insert
$licenseHeader = @"
/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */
"@ + [System.Environment]::NewLine + [System.Environment]::NewLine

# File extensions to target
$extensions = @("*.cs", "*.razor", "*.js", "*.css", "*.sql", "*.ps1", "*.sh", "*.yml", "*.yaml")

# Directories to exclude
$excludeDirs = @(".git", ".gemini", "bin", "obj", "node_modules", "wwwroot/lib")

# Get all target files
$files = Get-ChildItem -Recurse -Include $extensions | Where-Object { 
    $path = $_.FullName
    $excluded = $false
    foreach ($dir in $excludeDirs) {
        if ($path -like "*\$dir\*") {
            $excluded = $true
            break
        }
    }
    return -not $excluded
}

# Prepend license header to each file
foreach ($file in $files) {
    Write-Host "Updating $($file.FullName)..."
    $content = Get-Content -Path $file.FullName -Raw
    
    # Check if header already exists to avoid duplication (simple check)
    if (-not $content.StartsWith("/*`r`n * Copyright (c) 2026 David Fernandez Garzon")) {
        $newContent = $licenseHeader + $content
        Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8
    }
}

Write-Host "License headers updated successfully."


