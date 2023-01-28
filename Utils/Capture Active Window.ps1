# https://github.com/sw3103/movemouse/wiki/Scenarios#reactivate-previous-active-window-on-pausestop

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public class NativeMethods
{
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError=true)]
    public static extern uint GetWindowThreadProcessId(
        IntPtr hWnd, 
        out uint lpdwProcessId);
}
"@

$TextFilePath = Join-Path -Path:$env:TEMP -ChildPath:"MoveMouseWindowPid.txt"
$Hwnd = [NativeMethods]::GetForegroundWindow()
$WindowPid = 0

if ([NativeMethods]::GetWindowThreadProcessId($Hwnd, [ref]$WindowPid) -gt 0) {
    $WindowPid | Out-File -FilePath:$TextFilePath -Append:$false -Force
}
else {
    throw "Error capturing active window process ID."
}