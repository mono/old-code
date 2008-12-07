#!/usr/bin/perl
#
# $Id: $
#

=head1 NAME

bsd-man-to-xml.pl - Converts BSD man pages into ECMA XML.

=head1 SYNOPSIS

zcat /path/to/man/page.gz | B<bsd-man-to-xml.pl> > page.xml

=head1 DESCRIPTION

Parses a BSD nroff man page and tries to convert it into an ECMA XML
documentation block.  It generates the <summary/>, <remarks/>, and <returns/>
XML sections.

This program is B<NOT> currently suitable for converting Linux man pages, 
since the Linux man pages use different nroff macros.  The Linux man pages 
are also less semantic and more output-oriented -- for example, BSD will 
use C<.Fn function> to name a function, while Linux will use 
C<.B function> (.B bolds the named item, with no semantic implications for 
what it's bolding).

=head1 NOTES

You I<will> need to edit the generated text.  Not all nroff macros are
supported, and the translator needs to guess about some things.  In
particular, search for C<TODO> (for enumeration values, since this program
cannot know which enumeration all constants belong to) and C<Syscall> (since
we assume all functions are in Syscall, when many are really in Stdlib).

=head1 COPYRIGHT

Copyright (C) 2006 Jonathan Pryor  <jonpryor@vt.edu>

=cut

#
# Copyright (C) 2006 Jonathan Pryor  <jonpryor@vt.edu>
#
# Permission is hereby granted, free of charge, to any person obtaining
# a copy of this software and associated documentation files (the
# "Software"), to deal in the Software without restriction, including
# without limitation the rights to use, copy, modify, merge, publish,
# distribute, sublicense, and/or sell copies of the Software, and to
# permit persons to whom the Software is furnished to do so, subject to
# the following conditions:
# 
# The above copyright notice and this permission notice shall be
# included in all copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
# EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
# MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
# NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
# LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
# OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
# WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#

use strict;

my $ignore_input = undef;
my @sections = ();
my $cref_see = 1;
my $indent = 0;

my $printer = undef;

my $commands = {
	'\\"' => sub {}, # comment; ignore
	'Dd' => sub {}, # date; ignore
	'Dt' => sub {}, # man page header; ignore
	'Os' => sub {}, # wtf?; ignore
	'Sh' => \&command_section_start,
	'Nm' => sub {}, # function name within NAME section; ignore
	'Nd' => \&command_summary,
	'Xr' => \&command_cref,
	'Dv' => \&command_constant,
	'Fa' => \&command_parameter,
	'Ql' => \&command_quote,
	'Pp' => \&command_paragraph,
	'Fn' => \&command_function,
	'In' => \&command_include,
	'Va' => \&command_variable,
	'Er' => \&command_error_variable,
	'It' => \&command_list_item,
	'Bl' => \&command_list_begin,
	'El' => \&command_list_end,
	'Bf' => sub {}, # begin format (e.g. bold); ignore
	'Ef' => sub {}, # end formatting (e.g. bold); ignore
	'Bq' => \&command_bracket_quote,
	'Er' => sub {command_constant (shift);},
	'Tn' => sub {$_ = shift; my ($tn) = /^"([^"]+)"$/; print_lead ($tn);},
	'Sy' => \&command_list_item_header,
	'Bd' => \&command_indent_begin,
	'Ed' => \&command_indent_end,
	'Sx' => sub {command_quote (shift);},
	'Dq' => sub {print_lead ("<c>", shift, "</c>\n");},
	'Pq' => sub {command_parse (shift);},
	'Rv' => sub {}, # no idea what it does; ignore
	'Em' => sub {print_lead ("<i>", shift, "</i>\n");},
};

my $section_handlers = {
	'NAME' => sub {
		$ignore_input = undef; 
		section_begin ("summary", "para");
	},
	'DESCRIPTION' => sub {
		$ignore_input = undef; 
		section_end ("para", "summary"); 
		section_begin ("remarks", "para");
	},
	'RETURN VALUES' => sub {
		$ignore_input = undef; 
		section_end (); 
		section_end ("remarks"); 
		section_begin ("returns", "para");
		# Some man pages don't print out what the return value is.
		# Provide a stock response.
		$printer = sub {
			$_ = shift;
			$printer = undef;
			if ($_ eq "</para>\n") {
				print_lead ("  On success, zero is returned.\n");
				print_lead ("  On error, -1 is returned and \n");
				print_lead ("  <see cref=\"M:Mono.Unix.Native.Stdlib.GetLastError\" />\n");
				print_lead ("  returns the translated error.\n");
				print_lead ($_);
			}
			else {
				print_lead ($_);
			}
		};
	},
	'ERRORS' => sub {
		$ignore_input = undef; 
		section_end ("para"); 
		section_begin ("block subset=\"none\" type=\"usage\"", "para");
	},
	'SEE ALSO' => sub {
		$ignore_input = undef; 
		while (@sections) {
			section_end ();
		} 
		$cref_see = 0;
	},
};

