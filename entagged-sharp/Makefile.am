SUBDIRS = src tests
DISTCLEANFILES=*.pidb *.bak

# NOT ALLOWING INSTALLATION RIGHT NOW
# http://www.mono-project.com/Guidelines:Application_Deployment
# 
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = entagged-sharp.pc

install-data-hook:
	cat ./INSTALL

EXTRA_DIST = entagged-sharp.pc.in 

