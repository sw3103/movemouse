# https://github.com/sw3103/movemouse/wiki/Scenarios#reactivate-previous-active-window-on-pausestop

$TextFilePath = Join-Path -Path:$env:TEMP -ChildPath:"MoveMouseWindowPid.txt"

if (Test-Path -Path:$TextFilePath) {
    $WindowPid = Get-Content -Path:$TextFilePath

    if (![System.String]::IsNullOrWhiteSpace($WindowPid)) {
        Add-Type -AssemblyName:"Microsoft.VisualBasic"
        [Microsoft.VisualBasic.Interaction]::AppActivate([int]$WindowPid)
    }
    else {
        throw "$TextFilePath was empty."
    }
}
else {
    throw "$TextFilePath does not exist. You need to run this script with the -Capture parameter first."
}