while (<>) {
	my ($line);
	if (($line) = /^\.(.*)$/) {
		command_parse ($line);
	}
	else {
		print_lead ($_);
	}
}

sub command_parse {
	$_ = shift;
	my ($arg, $rest);
	if (($arg, undef, $rest) = /^([\w\\"]+)(\s(.*))?$/ and exists $commands->{$arg}) {
		return $commands->{$arg}->($rest);
	}
	else {
		print_lead ($_);
	}
	return undef;
}

sub command_section_start {
	$_ = shift;
	my $name;
	my $rest;
	if (($name, $rest) = /^([\w\s]+)(.*)$/ and exists $section_handlers->{$name}) {
		$section_handlers->{$name}->();
		return $rest;
	}
	else {
		$ignore_input = 1;
	}
	return $_;
}

sub command_summary {
	print_lead (shift, "\n");
	return "";
}

sub command_cref {
	$_ = shift;
	my ($cmd, $section, $rest) = /^(\w+)\s(\w+)(.*)/;
	if ($cref_see) {
		if ($section ne "2" and $section ne "3") {
			print_lead ("<c>$cmd</c>($section)$rest\n");
		}
		elsif ($cref_see){
			print_lead ("<see cref=\"M:Mono.Unix.Native.Syscall.$cmd\" />($section)$rest\n");
		}
		return "";
	}
	elsif (!$cref_see and ($section eq "2" or $section eq "3")) {
		print_lead ("<altmember cref=\"M:Mono.Unix.Native.Syscall.$cmd\" />\n");
	}
	return "";
}

sub command_constant {
	$_ = shift;
	my ($pre, $arg, $rest) = /^([^\w]*)(\w+)(.*)$/;
	my $type = "TODO";
	if    ($arg =~ /SIG_/)    {$type = "Stdlib";}
	elsif ($arg =~ /SIG[^_]/) {$type = "Signum";}
	elsif ($arg =~ /O_/)      {$type = "OpenFlags";}
	elsif ($arg =~ /E[^_]+/)  {$type = "Errno";}
	elsif ($arg =~ /._OK/)    {$type = "AccessModes";}
	print_lead ("$pre <see cref=\"F:Mono.Unix.Native.$type.$arg\" /> $rest\n");
	return "";
}

sub command_parameter {
	$_ = shift;
	my ($arg, $rest) = get_possibly_quoted_string ($_);
	print_lead ("<paramref name=\"$arg\" /> $rest\n");
	return "";
}

sub get_possibly_quoted_string {
	$_ = shift;
	my ($a, $b, undef, $rest) = /^("([^"]+)"|\w+)(\s(.*))?$/;
	return (($b || $a), $rest);
}

sub command_paragraph {
	section_end ("para");
	section_begin ("para");
	return "";
}

sub command_bracket_quote {
	$_ = shift; 
	command_parse ($_);
}

sub command_quote {
	$_ = shift;
	# my ($arg, $rest) = /^(\S+)(\s[.,])?$/;
	my ($arg, $rest) = get_possibly_quoted_string ($_);
	$arg =~ s/\\&//g;
	print_lead ("\"<c>$arg</c>\"$rest\n");
	return "";
}

sub command_function {
	$_ = shift;
	my ($fn, $args, $rest) = /^(\w+)\s*(".*")?\s*(.*)$/;
	$args =~ s/"(.*)"/$1/;
	my @args = split /" "/, $args;
	print_lead ("<c>$fn</c>(", join (", ", @args), ")$rest\n");
	return "";
}

sub command_include {
	$_ = shift;
	my ($arg, $rest) = /^([\w\.\/]+)(.*)$/;
	print_lead ("<c>$arg</c>$rest\n");
	return "";
}

sub command_variable {
	$_ = shift;
	my ($arg, $rest) = /^(\w+)(.*)$/;
	if ($arg eq "errno") {
		print_lead ("<see cref=\"M:Mono.Unix.Native.Stdlib.GetLastError\" />$rest\n");
	}
	else {
		print_lead ("<c>$arg</c>$rest\n");
	}
	return "";
}

sub command_error_variable {
	$_ = shift;
	my ($arg, $rest) = /^(\w+)(.*)$/;
	print_lead ("<see cref=\"M:Mono.Unix.Native.Errno.$arg\" />$rest\n");
	return "";
}

my $list_type;

sub command_list_begin {
	$_ = shift;
	section_end ("para");
	my ($type) = /^([-\w]+)$/;
	if ($type eq "-bullet") {
		$list_type = "bullet";
		section_begin ("list type=\"bullet\"");
	}
	else {
		$list_type = "table";
		section_begin ("list type=\"table\"");
	}
}

sub command_list_item_header {
	$_ = shift;

	my $header;
	if (($header) = /^"([^"]+)"/) {
		section_begin ("listheader");
		my @items = split /\t/, $header;
		section_begin ("term");
		print_lead (shift @items, "\n");
		section_end ("term");
		foreach my $t (@items) {
			section_begin ("description");
			print_lead ($t, "\n");
			section_end ("description");
		}
		section_end ("listheader");
	}
}

