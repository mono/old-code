dnl -*- mode: m4 -*-
AC_PREREQ(2.52)

dnl Keep these in sync with the Buildfile

AC_INIT(MBuild, 0.0.4.0, peter@newton.cx, mbuild)

COMPAT=0.0
AC_SUBST(COMPAT)

dnl blah blah blah

if test "x$srcdir" = "x." ; then
	:
else
	AC_MSG_ERROR([Sorry, MBuild cannot be built outside of its source directory.])
fi

AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
	AC_MSG_ERROR([MBuild requires pkg-config to be compiled])
fi

AC_ARG_WITH([mono],
[  --with-mono=/path/to/mono    Tell me the prefix that Mono is installed in (normally not needed)],
[], [with_mono=no])

AC_MSG_CHECKING([for Mono])

if test "x$with_mono" = "xno" ; then
	if $PKG_CONFIG --exists mono ; then
		AC_MSG_RESULT(ok)
	else
		AC_MSG_RESULT(pkg-config failed)
		AC_MSG_ERROR([The program \'$PKG_CONFIG\' failed to run or
find Mono. Either pkg-config is not correctly installed, Mono is not correctly
installed, or you need to use the PKG_CONFIG_PATH environment variable.])
	fi

	monoprefix=`$PKG_CONFIG --variable=prefix mono`
else
	AC_MSG_RESULT([specified: $with_mono])
	monoprefix="$with_mono"
fi

AC_PATH_PROG(MONO, mono, no, [$monoprefix/bin:$PATH])
if test "x$MONO" = "xno"; then
	AC_MSG_ERROR([Could not find 'mono'. To compile MBuild, you must have the Mono
runtime installed])
fi

AC_PATH_PROG(MCS, gmcs, no, [$monoprefix/bin:$PATH])
if test "x$MCS" = "xno"; then
	AC_MSG_ERROR([Could not find 'gmcs'. To compile MBuild, you must have the
Mono C-Sharp 2.0 compiler installed.])
fi

AC_PATH_PROG(GACUTIL, gacutil, no, [$monoprefix/bin:$PATH])
if test "x$GACUTIL" = "xno"; then
	AC_MSG_ERROR([Could not find 'gacutil'. To compile MBuild, you must have the
Mono runtime installed.])
fi

AC_PATH_PROG(JAY, jay, no, [$monoprefix/bin:$PATH])
if test "x$JAY" = "xno"; then
	AC_MSG_ERROR([Could not find 'jay'. To compile MBuild, you must have the
Mono development toolkit installed.])
fi

AC_PATH_PROG(SN, sn, no, [$monoprefix/bin:$PATH])
if test "x$SN" = "xno"; then
	AC_MSG_ERROR([Could not find 'sn'. To compile MBuild, you must have the
Mono development toolkit installed.])
fi

AC_ARG_WITH([jay-skeleton],
[  --with-jay-skeleton=filename Tell me where to find the Jay skeleton file (normally not needed)],
[], [with_jay_skeleton=no])

AC_MSG_CHECKING([for Jay skeleton file])

if test x$with_jay_skeleton = xno ; then
	if test -f "$monoprefix/share/jay/skeleton.cs" ; then
		AC_MSG_RESULT(ok)
		with_jay_skeleton="$monoprefix/share/jay/skeleton.cs"
	else
		AC_MSG_RESULT([did not find $monoprefix/share/jay/skeleton.cs])
		AC_MSG_ERROR([I could not find the Jay skeleton template, a development
file that should be installed along with Mono. Do you have the Mono developer files
installed?])
	fi
else
	AC_MSG_RESULT([specified: $with_jay_skeleton])
fi

JAY_SKELETON_FILE="$with_jay_skeleton"
AC_SUBST(JAY_SKELETON_FILE)

AC_OUTPUT([Makefile])
