# building all
all: mPHPRuntime.dll mPHP.exe

# building mPHP Compiler
mPHP.exe: mPHPRuntime.dll *.cs CSCupRuntime/*.cs
	mcs /warn:1 /main:PHP.Core.Compiler /t:exe /out:mPHP.exe *.cs CSCupRuntime/*.cs

# building mPHP Runtime
mPHPRuntime.dll: AssemblyInfo.cs Core.cs Types.cs AST.cs SymbolTable.cs Lists.cs Report.cs
	mcs /warn:1 /t:library /out:mPHPRuntime.dll AssemblyInfo.cs Core.cs Types.cs AST.cs SymbolTable.cs Lists.cs Report.cs
