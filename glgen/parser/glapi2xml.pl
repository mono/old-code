#!/usr/bin/perl
#
# glapi2xml.pl : Generates an XML representation of the GL API.
#
##############################################################

use XML::LibXML;
use C::Scan;

if (!$ARGV[2]) {
	die "Usage: gapi2xml.pl <namespace> <outfile> <infile>\n";
}

$ns = $ARGV[0];
$pps = $ARGV[2];

##############################################################
# If a filename was provided see if it exists.  We parse existing files into
# a tree and append the namespace to the root node.  If the file doesn't 
# exist, we create a doc tree and root node to work with.
##############################################################

if ($ARGV[1] && -e $ARGV[1]) {
	#parse existing file and get root node.
	$doc = XML::LibXML->new->parse_file($ARGV[1]);
	$root = $doc->getDocumentElement();
} else {
	$doc = XML::LibXML::Document->new();
	$root = $doc->createElement('api');
	$doc->setDocumentElement($root);
}

$ns_elem = $doc->createElement('namespace');
$ns_elem->setAttribute('name', $ns);
$root->appendChild($ns_elem);

$c = new C::Scan(filename => $pps);

#
# Fetch and iterate through information about function declarations.
#
# do the types
my $hash_ref = $c->get('typedef_hash');
while ( ($k, $v) = each %$hash_ref ) {
        #print "$k => @$v[1]\n";
        #print "@$v[0]\n";

        my $elem = $doc->createElement('type');
        @$v[0] =~ s/^ //;
        @$v[0] =~ s/ $//;
	$elem->setAttribute('aname', $k);
	$elem->setAttribute('atype', @$v[0]);
	$ns_elem->appendChild($elem);

        $tcnt = $tcnt + 1;
}

#
# Fetch and iterate through information about #define values w/out args.
#
my $hash_ref = $c->get('defines_no_args');
while ( ($k,$v) = each %$hash_ref ) {
        #print "name: $k, value: $v\n";

        my $elem = $doc->createElement('const');
        $elem->setAttribute('name', $k);
        $elem->setAttribute('val', $v);
	$ns_elem->appendChild($elem);

        $ccnt = $ccnt + 1;
}

my @array_ref = @{$c->get('parsed_fdecls')};
foreach my $func (@array_ref) {
    my ($type, $name, $args, $full_text, undef) = @$func;
    $fcnt = $fcnt + 1;

    my $elem = $doc->createElement('func');
    $elem->setAttribute('name', $name);
    $elem->setAttribute('type', $type);

    #print("func: $name, retval: $type\n");
    #print("---\n");

    my $params = $doc->createElement('parameters');

    foreach my $arg (@$args) {
        my ($atype, $aname, $aargs, $full_text, $array_modifiers) = @$arg;
        #printf("atype: $atype, aname: $aname, aargs: $aargs, am: $array_modfifers\n");

	my $param = $doc->createElement('param');

	$atype =~ /^(\w+)\s+\((\*).*\)\s*\((.*)\)/;
	#print "---\n";
	#print "$1\n";
	#print "$2\n";
	#print "$3\n";

	if ($2 eq "*") {
		$param->setAttribute('callback', "t");
		my $callback = $doc->createElement('callback');
		$param->appendChild($callback);

		$callback->setAttribute("type", $1);

		my @cpars = split(/,/, $3);

		foreach my $cbarg (@cpars) {
			$cbp = $doc->createElement('param');
			$cbarg =~ /\s?(.*)\s(.*)$/;

			$nme = $2;
			if ($2 eq "*") {
				$nme = "arg_$fcnt";
			}
			$cbp->setAttribute('name', $nme);
			$cbp->setAttribute('type', $1);
			$callback->appendChild($cbp);
		}
			
	} else {
		$param->setAttribute('callback', "f");
		$param->setAttribute('name', $aname);
		$param->setAttribute('type', $atype);
	}
	
	$params->appendChild($param);
    }

    $elem->appendChild($params);
    $ns_elem->appendChild($elem);

    #print("xxx\n");
}

##############################################################
# Output the tree
##############################################################

if ($ARGV[1]) {
	open(XMLFILE, ">$ARGV[1]") || 
				die "Couldn't open $ARGV[1] for writing.\n";
	print XMLFILE $doc->toString();
	close(XMLFILE);
} else {
	print $doc->toString();
}

##############################################################
# Generate a few stats from the parsed source.
##############################################################

print "funcs: $fcnt types: $tcnt  consts: $ccnt\n";

