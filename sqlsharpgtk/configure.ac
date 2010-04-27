AC_INIT(README)
AC_CANONICAL_SYSTEM
AM_INIT_AUTOMAKE(sqlsharpgtk, 0.4)
AM_MAINTAINER_MODE

AC_PROG_INSTALL

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
	AC_MSG_ERROR([You need to install pkg-config])
fi

dnl C# compiler
AC_PATH_PROG(CSC, csc, no)
AC_PATH_PROG(MCS, gmcs, no)
AC_PATH_PROG(MONO, mono, no)

CS="C#"
if test "x$CSC" = "xno" -a "x$MCS" = "xno"  ; then
	dnl AC_MSG_ERROR([You need to install a C# compiler])
	AC_MSG_ERROR([No $CS compiler found])
fi

if test "x$MCS" = "xno" ; then
	MCS=$CSC
fi

if test "x$MONO" = "xno"; then
	AC_MSG_ERROR([No mono runtime found])
fi

dnl gtk-sharp
GTKSHARP_REQUIRED_VERSION=2.0
PKG_CHECK_MODULES(GTKSHARP, gtk-sharp-2.0 >= $GTKSHARP_REQUIRED_VERSION)

AC_SUBST(MCS)

dnl PKG_CHECK_MODULES(SUPERSTRING_SHARP, superstring-sharp)

dnl find assembly paths for unstable installed assemblies that will be
dnl copied into monoskel installation directory for local usage with the app
dnl UNSTABLE_INSTALL_ASSEMBLIES="superstring-sharp"
UNSTABLE_INSTALL_ASSEMBLIES=
dnl INSTALLED_ASSEMBLIES="`$PKG_CONFIG --variable=Libraries $UNSTABLE_INSTALL_ASSEMBLIES`"
dnl AC_SUBST(INSTALLED_ASSEMBLIES)

AC_OUTPUT([
Makefile
browser/Makefile
browser/AssemblyInfo.cs
sqlsharpgtk/Makefile
sqlsharpgtk/AssemblyInfo.cs
])


dnl Configuration summary
echo ""
echo "Configuration summary"
echo ""
echo "   * Installation prefix = $prefix"
echo "   * $CS compiler: $MCS"
echo ""

dnl if test "$UNSTABLE_INSTALL_ASSEMBLIES" = "superstring-sharp" ; then
dnl echo "*********************************************************************************"
dnl   echo " This is the monoskel configure script; it includes a dependency on superstring."
dnl echo " Don't forget to remove this dependency in the configure.ac script if you intend"
dnl   echo " to use it in your own project."
dnl   echo "*********************************************************************************"
dnl fi

