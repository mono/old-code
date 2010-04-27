AC_INIT(README)
AC_CANONICAL_SYSTEM
AM_MAINTAINER_MODE

MAJOR_VERSION=0
MINOR_VERSION=2
MICRO_VERSION=0
VERSION=$MAJOR_VERSION.$MINOR_VERSION.$MICRO_VERSION

AM_INIT_AUTOMAKE(entagged-sharp, $VERSION)

AC_SUBST(MAJOR_VERSION)
AC_SUBST(MINOR_VERSION)
AC_SUBST(MICRO_VERSION)

AC_PROG_INSTALL

AC_DEFINE_UNQUOTED(MAJOR_VERSION, $MAJOR_VERSION, [Major version number])
AC_DEFINE_UNQUOTED(MINOR_VERSION, $MINOR_VERSION, [Minor version number])
AC_DEFINE_UNQUOTED(MICRO_VERSION, $MICRO_VERSION, [Micro version number])

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
	AC_MSG_ERROR([You need to install pkg-config])
fi

dnl Mono
AC_PATH_PROG(MONO, mono)
if test "x$MONO" = "x"; then
	AC_MSG_ERROR([Can not find "mono" in your PATH])
fi

dnl mcs
AC_PATH_PROG(MCS, mcs)
if test "x$MCS" = "x" ; then
	AC_MSG_ERROR([Can not find "mcs" in your PATH])
fi

dnl Nunit
PKG_CHECK_MODULES(MONO_NUNIT, mono-nunit >= 1.0, do_tests="yes", do_tests="no")
AC_SUBST(MONO_NUNIT_LIBS)
AM_CONDITIONAL(ENABLE_TESTS, test "x$do_tests" = "xyes")
if test "x$do_tests" = "xno"; then
	AC_MSG_WARN([Could not find mono-nunit: tests will not be available.])
fi

AC_SUBST(MCS)
AC_SUBST(PKG_CONFIG)

AC_OUTPUT([
entagged-sharp.pc
Makefile
src/Makefile
tests/Makefile
])

echo "DEPRECATED, please do not use in new code; check out the 'taglib-sharp' module"

