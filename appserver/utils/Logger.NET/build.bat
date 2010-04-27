@echo off
set FrameworkPath=%SystemRoot%\Microsoft.NET\Framework

set FrameworkVersion=v1.1.4322
if exist %FrameworkPath%\%FrameworkVersion%\csc.exe goto Start
set FrameworkVersion=v1.0.3705
if exist %FrameworkPath%\%FrameworkVersion%\csc.exe goto Start

:Start
set SRC=/recurse:*.cs
set LIBS=System.Drawing.dll
set FLAGS=

set OUT=Logger.NET.dll
set TARGET=library

REM
REM Should'nt be nessesary to edit below
REM

set COMMON=%FrameworkPath%\%FrameworkVersion%\csc.exe /out:%OUT% /target:%TARGET% /lib:%LIBS% %FLAGS% %SRC%

if %1()==() goto Release
if %1==debug goto Debug

:Release
%COMMON% /optimize+ /debug- /define:RELEASE
goto :End

:Debug
%COMMON% /optimize- /debug:full /define:DEBUG

:End
