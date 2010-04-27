all: mlogo.exe 

clean:
	rm -f mlogo.exe 

MCS=mcs

MLOGO_SOURCES = \
	main.cs \
	#
	
mlogo.exe: $(MLOGO_SOURCES)
	$(MCS) /out:mlogo.exe $(MLOGO_SOURCES) /r:../Mono.Languages.Logo/Mono.Languages.Logo.dll

