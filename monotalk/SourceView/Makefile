MCS=mcs

ASSEMBLIES=-r gconf-sharp -r gnome-sharp -r gtk-sharp -r glade-sharp -r /home/petr/local/lib/glib-sharp.dll
RESOURCES=-resource:demoapp.glade,demoapp.glade
RESOURCE_FILES=demoapp.glade
SOURCES=\
        Highlights.cs \
        Config.cs \
        Pattern.cs \
        Style.cs \
	SourceBuffer.cs \
	SourceView.cs \
        DemoApp.cs


all: sourceview.schema test.exe

clean:
	rm -f *.exe

test.exe: $(SOURCES) $(RESOURCE_FILES)
	$(MCS) -o $@ $(ASSEMBLIES) $(SOURCES) $(RESOURCES)

install:	sourceview.config
	./svconftool sourceview.config --schema sourceview.schema --install --apply
