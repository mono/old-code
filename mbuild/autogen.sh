#!/bin/sh
# Run this to generate all the initial makefiles, etc.
# Ripped off from the Mono ripoff from GNOME macros version

PKG_NAME=MBuild

srcdir=`dirname $0`
test -z "$srcdir" && srcdir=.

if test "x$srcdir" = "x." ; then
  :
else
  echo "Sorry, $PKG_NAME cannot be compiled out of its source directory."
  echo "Please run autogen.sh from inside the $PKG_NAME source package."
  exit 1
fi

(autoconf --version) < /dev/null > /dev/null 2>&1 || {
  echo
  echo "**Error**: You must have \`autoconf' installed to compile $PKG_NAME."
  echo "Download the appropriate package for your distribution,"
  echo "or get the source tarball at ftp://ftp.gnu.org/pub/gnu/"
  exit 1
}

echo "Running autoconf ..."
autoconf || { echo "**Error**: autoconf failed."; exit 1; }

if test -z "$*"; then
  echo "**Warning**: I am going to run \`configure' with no arguments."
  echo "If you wish to pass any to it, please specify them on the"
  echo \`$0\'" command line."
  echo
fi

if test x$NOCONFIGURE = x; then
  echo Running $srcdir/configure $conf_flags "$@" ...
  $srcdir/configure $conf_flags "$@" \
  && echo Now type \`make\' to compile $PKG_NAME || exit 1
else
  echo Skipping configure process.
fi
