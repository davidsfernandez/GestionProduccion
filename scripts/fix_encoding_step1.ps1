# Brazilian Portuguese Encoding and Mojibake Fixer (Safe Version)

$targetDirs = @("Client\GestionProduccion.Client\Pages", "Client\GestionProduccion.Client\Layout", "Client\GestionProduccion.Client\Components", "Client\GestionProduccion.Client\Resources")
$extensions = @("*.razor", "*.cs")

# Use regex with byte patterns or direct strings but handle with care
$fixes = @(
    @("Ã§", "ç"),
    @("Ã£", "ã"),
    @("Ãµ", "õ"),
    @("Ã¡", "á"),
    @("Ã©", "é"),
    @("Ã­", "í"),
    @("Ã³", "ó"),
    @("Ãº", "ú"),
    @("Ãª", "ê"),
    @("Ã‡", "Ç"),
    @("Ãƒ", "Ã"),
    @("Ã“", "Ó")
)

foreach ($dir in $targetDirs) {
    if (Test-Path $dir) {
        $files = Get-ChildItem -Path $dir -Recurse -Include $extensions
        foreach ($file in $files) {
            $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::GetEncoding("iso-8859-1"))
            
            # Check if it looks like UTF-8 interpreted as ISO-8859-1
            if ($content.Contains("Ã")) {
                Write-Host "Repairing: $($file.Name)"
                
                # Re-read correctly as UTF8 if possible, or perform manual replacements
                $utf8Content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
                
                # Force save as UTF8 with BOM
                [System.IO.File]::WriteAllText($file.FullName, $utf8Content, [System.Text.Encoding]::UTF8)
            }
        }
    }
}

Write-Host "Step 1: Normalization to UTF-8 with BOM completed."
