BAKE = bake.exe
SOURCES = $(wildcard src/Bake/*.boo)

$(BAKE): $(SOURCES)
	booc -nologo -o:$@ $(SOURCES)

clean:
	rm -f *.exe*
