
# this is used in the MCS build system, but it doesn't make sense to me.
# Where's OS defined?
ifndef PLATFORM
ifeq ($(OS), Windows_NT)
PLATFORM = win32
else
PLATFORM = linux
endif
endif

ifeq ($(PLATFORM), win32)
CSC = csc
DS=\\
else
CSC = mcs
DS=/
endif

PACKAGE = type-reflector
VERSION = 0.8
topdir = .
mkinstalldirs = mkdir -p

INSTALL = install

PROGRAM = type-reflector.exe

PROGRAM_LIBS = -r:System.dll

PROGRAM_FLAGS = -out:$(PROGRAM) -d:TRACE -d:DEBUG $(PROGRAM_LIBS)

# For debugging
CSCFLAGS = /debug+ /debug:full
# CSCFLAGS = 

MONO = mono

DISPLAYERS_DIR=displayers$(DS)

DISPLAYERS_FILES = \
	$(DISPLAYERS_DIR)ConsoleTypeDisplayer.cs \
	$(DISPLAYERS_DIR)IndentingTextWriter.cs \
	$(DISPLAYERS_DIR)ITypeDisplayer.cs \
	$(DISPLAYERS_DIR)TypeDisplayer.cs \

FINDERS_DIR=finders$(DS)

FINDERS_FILES = \
	$(FINDERS_DIR)ExplicitNodeFinder.cs \
  $(FINDERS_DIR)GroupingNodeFinder.cs \
	$(FINDERS_DIR)INodeFinder.cs \
	$(FINDERS_DIR)NodeFinder.cs \
  $(FINDERS_DIR)ReflectionNodeFinder.cs \

FORMATTERS_DIR=formatters$(DS)

FORMATTERS_FILES = \
	$(FORMATTERS_DIR)CSharpNodeFormatter.cs \
	$(FORMATTERS_DIR)DefaultNodeFormatter.cs \
	$(FORMATTERS_DIR)INodeFormatter.cs \
	$(FORMATTERS_DIR)LanguageNodeFormatter.cs \
	$(FORMATTERS_DIR)NodeFormatter.cs \
	$(FORMATTERS_DIR)VBNodeFormatter.cs \

PROGRAM_FILES =  \
	AssemblyInfo.cs \
	Factories.cs \
	IPolicy.cs \
	Node.cs \
  NodeGrouper.cs \
	NodeInfo.cs \
	Policy.cs \
	NodeTypes.cs \
	ProgramOptions.cs \
	TestTypes.cs \
	TextFormatter.cs \
	TypeFactory.cs \
	TypeLoader.cs \
	TypeReflectorApp.cs \
	TypeReflectorOptions.cs \
	$(DISPLAYERS_FILES) \
	$(FINDERS_FILES) \
	$(FORMATTERS_FILES) \

CSC_INVOKE = $(CSC) $(CSCFLAGS) $(PROGRAM_FLAGS) $(PROGRAM_FILES) $(PROGRAM_LIBS)

GUI_LIBS = -r:System.Drawing.dll

GUI_ART_DIR = $(DISPLAYERS_DIR)art$(DS)

GUI_RESOURCES = \
	-resource:$(GUI_ART_DIR)abstract.png,abstract.png \
	-resource:$(GUI_ART_DIR)class.png,class.png \
	-resource:$(GUI_ART_DIR)constructor.png,constructor.png \
	-resource:$(GUI_ART_DIR)enum.png,enum.png \
	-resource:$(GUI_ART_DIR)event.png,event.png \
	-resource:$(GUI_ART_DIR)field.png,field.png \
	-resource:$(GUI_ART_DIR)interface.png,interface.png \
	-resource:$(GUI_ART_DIR)method.png,method.png \
	-resource:$(GUI_ART_DIR)prop-read-only.png,prop-read-only.png \
	-resource:$(GUI_ART_DIR)prop-read-write.png,prop-read-write.png \
	-resource:$(GUI_ART_DIR)prop-write-only.png,prop-write-only.png \
	-resource:$(GUI_ART_DIR)sealed.png,sealed.png \
	-resource:$(GUI_ART_DIR)transparent.png,transparent.png 

GUI_GTK_DIR = $(DISPLAYERS_DIR)gtk$(DS)

GUI_GTK_FILES = \
	$(GUI_GTK_DIR)GtkTypeDisplayer.cs \
	$(GUI_GTK_DIR)AppWindowManager.cs \

GUI_GTK_GLADE_FILE = $(GUI_GTK_DIR)type-reflector.glade

GUI_GTK_FLAGS = -resource:$(GUI_GTK_GLADE_FILE),type-reflector.glade

GUI_GTK_LIBS = -pkg:gtk-sharp -pkg:glade-sharp -pkg:gnome-sharp

GUI_SWF_DIR = $(DISPLAYERS_DIR)swf$(DS)

GUI_SWF_FILES = \
	$(GUI_SWF_DIR)SwfWindow.cs \
	$(GUI_SWF_DIR)SwfTypeDisplayer.cs \

GUI_SWF_LIBS = -r:System.Windows.Forms.dll

GUI_SWF_FLAGS = 

