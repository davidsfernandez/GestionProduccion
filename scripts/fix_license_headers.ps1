# Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
# 
# This software and its associated documentation files are the exclusive property 
# of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
# or use of this software, via any medium, is strictly prohibited.
# 
# Proprietary and Confidential.

# Corrected License Headers Script

$cStyleHeader = @"
/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */
"@

$hashStyleHeader = @"
# Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
# 
# This software and its associated documentation files are the exclusive property 
# of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
# or use of this software, via any medium, is strictly prohibited.
# 
# Proprietary and Confidential.
"@

# Extension mappings
$cExtensions = @(".cs", ".razor", ".js", ".css", ".sql")
$hashExtensions = @(".yml", ".yaml", ".sh", ".ps1", "Dockerfile")

$excludeDirs = @(".git", ".gemini", "bin", "obj", "node_modules", "wwwroot/lib")

function Update-FileHeader($file, $header) {
    $content = Get-Content -Path $file.FullName -Raw
    
    # Remove any existing wrong header (both styles)
    if ($content.StartsWith("/*`r`n * Copyright (c) 2026 David Fernandez Garzon") -or 
        $content.StartsWith("# Copyright (c) 2026 David Fernandez Garzon")) {
        
        # Simple way to remove the first block comment or hash block
        if ($content.StartsWith("/*")) {
            $endIdx = $content.IndexOf("*/")
            if ($endIdx -ge 0) {
                $content = $content.Substring($endIdx + 2).TrimStart()
            }
        } elseif ($content.StartsWith("#")) {
            # Remove consecutive lines starting with #
            $lines = $content -split "`r?`n"
            $firstNonHash = 0
            while ($firstNonHash -lt $lines.Count -and $lines[$firstNonHash].StartsWith("#")) {
                $firstNonHash++
            }
            $content = ($lines[$firstNonHash..($lines.Count-1)] -join [System.Environment]::NewLine).TrimStart()
        }
    }

    $newContent = $header + [System.Environment]::NewLine + [System.Environment]::NewLine + $content
    Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8
}

# Process Files
$allFiles = Get-ChildItem -Recurse -File | Where-Object { 
    $path = $_.FullName
    $excluded = $false
    foreach ($dir in $excludeDirs) { if ($path -like "*\$dir\*") { $excluded = $true; break } }
    return -not $excluded
}

foreach ($file in $allFiles) {
    if ($cExtensions -contains $file.Extension) {
        Write-Host "Applying C-style to $($file.Name)"
        Update-FileHeader $file $cStyleHeader
    } elseif ($hashExtensions -contains $file.Extension -or $file.Name -eq "Dockerfile") {
        Write-Host "Applying Hash-style to $($file.Name)"
        Update-FileHeader $file $hashStyleHeader
    }
}

Write-Host "Audit and correction completed."

