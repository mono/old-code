MCS=mcs

ASSEMBLIES=

SOURCES=\
	cs-tokenizer.cs		\
	cs-parser.cs		\
	genericparser.cs	\
	location.cs		\
	report.cs		\
	App.cs			\
	Indexer.cs		\
	SourceDB.cs		\

all: indexer.exe

indexer.exe: $(SOURCES)
	$(MCS) -o $@ $(ASSEMBLIES) $(SOURCES)

clean:
	rm -f *.exe

cs-parser.cs: cs-parser.jay
	./jay -ctv < skeleton.cs $^ > $@ || (rm $@; exit 1)
