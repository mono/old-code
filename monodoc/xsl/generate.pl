#!/usr/bin/perl -w

# generate.pl -- generate static monodoc docs
# Scott Bronson
# 15 Oct 2002

# OK, this file was supposed to be a tiny utility.  It's grown badly,
# far beyond what I envisioned.  What can I say?  It seems to work...

# NOTE: generate doesn't do dependencies.  If you change doc.xsl, you
#       need to touch the xsl files that include it to have anything
#       actually be rebuilt (i.e. namespace.xsl, summary.xsl, etc).


use strict;

# download this from CPAN
use HTML::Parser;
use HTML::LinkExtor;

# these should all be installed by default
use File::Path;
use File::Basename;


# ------- you might want to change these -------

# This is where we can find the assembly directories
my $doc_dir = "../class";

# "System.Configuration.Install" / "System.XML" etc.
my $default_assembly = "corlib";


# ------ probably don't change ---------

# By default, put everything in "output" directory in current dir.
my $outdir = $ENV{'MONODOC_OUTPUT_DIR'} || "./output";

# Path to xsl files.
my $xsldir = $ENV{'MONODOC_XSL_ROOT'} || ".";


# ------ no configuration below this line ---------

# OK let's go


# COMMAND-LINE ARGUMENTS:

my $debug = 0;		# -d: turn on debug messages
my $blather = 0;  	# -n: noisy: allow xalan to blather uselessly
	# problem is, you can't turn off blather without also
	# turning off vital error messages.
my $quiet = 0;		# -q: only output what is requested by the user
my $missing = 0;	# -m: dump a list of missing files to stdout
my $show_cmd = 0;	# -c: show XSL command line for each file
my $strikeout = 0;	# -s: update docs with bad links struck out
# Any other cmd-line args will be interpreted as the name of the
# assembly to generate.  We generate mscorlib by default.


# these specify the assembly to process
my $xmldir = "$doc_dir/$default_assembly";
$default_assembly = "System.Xml" if $default_assembly eq "System.XML";
$default_assembly = "mscorlib" if $default_assembly eq "corlib";
my $root = $default_assembly.'.xml,namespace.xsl[l=en,ns=].html';


my %todo = ();
my %done = ();
my %dir_exists = ();
my %missing = ();


# Process command-line arguments
for(@ARGV) {
	$debug =   1, next if /^-d/;
	$blather = 1, next if /^-n/;
	$quiet =   1, next if /^-q/;
	$missing = 1, next if /^-m/;
	$show_cmd = 1, next if /^-c/;
	$strikeout = 1, next if /^-s/;

	# if it's not a cmd-line arg, it's an assembly.
	my $assembly = $_;

	$xmldir = "$doc_dir/$assembly";
	-d $xmldir or die "Could not find $xmldir\n";

	# crazy ximian folder names...
	$assembly = "System.Xml" if $assembly eq "System.XML";
	$assembly = "mscorlib" if $assembly eq "corlib";

	$root = $assembly.'.xml,namespace.xsl[l=en,ns=].html';

	!$quiet && print "Processing $_ assembly\n";
	last;  # we can't process multiple assemblies yet.
}

-d $xmldir or die "Could not find $xmldir\n";


# Include a reasonable index file
write_output_file("index.html", <<EOL
<HTML>
<HEAD> <TITLE>Mono Documentation Redirect</TITLE>
<META HTTP-EQUIV="refresh" 
	CONTENT="1;URL=$root">
</HEAD> <BODY>
<H2>Documentation Redirect</H2>
You will now be redirected to the
<A HREF="$root">root page</A>.
You should be taken there automatically.
</BODY>
</HTML>
EOL
);


# Right now we need to generate the root with sablotron and the rest
# of the XSL with xalan (there's some sort of bug in Xalan 1.2 that
# prevents it from generating the root, and Sablotron doesn't implement
# the ID function, a "minor" discrepancy from the standard???)
process(\&sablotron_cmdgen, $root);


while(%todo) {
	my $item = (keys %todo)[0];
	delete($todo{$item});
	process(\&sablotron_cmdgen, $item);
}


!$quiet && print scalar(keys(%missing)) . " missing documentation files.  " .
	"Use -d to list them.\n";
if($missing && %missing) {
	!$quiet && print "\nMissing:\n"; 
	print join("\n", sort(keys(%missing))), "\n";
}


if($strikeout) {
	for(keys(%done)) {
		next if $missing{$_};
		strikeout($_);
	}
}

exit(0);


