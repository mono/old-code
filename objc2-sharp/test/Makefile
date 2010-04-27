all:
	gmcs *.cs -pkg:mono-nunit -r:objc2-sharp.dll -out:objc2-sharp-tests.dll -target:library

run:
	nunit-console2 /nologo /thread objc2-sharp-tests.dll
