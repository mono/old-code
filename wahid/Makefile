SOURCES = Cell.cs Formula.cs Locale.cs Sheet.cs Workbook.cs ZipAccess.cs Value.cs xlsx.cs 

DESKTOP_SOURCES = DirAccess.cs 

wahid.exe: main.cs $(SOURCES) $(DESKTOP_SOURCES) Makefile
	gmcs -debug -out:wahid.exe main.cs $(SOURCES) $(DESKTOP_SOURCES) -r:System.Xml.Linq -r:System.Core

run: wahid.exe
	mono --debug wahid.exe