sub command_list_item {
	$_ = shift;

	if (/^Sy\s/) {
		command_parse ($_);
		return "";
	}
	elsif ($sections [-1] eq "list" and /^Bq Er E\w+$/) {
		# start of an errno list.
		section_begin ("listheader", "term");
		print_lead ("Error\n");
		section_end ("term");
		section_begin ("description");
		print_lead ("Details\n");
		section_end ("description", "listheader");
	}

	if ($sections [-1] eq "para") {
		section_end ("para");
		section_end (); # term or description
		section_end ("item");
	}

	section_begin ("item", "term");
	my @descs = split /\sTa\s/, $_;
	command_parse (shift @descs);
	print "\n";

	if ($list_type eq "bullet") {
		section_begin ("para");
		return "";
	}

	section_end ("term");

	if (@descs == 0) {
		section_begin ("description", "para");
		return "";
	}
	foreach my $desc (@descs) {
		section_end ("para", "description") if $sections [-1] eq "para";
		section_begin ("description", "para");
		my $rest = undef;
		if ($desc =~ /^"/) {
			($desc, $rest) = $desc =~ /^"([^"]+)"(.*)$/;
		}
		command_parse ($desc);
		command_parse ($rest) if $rest;
		print "\n";
	}
	return "";
}

sub command_list_end {
	if ($sections [-1] eq "para") {
		section_end ("para");
		section_end (); # term or description
		section_end ("item");
	}
	section_end ("list");
	section_begin ("para");
}

my $num_indent_entries = undef;

sub command_indent_begin {
	section_end ("para");

	$printer = \&command_indent_line;
}

sub command_indent_line {
	my $line = shift;
	my @entries = split /\t+/, $line;

	unless (defined $num_indent_entries) {
		$num_indent_entries = scalar @entries;
		if ($num_indent_entries == 1) {
			print_indent ();
			print "<block subset=\"none\" type=\"usage\">\n";
		}
		else {
			print_indent (); print ("<list type=\"table\">");
			print_indent (); print ("  <listheader>\n");
			print_indent (); print ("    <term>Value</term>\n");
			for (my $i = 1; $i < $num_indent_entries; ++$i) {
				print_indent (); print ("    <description>Details</description>\n");
			}
			print_indent (); print ("  </listheader>\n");
		}
	}

	if ($num_indent_entries == 1) {
		my $l = $entries [0];
		$l =~ s/\n$//;
		print_indent (); print "  <para>", $l, "</para>\n";
	}
	else {
		print_indent (); print "  <item>\n";
		print_indent (); print "    <term>", (shift @entries), "</term>\n";
		foreach my $e (@entries) {
			my $g = $e;
			$g =~ s/\n$//;
			print_indent (); print "    <description>$g</description>\n";
		}
		print_indent (); print "  </item>\n";
	}
}

sub command_indent_end {
	$printer = undef;
	if ($num_indent_entries == 1) {
		print_lead (); print "</block>\n";
	}
	else {
		print_lead (); print "</list>\n";
	}
	$num_indent_entries = undef;
	section_begin ("para");
}

sub print_indent {
	unless ($ignore_input) {
		print "        ";
		print ("  " x $indent);
	}
}

sub print_lead {
	unless ($ignore_input) {
		if (defined $printer) {
			$printer->(@_);
		}
		else {
			print_indent ();
			foreach my $a (@_) {
				my $b = $a;
				$b =~ s/\\(.)/$1/g;
				print $b;
			}
		}
	}
}

sub section_begin {
	foreach my $section (@_) {
		print_lead ("<$section>\n");
		my ($tag) = $section =~ /^(\w+)/;
		push @sections, $tag;
		++$indent;
	}
}

sub section_end {
	my $end = sub {
		my $section = shift;
		die unless defined $section;
		--$indent;
		print_lead ("</$section>\n");
	};

	if (scalar @_) {
		foreach my $section (@_) {
			my ($expected) = pop @sections;
			my (undef, undef, $line, $method) = caller;
			die "internal error: expected '$expected' but got '$section' " . 
				"(at $method:$line)" if $section ne $expected;
			$end->($section);
		}
	}
	else {
		$end->(pop @sections);
	}
}