# These are files that are `touch'ed during the build process to prevent
# constant rebuilding.
TARGETS = gui-all gtkui swfui

all: gui test-assemblies

test-assemblies : TestTypes.dll

TestTypes.dll: TestTypes.cs
	$(CSC) -out:$@ -t:library $<

console : $(PROGRAM)

$(PROGRAM) : $(PROGRAM_FILES)
	$(CSC_INVOKE)
	chmod +x $(PROGRAM)

# Pick the appropriate GUI Toolkit...
# Fall back on console-only if nothing else compiles.
gui: $(PROGRAM_FILES) $(GUI_GTK_FILES) $(GUI_GTK_GLADE_FILE) $(PROGRAM_FILES) $(GUI_SWF_FILES)
	$(MAKE) gui-all || $(MAKE) gtkui || $(MAKE) swfui || $(MAKE) console

# Compile in support for all GUI front-ends (Gtk#, Swf, and anything else...)
gui-all: $(PROGRAM_FILES) $(GUI_GTK_FILES) $(GUI_GTK_GLADE_FILE) $(PROGRAM_FILES) $(GUI_SWF_FILES)
	$(CSC_INVOKE) $(GUI_GTK_FLAGS) $(GUI_SWF_FLAGS) $(GUI_GTK_FILES) $(GUI_SWF_FILES) $(GUI_GTK_LIBS) $(GUI_SWF_LIBS) $(GUI_RESOURCES) $(GUI_LIBS)
	chmod +x $(PROGRAM)
	touch $@

# Gtk# front-end
gtkui: $(PROGRAM_FILES) $(GUI_GTK_FILES) $(GUI_GTK_GLADE_FILE)
	$(CSC_INVOKE) $(GUI_GTK_FLAGS) $(GUI_GTK_FILES) $(GUI_GTK_LIBS) $(GUI_RESOURCES) $(GUI_LIBS)
	chmod +x $(PROGRAM)
	touch $@

# System.Windows.Forms front-end
swfui: $(PROGRAM_FILES) $(GUI_SWF_FILES)
	$(CSC_INVOKE) $(GUI_SWF_FLAGS) $(GUI_SWF_FILES) $(GUI_SWF_LIBS) $(GUI_RESOURCES) $(GUI_LIBS)
	chmod +x $(PROGRAM)
	touch $@

test-run : $(PROGRAM)
	$(MONO) $(PROGRAM) -A $(PROGRAM) $(ARGS)

test-type : $(PROGRAM)
	$(MONO) $(PROGRAM) -A $(PROGRAM) --max-depth=10000 $(ARGS) TestClass$$

run : $(PROGRAM)
	$(MONO) $(PROGRAM) $(ARGS)

windows:
	NAnt -buildfile:type-reflector.build

clean:
	rm -f *.exe *.pdb *.dll *.dbg *~ $(TARGETS) $(DISPLAYERS_DIR)*~ $(GUI_GTK_DIR)*~ $(GUI_SWF_DIR)*~ $(FINDERS_DIR)*~ $(FORMATTERS_DIR)*~

# We don't generate any files for building, so don't do anything
distclean: clean
	

TR_FILES = \
	$(PROGRAM) \
	$(PROGRAM).config \


install: $(BIN_FILES) $(TR_FILES)
	if test "x$$prefix" = x; then \
		echo Usage is: make -f makefile.gnu install prefix=YOURPREFIX; \
		exit 1; \
	fi
	mkdir -p "$(prefix)/bin/"
	mkdir -p "$(prefix)/share/$(PACKAGE)/"
	cat type-reflector.in | sed 's:@PREFIX@:$(prefix):g' > "$(prefix)/bin/$(PACKAGE)"
	chmod +x "$(prefix)/bin/$(PACKAGE)"
	for i in $(TR_FILES) ; do \
		($(INSTALL) -m 755 $$i $(prefix)/share/$(PACKAGE)/) || exit 1; \
	done

uninstall:
	if test x$$prefix = x; then \
		echo Usage is: make -f makefile.gnu uninstall prefix=YOURPREFIX; \
		exit 1; \
	fi
	rm "$(prefix)/bin/$(PACKAGE)"
	for i in $(TR_FILES) ; do \
		(rm -f $(prefix)/share/$(PACKAGE)/$$i) || exit 1; \
	done
	-rmdir $(prefix)/share/$(PACKAGE)

linecount:
	wc -l $(PROGRAM_FILES) $(GUI_GTK_FILES)


# For packaging into a tarball
distdir = $(topdir)/$(PACKAGE)-$(VERSION)

# Simple, probably inaccurate (it'll likely copy too many files), but effective
distdir:
	-mkdir $(distdir)
	for f in `find . | grep -v CVS | grep -v ~$$ | grep -v $(distdir)` ; do \
		if test -d $$f; then \
			$(mkinstalldirs) "$(distdir)/$$f" ; \
		else \
			cp -p "$$f" "$(distdir)/$$f" ; \
		fi; \
	done

dist dist-all: distdir
	tar chof - $(distdir) | GZIP=$(GZIP_ENV) gzip -c >$(distdir).tar.gz
	rm -rf $(distdir)

# vim: noexpandtab

