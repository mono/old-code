CSC=mcs
#PROGFILES=`cygpath \`cygpath -m -s "$(PROGRAMFILES)"\``
#CSC=$(PROGFILES)/Mono-1.1.3/bin/mcs.bat /d:WIN32 
#ROOT=/cygdrive/$(subst \,/,$(subst :\,/,$(SYSTEMROOT)))
#CSC=$(ROOT)/microsoft.net/framework/v1.1.4322/csc.exe /d:WIN32 /optimize+

default:
	$(CSC) /target:library ./src/SNMP.cs ./src/AssemblyInfo.cs /out:Mono.Net.Snmp.dll
	$(CSC) /target:exe ./samples/SimpleSNMP.cs /r:Mono.Net.Snmp.dll /out:./samples/SimpleSNMP.exe
	$(CSC) /target:exe ./samples/MacAddress.cs /r:Mono.Net.Snmp.dll /out:./samples/MacAddress.exe
clean:
	rm Mono.Net.Snmp.dll ./samples/SimpleSNMP.exe ./samples/MacAddress.exe