sub process
{
	my $cmdgen = shift;
	my $file = shift;

	!$quiet && printf "%3d down %3d to go: ",
		scalar(keys %done), 1 + scalar(keys %todo);

	my %inf = eval { parse_link($file); };
	if( $@ ) {
		print "\nwhile processing $file\n";
		die($@);
	}
	my $cmd = &$cmdgen(%inf);

	$done{$file} = 1;

	if(-f $inf{'xmlpath'}) {
		if(-f "$outdir/$file") {
			if(is_older("$outdir/$file", 
				$inf{'xmlpath'}, $inf{'xslpath'}))
			{
				# out is older than xsl or xml
				!$quiet && print "rebuild $file\n";
			} else {
				!$quiet && print "exists $file\n";
			link_extractor(\%inf)->parse_file("$outdir/$file");
				return;
			}
		} else {
			!$quiet && print "translating $file\n";
			$debug > 0 && print "command: $cmd\n";
		}
	} else {
		if( $inf{'xsl'} eq "summary.xsl" )
		{
			!$quiet && print "missing xml $file\n";
			$missing{$file}++;
			!$quiet && $debug > 0 && print 
				"skip: $inf{'xmlpath'} does not exist\n";
			return;
		} else {
			print "\nbad link: $file\n";
			print "command: $cmd\n";
			die "File missing: $inf{'xmlpath'}\n";
		}
	}

	$show_cmd && print "command: $cmd\n";
	# fork process and slurp output
	my($binary) = $cmd =~ /^(\S+)/;
	open(FH, "$cmd|") or die "command: $cmd\nCould not fork $binary: $!\n";
	local $/; # slurp entire file
	my $data = <FH>;
	close FH;

	# Ensure child ran without errors
	my $signal_no = $? & 0xFF;
	my $exit_val = ($? >> 8) & 0xFF;
	if($signal_no || $exit_val) {
		$data =~ /((?:.*\n){0,6})$/;
		$data = "out: ...\n" . ($1?"out: ":"") .
			join("\nout: ", split(/\n/, $1));
		die "command: $cmd\n$data\n" .
			"XSLT Processor bailed on signal $signal_no " .
			"(exit val=$exit_val)\n" if $signal_no;
		die "command: $cmd\n$data\n" .
			"XSLT Processor exited with value $exit_val\n"
			if $exit_val;
	}

	eval {
		link_extractor(\%inf)->parse($data)->eof;
		write_output_file($file, $data);
	};
	if( $@ ) {
		print "while translating $file\n";
		print "command: $cmd\n";
		die($@);
	}
}


sub print_hash
{
    my( $hash ) = @_;
    my $key;

    foreach $key ( sort keys %{$hash} ) {
       print "\"$key\": ", ${%{$hash}}{$key}, "\n";
    }
}


# Returns true if the first file passed is older than
# any of the rest of the files passed as arguments.

sub is_older
{
	my $file = shift;
	my @s = stat($file);

	for(@_) {
		my @n = stat($_);
		return 1 if($s[9] < $n[9]);
	}

	return 0;
}


sub link_extractor
{
	# returns an HTML::Parser set up to extract links and queue
	# them for processing

	my $inf = shift;

	return HTML::LinkExtor->new( sub {
		my($tag, %links) = @_;
		return unless exists $links{'href'};
		my $link = $links{'href'};
		$link =~ s#^.*/\./##;  # delete everything before /./
		$link =~ s/\#.*$//;    # delete everything after #
		if(check_link($inf, $link)) {
			$todo{$link}++ unless exists($done{$link});
		}
	} );
}


sub write_output_file
{
	my $fname = shift;
	my $data = shift;

	# Make sure the directory exists
	my $dirname = dirname($fname);
	$dirname = "" if $dirname eq '.';

	unless($dir_exists{$dirname}) {
		-d "$outdir/$dirname" or mkpath("$outdir/$dirname")
			or die "Could not mkdir $outdir/$dirname: $!\n";
		$dir_exists{$dirname}++;
	}

	open(OUT,">$outdir/$fname") or die "Error opening $outdir/$fname: $!\n";
	print OUT $data;
	close OUT;
}


sub parse_link
{
	my $link = shift;

	my $lastkey = "";

	# parse information out of filename, return hash with this info:
	# xml: name of xml file      xmlpath: path to xml file
	# xsl: name of xsl file      xslpath: path to xsl file
	# params: hash of parameter name/value pairs.
	# link: the link that was parsed to generate this info

	# files...
	$link =~ /^ (?:(.*) \/)? ([A-Za-z0-9_\.]+.xml) , ([A-Za-z0-9_]+.xsl) \[?/xg
		or die "Malformed .xml or .xsl: $link\n";
	my %inf = ('path'=>$1, 'xml'=>$2,
		'xmlpath'=>"$xmldir" . ($1?"/$1":"") ."/$2",
		'xsl'=>$3, 'xslpath'=>"$xsldir/$3",
		'params' => {}, 'link' => $link );

	# params...
	while($link =~ m/\G([A-Za-z0-9_]+)=([^,=\]]*),?\s*/g) {
		die "Param names must go in alphabetical order: $link\n"
			if ($1 cmp $lastkey) != 1;
		$lastkey = $1;
		$inf{'params'}->{$1} = $2;
	}

	# finish up
	$link =~ m/\G.*html/g or die "Could not parse end of filename: $link!\n";
	
	return %inf;
}


# This is basically a lint for links.  It knows about the format
# of the links that the various xsl files require, and ensures 
# that format is strictly followed.

