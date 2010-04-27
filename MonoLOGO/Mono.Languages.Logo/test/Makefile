all: test-cts.exe test-tokenizer.exe test-parser.exe test-interp.exe test-binder.exe

clean:
	rm -f test-cts.exe test-tokenizer.exe test-parser.exe

MCS=mcs

test-cts.exe: test-cts.cs
	$(MCS) test-cts.cs /r:../Mono.Languages.Logo.dll

test-tokenizer.exe: test-tokenizer.cs
	$(MCS) test-tokenizer.cs /r:../Mono.Languages.Logo.dll

test-parser.exe: test-parser.cs
	$(MCS) test-parser.cs /r:../Mono.Languages.Logo.dll

test-interp.exe: test-interp.cs
	$(MCS) test-interp.cs /r:../Mono.Languages.Logo.dll

test-binder.exe: test-binder.cs
	$(MCS) test-binder.cs /r:../Mono.Languages.Logo.dll
