#!/bin/sh

if [ -f Makefile ]; then
	make maintainer-clean
fi

rm -f compile INSTALL config.h.in aclocal.m4 ltmain.sh Makefile.in depcomp missing install-sh configure config.sub config.guess mkinstalldirs