sub check_link
{
	my $cur_inf = shift;
	my $link = shift;

	return 0 if $link =~ /BAD.LINK/;

	die "Invalid path, '..': $link\n" if $link =~ /\.\./;
	die "Invalid path, '/./': $link\n" if $link =~ /\/\.\//;

	my %inf = parse_link($link);
	my @i = ($cur_inf, \%inf);

	diemsg(@i, "fairly bad link!\n") if $link =~ /\/\//;

	# This is just another way to write a switch statement.
	# AFAIK I invented this.  Time to patent it (under a pseudonym :)
	for( $inf{'xsl'} ) {(
	/^namespace.xsl$/ && sub {
		check_params(@i, { 'l' => 'en', 'ns' => undef });
	} || /^summary.xsl$/ && sub {
		check_params(@i, { 'l' => 'en' });
	} || /^members.xsl$/ && sub {
		check_params(@i, { 'l' => 'en', 'view' => undef });
		diemsg(@i, "Invalid view: $inf{'params'}->{'view'}!\n")
			if $inf{'params'}->{'view'} ne 'n'
			&& $inf{'params'}->{'view'} ne 'a'
			&& $inf{'params'}->{'view'} ne 't';
	} || /^item.xsl$/ && sub {
		check_params(@i, { 'l' => 'en', 'm' => undef });
		diemsg(@i, "bad member name: $inf{'params'}->{'m'}\n")
			if $inf{'params'}->{'m'} =~ /[^A-Za-z0-9_]/
			&& $inf{'params'}->{'m'} ne '.ctor';
	} || sub {
		die "Unknown xsl file: " . $inf{'xsl'} . "\n";
	})->() };

	return 1;
}


# checks the parameters in inf against the parameters listed
# in $ck.  All must exist, and all values must match (specify
# undef for don't cares).

sub check_params
{
	my $cur_inf = shift;
	my $inf = shift;
	my $ck = shift;

	my @i = ($cur_inf, $inf);

	# check for the correct number of keys
	keys(%{$inf->{'params'}}) == keys(%$ck) or
		diemsg(@i, "There should be " .
		scalar(keys(%$ck)) . " keys in params, not " .
		scalar(keys %{$inf->{'params'}}) . "!\n");

	for(keys(%$ck)) {
		exists($inf->{'params'}->{$_}) or
			diemsg(@i, "You need to include a $_ parameter!\n");
		diemsg(@i, "The $_ parameter must equal '$ck->{$_}'!\n")
			if defined($ck->{$_}) and
			($inf->{'params'}->{$_} ne $ck->{$_});
	}
}


sub diemsg
{
	my $cur_inf = shift;
	my $inf = shift;
	my $msg = shift;

	print "processing: " . $cur_inf->{'link'} . "\n";
	print "bad link: " . $inf->{'link'} . "\n";
	die "ERROR!  $msg";

}


sub sablotron_cmdgen
{
	my %p = @_;

	my $cmd = "sabcmd $p{xslpath} $p{xmlpath}";
	for(keys %{$p{'params'}}) {
		$cmd .= ' \'$' . $_ . '=' . $p{'params'}->{$_} . '\'';
	}

	return $cmd;
}


sub xalan_cmdgen
{
	my %p = @_;

	my $cmd = "xalan " . ($blather ? "" : "-Q ") . "-XSL $p{xslpath} -IN $p{xmlpath}";
	for(keys %{$p{'params'}}) {
		$cmd .= " -PARAM $_ \"'" . $p{'params'}->{$_} . "'\"";
	}

	return $cmd;
}


# Strikes out all links that are found in the missing hash.
# Modifies HTML file in-place (hopefully that's easier on the disk cache).
# I tell ya, this new HTML::Parser design is one weird gob of state.

sub strikeout
{
	my $file = shift;
	my $data = "";		# the output
	my @tagstack = ();	# keeps track of a tag depth (they nest)
	my $dirty = 0;		# the number of updates to this file
	my $suppress = 0;	# have we already hit this file?

	$debug > 1 && print "Updating $file...\n";

	my $p = HTML::Parser->new(
		start_h => [sub {
			my($tag, $attr, $text) = @_;
			my $k = 0;
			$suppress = 1 if $tag eq "s";
			if($tag eq "a") {
				if(exists $attr->{'href'}) {
					my $link = $attr->{'href'};
					$link =~ s#^.*/\./##;
					if($missing{$link}) {
						$dirty += 1;
						if(!$suppress) {
							$k = 1;
							$data .= "<s>";
						}
					}
				}
				push @tagstack, $k;
			}
			$data .= $text;
		}, 'tag,attr,text'],
		end_h => [ sub {
			my($tag, $text) = @_;
			$data .= $text;
			$suppress = 0 if $tag eq "s";
			if($tag eq "/a") {
				$data .= "</s>" if pop @tagstack;
			}
		}, 'tag,text' ],
		default_h => [ sub { $data .= shift; }, 'text' ],
		) or die "Making the parser didn't work! $!\n";
	

	open(FILE, "+<$outdir/$file") or die "Can't read $outdir/$file: $!\n";

	$p->parse_file(*FILE);

	if($dirty) {
		print "Updated $dirty time" . ($dirty == 1 ? "" : "s")
			. ": $file\n";
		seek(FILE, 0, 0);
		print FILE $data;
		truncate(FILE, tell(FILE));
	}
	close FILE;
}

