#!/usr/bin/perl -w

# check-missing.pl -- double check missing files
# Scott Bronson
# 19 Oct 2002

# Runs locate on all the files that generate.pl claims are missing
# to see if they really ARE missing.
#
# Run this command with no arguments.  It runs generate, then parses
# its list of output files.  Generate works incrementally if the
# output directory exists and is up to date.


use strict;

my %exists = ();
my $count = 0;


print "Running generate.pl.  This might take a while...\n";

open(FH, "./generate.pl -q -m|") or die
	"Could not fork generate.pl: $!\n";
while(<FH>) {
	next unless /\/([^\/]+\.xml)/;
	my $file = $1;
	`locate /$file`;
	$count += 1;
	my $exit_val = ($? >> 8) & 0xFF;
	print "$file";
	if(!$exit_val) {
		print "  <---- ACTUALLY EXISTS!\n";
		$exists{$file}++;
	}
	print "\n";
}
close FH;


# Ensure child ran without errors
my $signal_no = $? & 0xFF;
my $exit_val = ($? >> 8) & 0xFF;
if($signal_no || $exit_val) {
	die "generate.pl bailed on signal $signal_no " .
		"(exit val=$exit_val)\n" if $signal_no;
	die "generate.pl exited with value $exit_val\n"
		if $exit_val;
}


print "$count files checked, " . scalar(keys(%exists)) .
	" files found.\n";

