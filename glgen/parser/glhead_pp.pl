#!/usr/bin/perl
#
# Parses GL headers.  Fun.
# Brutalized by: Mark Crichton <crichton@gimp.org>

use C::Scan;

foreach $dir (@ARGV) {
	@hdrs = (@hdrs, `ls $dir/*.h`);
}

foreach $fname (@hdrs) {

	$c = new C::Scan(filename => $fname);

	my @array_ref =@{$c->get('parsed_fdecls')};

	foreach my $func (@array_ref) {
		my ($type, $name, $args, $full_text, undef) = @$func;

		print("$type $name(");

		$i = scalar @$args;
		$j = 1;

		foreach my $arg (@$args) {
			my($type, $name, undef, $ft, $mod) = @$arg;

			print("$mod $type $name");

			if ($j != $i) {
				print(",");
			}

			$j = $j + 1;	
		}

		print(")\n");
	}
}
