#
#  autogen.sh
#
#  Authors
#    - C.J. Collier, Collier Technologies, <cjcollier@colliertech.org>
#    - Urs C. Muff, Quark Inc., <umuff@quark.com>
#    - Kangaroo, Geoff Norton
#    - Adham Findlay
#
#  Copyright (c) 2004 Quark Inc. and Collier Technologies.  All rights reserved.

#	$Header: /home/miguel/third-conversion/public/cocoa-sharp/autogen.sh,v 1.9 2004/08/10 19:04:21 gnorton Exp $
#

#Simple test for OS X
PATH=/usr/bin:$PATH

if [ -d /Library/Frameworks/Mono.framework/Versions/Current/share/aclocal ]; then
    aclocal -I /Library/Frameworks/Mono.framework/Versions/Current/share/aclocal
else
    echo "WARNING: Please use the official Mono OS X Packages"
    aclocal
fi

#glibtoolize --force --copy
automake -a
autoconf
./configure --enable-maintainer-mode $*
#./generator/genstubs.pl

#***************************************************************************
#
# $Log: autogen.sh,v $
# Revision 1.9  2004/08/10 19:04:21  gnorton
# auto* build system redone
#
# Revision 1.8  2004/08/03 21:29:01  adhamh
# Updated packaging script for CocoaSharp.  Along with postflight scrips so that links are created.
#
# Added some documenation to README and autogen.sh
# Added the mono.icns file to our example nib so that it could be built
#         without the rest of Cocoa-Sharp source.
#
# Revision 1.7  2004/06/21 04:57:30  gnorton
# Update to use our new .Net based generator (much butter; supports protocols/categories/interfaces)
# Outputs working glue that doesn't break the tests.
#
# Revision 1.6  2004/06/16 12:20:26  urs
# Add CVS headers comments, authors and Copyright info, feel free to add your name or change what is appropriate
#
#***************************************************************************
