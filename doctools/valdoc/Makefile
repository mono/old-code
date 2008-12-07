EXE=valdoc.exe
SRCS=ValDoc.cs 

GENERATED=generated.cs
SCHEMA=ecma_cli_doc_format.rng

all: $(EXE)

$(EXE): $(SRCS) $(GENERATED)
	mcs /out:$(EXE) /r:Commons.Xml.Relaxng.dll $(SRCS) $(GENERATED)

$(GENERATED):
	sh generate.sh < $(SCHEMA) > $(GENERATED)

clean:
	rm -f $(EXE) *~ $(GENERATED)
