$ErrorActionPreference = "Stop"

$renames = @{
    "Ideally use both. otherwise, pref EFleck_Line41_E.wav Fleck_Line41_F.wav" = "Fleck_Line41_E.wav"
    "All three are great. use all three if possible. otherwise, C is pref Fleck_Line45_A.wav Fleck_Line45_B.wavFleck_Line45_C.wav" = "Fleck_Line45_C.wav"
    "Ideally both, otherwise pref is AFleck_Line46_A.wav Fleck_Line46_D.wav" = "Fleck_Line46_A.wav"
    "If we can use all four lets do it. Otherwise, pref is AFleck_Line63_A.wav Fleck_Line63_B.wav Fleck_Line63_C.wav Fleck_Line63_D.wav" = "Fleck_Line63_A.wav"
    "Both or AFleck_Line64_A.wav Fleck_Line64_B.wav" = "Fleck_Line64_A.wav"
    "Both or B Fleck_Line67_A.wav Fleck_Line67_B.wav" = "Fleck_Line67_B.wav"
    "C or BothFleck_Line68_B.wav Fleck_Line68_C.wav" = "Fleck_Line68_C.wav"
    "Both or AFleck_Line116_A.wav Fleck_Line116_B.wav" = "Fleck_Line116_A.wav"
}

$folder = Join-Path $PSScriptRoot "..\..\Assets\Resources\Barks\Audio\Fleck"
$folder = (Resolve-Path $folder).Path

foreach ($pair in $renames.GetEnumerator()) {
    $source = Join-Path $folder $pair.Key
    $destination = Join-Path $folder $pair.Value

    if (Test-Path -LiteralPath $source) {
        if (Test-Path -LiteralPath $destination) {
            throw "Destination already exists: $destination"
        }

        Rename-Item -LiteralPath $source -NewName $pair.Value
        Write-Host "Renamed $($pair.Key) -> $($pair.Value)"
    }
}

$cutLine = Join-Path $folder "Cut LineFleck_Line221_B.wav"
if (Test-Path -LiteralPath $cutLine) {
    Remove-Item -LiteralPath $cutLine
    Write-Host "Removed Cut LineFleck_Line221_B.wav"
}
