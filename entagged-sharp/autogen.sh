#!/bin/sh
# Run this to generate all the initial makefiles, etc.

srcdir=`dirname $0`
test -z "$srcdir" && srcdir=.

PKG_NAME="entagged-sharp"

(test -f $srcdir/configure.ac) || {
    echo -n "**Error**: Directory "\`$srcdir\'" does not look like the"
    echo " top-level $PKG_NAME directory"
    exit 1
}

echo "Running aclocal..."
aclocal
echo "Running autoconf..."
autoconf
echo "Running automake --add-missing ..."
automake --add-missing
echo "Running ./configure --enable-maintainer-mode $@ ..."
./configure --enable-maintainer-mode "$@"
echo "Now run 'make' to compile $PKG_NAME"

