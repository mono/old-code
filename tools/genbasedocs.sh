#!/bin/sh

make

mkdir -p ../class/html

for assembly in corlib System.Runtime.Remoting System.Security System.Web System.XML;
	do
	echo $assembly
	mkdir -p ../class/$assembly/en/html
	mono monodocs2html.exe \
		--source ../class/$assembly/en \
		--dest ../class/$assembly/en/html \
		--template monotemplate.xsl;
	tar -zcf ../class/html/mono-docs-$assembly.tgz -C ../class $assembly/en/html;
	done

