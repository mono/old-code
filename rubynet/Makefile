CSC = mcs
CSCFLAGS = $(NOWARN) -debug
RM = rm -f

# Don't warn about unused variables/fields/methods/labels, unreachable code
NOWARN = -nowarn:219,169,162,164

SRCS = Array.cs Bignum.cs Class.cs Const.cs Enum.cs Exception.cs Hash.cs IO.cs Kernel.cs Loader.cs NETRuby.cs \
	node.cs Numeric.cs Object.cs Proc.cs Regexp.cs Scanner.cs \
	String.cs Symbol.cs Thread.cs Time.cs node_print.cs codegen.cs

MAINSRCS = main.cs

JAYSRCS = parse.y

TARGETSRCS = $(SRCS) $(JAYSRCS:.y=.cs)

TAGSRCS = $(TARGETSRCS) $(JAYSRCS)

OTHER = skeleton.cs Makefile LICENSE.txt

FILES = $(SRCS) $(OTHER)

OUT = Ruby.NET.dll
MAINOUT = netruby.exe

all: $(MAINOUT)

$(OUT): $(TARGETSRCS)
	$(CSC) $(CSCFLAGS) -out:$@ -target:library $(TARGETSRCS)
	
$(MAINOUT): $(MAINSRCS) $(OUT)	
	$(CSC) $(CSCFLAGS) -out:$@ -target:exe -r:$(OUT) $(MAINSRCS)

clean:
	$(RM) $(OUT) $(MAINOUT)

%.cs: %.y
	jay -c $< < skeleton.cs > $@
# The line directives confuse mcs
	sed -i '' -e 's!#line!//line!' $@

rubynet: $(FILES)
	mkdir rubynet
	cp $(FILES) rubynet

tags: $(TAGSRCS)
	ctags $(TAGSRCS)
    
node_print.cs: tags.rb node.cs
	./tags.rb

test: tests.rb $(MAINOUT)
	mono --debug $(MAINOUT) tests.rb
