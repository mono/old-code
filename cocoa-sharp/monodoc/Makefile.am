prefix=/Library/Frameworks/Mono.framework/Versions/Current
SUBDIRS=..

all: monodoc

monodoc:
	rm -Rf monodoc.app
	mcs -debug -out:browser.exe browser.cs Contributions.cs XmlNodeWriter.cs catalog.cs -resource:./monodoc.png,monodoc.png -r:System.Web.Services.dll -pkg:cocoa-sharp -r:tmp/monodoc.dll
	macpack -n:monodoc -a:./browser.exe -m:2 -o:. -r:monodoc.nib -r:/Library/Frameworks/Mono.framework/Versions/Current/lib/libCocoaSharpGlue.dylib -r:tmp/monodoc.dll -r:tmp/sources -r:tmp/monodoc.xml

run:
	./monodoc.app/Contents/MacOS/monodoc

debug:
	gdb --args ./monodoc.app/Contents/MacOS/monodoc --debug Test.exe

debug-dump:
	GC_DUMP_REGULARLY=1 GC_PRINT_STATS=1 gdb --args ./monodoc.app/Contents/MacOS/monodoc --debug Test.exe

clean:
	rm -rf monodoc.app
	rm -f *.exe
