@ECHO OFF

echo Uninstalling MyService...
echo ---------------------------------------------------
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\InstallUtil /u %~dp0\WindowsRemoteControlService.exe
echo ---------------------------------------------------
echo Done
pause