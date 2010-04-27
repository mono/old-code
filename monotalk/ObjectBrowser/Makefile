MCS=mcs -g
ART=../art/
INDEXER=../Indexer/
SOURCEVIEW=../SourceView/

ASSEMBLIES=-r glib-sharp.dll -r pango-sharp.dll -r atk-sharp.dll -r gdk-sharp.dll -r gtk-sharp.dll -r art-sharp.dll -r gnome-sharp.dll -r System.Drawing -r glade-sharp -r gconf-sharp -r Mono.CSharp.Debugger
RESOURCES=\
	-resource:$(ART)transparent.png,transparent.png			\
	-resource:$(ART)prop-read-only.png,prop-read-only.png		\
	-resource:$(ART)prop-write-only.png,prop-write-only.png 	\
	-resource:$(ART)prop-read-write.png,prop-read-write.png 	\
	-resource:$(ART)constructor.png,constructor.png			\
	-resource:$(ART)method.png,method.png				\
	-resource:$(ART)field.png,field.png				\
	-resource:$(ART)event.png,event.png				\
	-resource:$(ART)class.png,class.png				\
	-resource:$(ART)abstract.png,abstract.png			\
	-resource:$(ART)sealed.png,sealed.png				\
	-resource:$(ART)enum.png,enum.png				\
	-resource:$(ART)interface.png,interface.png			\
	-resource:demoapp.glade,demoapp.glade

RESOURCE_FILES=\
	demoapp.glade			\
	$(ART)transparent.png		\
	$(ART)prop-read-write.png	\
	$(ART)prop-write-only.png	\
	$(ART)prop-read-only.png	\
	$(ART)constructor.png		\
	$(ART)method.png		\
	$(ART)field.png			\
	$(ART)event.png			\
	$(ART)class.png			\
	$(ART)abstract.png		\
	$(ART)sealed.png		\
	$(ART)enum.png			\
	$(ART)interface.png		\

SOURCES=\
	DemoApp.cs		\
	FindBar.cs		\
	Hierarchy.cs		\
	MemberView.cs		\
	MemberRecord.cs		\
	MemberRecordFactory.cs	\
	ObjectBrowser.cs	\
	TypePool.cs		\
	$(INDEXER)/Indexer.cs	\
	$(INDEXER)/SourceDB.cs	\
	$(INDEXER)/cs-parser.cs	\
	$(INDEXER)/cs-tokenizer.cs	\
	$(INDEXER)/location.cs	\
	$(INDEXER)/report.cs	\
	$(INDEXER)/genericparser.cs	\
	$(SOURCEVIEW)/Config.cs	\
	$(SOURCEVIEW)/Pattern.cs	\
	$(SOURCEVIEW)/Style.cs	\
	$(SOURCEVIEW)/Highlights.cs	\
	$(SOURCEVIEW)/SourceView.cs	\

all: objectbrowser.exe

objectbrowser.exe: $(SOURCES) $(RESOURCE_FILES)
	$(MCS) -o $@ $(ASSEMBLIES) $(SOURCES) $(RESOURCES)

install:
	GCONF_CONFIG_SOURCE="" gconftool-2 --makefile-install-rule objectbrowser.schema

clean:
	rm -f *.exe
