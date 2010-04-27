all: ripper.exe archiver.exe

ripper.exe: Ripper.cs
	mcs -o ripper.exe Ripper.cs

archiver.exe: Archiver.cs
	mcs -o archiver.exe Archiver.cs

preview: 
	mono ripper.exe 
	mkdir preview
	cp -R pixmaps preview
	cp issue.html preview
	cp archives.html preview
	cp mwn.rss preview
	cp mwn.css preview
	cp archives.rss preview
	cp -R archives preview
	tar -zcf preview.tgz preview

clean:
	rm -rf *.exe
	rm -rf *~
	rm preview.tgz
	rm -rf preview


