@echo off
echo Installing assemblies into GAC
echo  - Mono.AppServer.security.dll
gacutil /nologo /if bin/Mono.AppServer.security.dll
echo  - Mono.AppServer.core.dll
gacutil /nologo /if bin/Mono.AppServer.core.dll
echo  - Mono.AppServer.webapplication.dll
gacutil /nologo /if bin/Mono.AppServer.webapplication.dll


