ISSUE_DIR = issue-`date -u +%Y%m%d`
BINDIST_DIR = mwntools-bin-`date -u +%Y%m%d`
SRCDIST_DIR = mwntools-src-`date -u +%Y%m%d`
all: contributor.exe guicontributor.exe monocvsspy.exe ripper.exe

contributor.exe: mwncontributor.cs
	mcs -o contributor.exe mwncontributor.cs -r System.Web.dll
	mcs -o libcontributor.dll mwncontributor.cs /target:library -r System.Web.dll

guicontributor.exe: mwncontributor.cs guicontributor.cs ui.glade
	mcs -o guicontributor.exe guicontributor.cs /resource:ui.glade -r gtk-sharp.dll -r glade-sharp.dll -r System.Drawing -L . -r libcontributor.dll

monocvsspy.exe: monocvsspy.cs
	mcs -o monocvsspy.exe monocvsspy.cs

ripper.exe: mwnripper.cs
	mcs -o ripper.exe mwnripper.cs

issue: 
	mkdir $(ISSUE_DIR)
	cp issue.html $(ISSUE_DIR)/index.html
	cp issue.txt $(ISSUE_DIR)
	cp about.html $(ISSUE_DIR)
	cp -R arch* $(ISSUE_DIR)
	cp contributing.html $(ISSUE_DIR)
	cp footer $(ISSUE_DIR)
	cp header $(ISSUE_DIR)
	cp interview.html $(ISSUE_DIR)
	cp interheader $(ISSUE_DIR)
	cp interviewgen.sh $(ISSUE_DIR)
	cp -R mwn.rss $(ISSUE_DIR)
	cp -R mwn.css $(ISSUE_DIR)
	cp -R pixmaps $(ISSUE_DIR)
	cp -R tools $(ISSUE_DIR)
	tar -zcf $(ISSUE_DIR).tar.gz $(ISSUE_DIR)
	rm -rf $(ISSUE_DIR)

dist: all
	rm -rf $(BINDIST_DIR)
	mkdir $(BINDIST_DIR)
	cp README $(BINDIST_DIR)
	cp libcontributor.dll $(BINDIST_DIR)
	cp contributor.exe $(BINDIST_DIR)
	cp guicontributor.exe $(BINDIST_DIR)
	cp modules.list $(BINDIST_DIR)
	cp monocvsspy.exe $(BINDIST_DIR)
	cp ripper.exe $(BINDIST_DIR)
	tar -zcf $(BINDIST_DIR).tar.gz 	$(BINDIST_DIR)
	rm -rf $(BINDIST_DIR)

srcdist: clean
	rm -rf $(SRCDIST_DIR)
	mkdir $(SRCDIST_DIR)
	cp Makefile.am $(SRCDIST_DIR)
	cp Makefile $(SRCDIST_DIR)
	cp guicontributor.cs $(SRCDIST_DIR)
	cp modules.list $(SRCDIST_DIR)
	cp monocvsspy.cs $(SRCDIST_DIR)
	cp mwncontributor.cs $(SRCDIST_DIR)
	cp mwnripper.cs $(SRCDIST_DIR)
	cp ui.glade $(SRCDIST_DIR)
	tar -zcf $(SRCDIST_DIR).tar.gz $(SRCDIST_DIR)
	rm -rf $(SRCDIST_DIR)

clean:
	rm ripper.exe
	rm guicontributor.exe
	rm contributor.exe
	rm monocvsspy.exe
	rm libcontributor.dll
	rm *~
