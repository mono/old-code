GusWeb_FILES = \
	$(srcdir)/AssemblyInfo.cs \
	$(srcdir)/Global.asax.cs \
	$(srcdir)/FileHandler.cs \
	$(srcdir)/FileService.asmx.cs

GusWeb_FLAGS = /target:library /r:System.Data.dll /r:System.Web.dll /r:System.Web.Services.dll

all: $(srcdir)/bin/GusWeb.dll

$(srcdir)/bin/GusWeb.dll: $(GusWeb_FILES)
	mkdir -p $(srcdir)/bin
	$(CSC) /out:$@ $(CSCFLAGS) $(GusWeb_FLAGS) $^ 

.PHONY: xsp
xsp:
if WINDOWS
	cp $(XSP_DIR)/xsp.* $(srcdir)/bin
	./bin/xsp.exe
else
	xsp
endif
	
	
CLEAN_FILES = *.exe *.dll *.pdb *.mdb $(srcdir)/obj $(srcdir)/files $(srcdir)/bin

clean-local:
	rm -rf $(CLEAN_FILES)
    
