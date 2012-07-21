@ECHO OFF

echo Installing MyService...
echo ---------------------------------------------------
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /i %~dp0\WindowsRemoteControlService.exe
echo ---------------------------------------------------
echo Done.
pause