
all: 
	mono /cvs/lb/lb.exe
	chmod 644 archive/*/*.html
	chmod 644 *html *rss2 
	rsync -pr -v --rsh=ssh texts archive \
	log-style.css *.rss2 index.html all.html		\
	mono@segfault.cam.novell.com:public_html/news
	curl -o /dev/null http://www.mono-project.com/Main_Page?action=purge
