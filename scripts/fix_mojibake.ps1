# Brazilian Portuguese Encoding and Mojibake Fixer

$mojibakeMap = @{
    "Ã§" = "ç";
    "Ã£" = "ã";
    "Ãµ" = "õ";
    "Ã¡" = "á";
    "Ã©" = "é";
    "Ã­" = "í";
    "Ã³" = "ó";
    "Ãº" = "ú";
    "Ã‚" = "Â";
    "ÃŠ" = "Ê";
    "ÃŽ" = "Î";
    "Ã”" = "Ô";
    "Ã›" = "Û";
    "Ã€" = "À";
    "Ã‰" = "É";
    "Ã" = "Í";
    "Ã“" = "Ó";
    "Ãš" = "Ú";
    "Ã‡" = "Ç";
    "Ãƒ" = "Ã";
    "Ã•" = "Õ";
    "Ã¬" = "ì";
    "â€“" = "–";
    "â€”" = "—";
    "Âº" = "º";
    "Âª" = "ª";
    "Ãª" = "ê"
}

$targetDirs = @("Client\GestionProduccion.Client\Pages", "Client\GestionProduccion.Client\Layout", "Client\GestionProduccion.Client\Components", "Client\GestionProduccion.Client\Resources")
$extensions = @("*.razor", "*.cs")

foreach ($dir in $targetDirs) {
    if (Test-Path $dir) {
        $files = Get-ChildItem -Path $dir -Recurse -Include $extensions
        foreach ($file in $files) {
            Write-Host "Repairing encoding and mojibake in: $($file.FullName)"
            
            # Read content as UTF8
            $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
            
            # Replace mojibake patterns
            foreach ($key in $mojibakeMap.Keys) {
                if ($content.Contains($key)) {
                    $content = $content.Replace($key, $mojibakeMap[$key])
                }
            }
            
            # Write back as UTF-8 WITH BOM (using [System.Text.Encoding]::UTF8 ensures BOM in .NET)
            [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.Encoding]::UTF8)
        }
    }
}

Write-Host "Repairs completed. All files are now UTF-8 with BOM and mojibake-free."
