TARGET = $(ASSEMBLY)
APIS = $(API)
API = gst-api.xml
RAW_API = gst-api.raw
METADATA = Gst.metadata
ASSEMBLY_NAME = gst-sharp
references = /pkg:gtk-sharp-2.0
KEYFILE = gtk-sharp.snk

ASSEMBLY = $(ASSEMBLY_NAME).dll
gapidir = $(datadir)/gapi
gapi_DATA = $(APIS)
CLEANFILES = $(ASSEMBLY) generated-stamp $(API) $(KEYFILE)
DISTCLEANFILES = $(ASSEMBLY).config AssemblyInfo.cs

all: $(TARGET)

install-data-local:
	@if test -n '$(TARGET)'; then                       \
	  echo "$(GACUTIL) /i $(ASSEMBLY) /f $(GACUTIL_FLAGS)";    \
	  $(GACUTIL) /i $(ASSEMBLY) /f $(GACUTIL_FLAGS) || exit 1; \
	fi

uninstall-local:
	@if test -n '$(TARGET)'; then                       \
	  echo "$(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS)";  \
	  $(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS) || exit 1;   \
	fi

clean-local:
	rm -rf generated

sources =				\
	Application.cs			\
	AssemblyInfo.cs			\
	CommonTags.cs

build_sources = $(addprefix $(srcdir)/, $(sources))

customs = 		\
	Bin.custom 	\
	Element.custom

build_customs = $(addprefix $(srcdir)/, $(customs))

EXTRA_DIST =			\
	$(RAW_API)		\
	$(METADATA)		\
	$(customs)		\
	$(sources) \
	$(ASSEMBLY).config.in

$(API): $(srcdir)/$(RAW_API) $(srcdir)/$(METADATA)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(GAPI_FIXUP) --api=$(API) --metadata=$(srcdir)/$(METADATA)

generated-stamp: $(API) $(GAPI_CODEGEN) $(build_customs)
	$(GAPI_CODEGEN) --generate $(API) \
	--outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME) \
	&& touch generated-stamp

$(KEYFILE): $(top_srcdir)/gtk-sharp.snk
	cp $(top_srcdir)/gtk-sharp.snk .

$(ASSEMBLY): $(build_sources) generated-stamp $(KEYFILE)
	$(CSC) /nowarn:0612 /debug /unsafe /out:$(ASSEMBLY) /target:library $(references) $(build_sources) $(GENERATED_SOURCES)

