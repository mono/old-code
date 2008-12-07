#!/usr/bin/perl
#
# $Id: $
#

=head1 NAME

bsd-man-to-exception.pl - Converts BSD man pages into ECMA XML <exception/> elements

=head1 SYNOPSIS

zcat /path/to/man/page.gz | B<bsd-man-to-exception.pl> > page.xml

=head1 DESCRIPTION

Parses a BSD nroff man page looking for the C<ERRORS> section and converts the
error numbers found into ECMA XML <exception/> elements.  The errno values
found are mapped to the best-fit .NET Exception that will be thrown by
C<Mono.Unix.UnixMarshal.ThrowExceptionForError(Mono.Unix.Native.Errno)>.

This program is B<NOT> currently suitable for converting Linux man pages, 
since the Linux man pages use different nroff macros.  The Linux man pages 
are also less semantic and more output-oriented -- for example, BSD will 
use C<.Fn function> to name a function, while Linux will use 
C<.B function> (.B bolds the named item, with no semantic implications for 
what it's bolding).

=head1 NOTES

You should review the generated text, to ensure conformance with argument
names and other matters.

The exception mapping in get_exception_type() should be kept in sync with 
C<Mono.Unix.UnixMarshal.CreateExceptionForError(Mono.Unix.Native.Errno)>.

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

my $in_errors = undef;

# $errors->{ExceptionType}->[ message1, message2 ...]
my $errors = {};

my $last_errno = undef;
my $message = "";

my $commands = {
	".It" => \&command_begin_error,
	".Dv" => \&command_constant,
	".Fa" => \&command_parameter,
	".Ql" => \&command_quote,
	".El" => \&command_end_error,
	".Sh" => sub { $in_errors = undef; },
};

while (<>) {
	unless (defined $in_errors) {
		$in_errors = 1 if (/^\.Sh.*ERRORS$/);
		next;
	}
	chomp;
	my $arg;
	if (($arg) = /^(\.[\w\\"]+)/) {
		if (exists $commands->{$arg}) {
			$commands->{$arg}->($_);
		}
		else {
			append_message ($_);
		}
	}
	else {
		append_message ($_);
	}
}

command_end_error ();

foreach my $etype (sort etype_sort keys %$errors) {
	create_exception ($etype , $errors->{$etype});
}

sub append_message {
	my $append = shift;
	if ($message eq "") {
		$message = $append;
	}
	else {
		$message .= "\n            $append";
	}
}

sub add_error {
	my $errors = shift;
	my $etype  = shift;
	my $message= shift;

	unless (exists $errors->{$etype}) {
		$errors->{$etype} = [];
	}

	my $elist = $errors->{$etype};
	push @$elist, $message;
}

sub get_exception_type {
	my $errno = shift;

	return "System.ArgumentException"           if $errno eq "EINVAL" or 
		$errno eq "EBADF";
	return "System.ArgumentOutOfRangeException" if $errno eq "ERANGE";
	return "System.IO.DirectoryNotFoundException" if $errno eq "ENOTDIR";
	return "System.IO.FileNotFoundException"    if $errno eq "ENOENT";
	return "System.InvalidOperationException"   if $errno eq "EOPNOTSUPP" or
		$errno eq "EPERM";
	return "System.InvalidProgramException"     if $errno eq "ENOEXEC";
	return "System.IO.IOException"              if $errno eq "EIO" or 
	  $errno eq "ENOSPC" or $errno eq "ENOTEMPTY" or $errno eq "ENXIO" or 
		$errno eq "EROFS" or $errno eq "ESPIPE";
	return "System.NullReferenceException"      if $errno eq "EFAULT";
	return "System.OverflowException"           if $errno eq "EOVERFLOW";
	return "System.IO.PathTooLongException"     if $errno eq "ENAMETOOLONG";
	return "System.UnauthorizedAccessException" if $errno eq "EACCESS" or
		$errno eq "EISDIR";
	return "Mono.Unix.UnixIOException";
}

sub command_begin_error {
	shift;
	my $arg;
	if (($arg) = /^\.It Bq Er (.+)$/) {
		command_end_error ();
		$last_errno = $arg;
	}
	else {
		append_message ($_);
	}
}

sub command_end_error {
	if (defined $last_errno) {
		append_message ("[<see cref=\"F:Mono.Unix.Native.Errno.$last_errno\" />]");
		add_error ($errors, get_exception_type ($last_errno), $message);
	}
	$last_errno = undef;
	$message = "";
}

sub command_constant {
	shift;
	my ($arg, $rest) = /^\.Dv (\w+)(.*)$/;
	append_message ("<see cref=\"F:Mono.Unix.Native.TODO.$arg\" /> $rest");
}

sub command_parameter {
	shift;
	my ($arg, $rest) = /^\.Fa (\w+)(.*)$/;
	append_message ("<paramref name=\"$arg\" /> $rest");
}

sub command_quote {
	shift;
	my ($arg, $rest) = /^\.Ql (\S+)( [.,])?$/;
	$arg =~ s/\\&//g;
	append_message ("\"<c>$arg</c>\"$rest");
}

sub create_exception {
	my $etype = shift;
	my $elist = shift;

	print <<EOF;
        <exception cref="$etype">
EOF
	my $first_message = shift @$elist;
	print_message ($first_message);
	foreach my $message (@$elist) {
		print <<EOF;
          <para>-or-</para>
EOF
		print_message ($message);
	}
	unshift @$elist, $first_message;
	print <<EOF;
        </exception>
EOF
}

sub print_message {
	my $message = shift;
	print <<EOF;
          <para>
            $message
          </para>
EOF
}

# Sorts exception types so that System.* exceptions come before Mono.*
# exceptions, and fewer namespaced types are before greater namespaced types
# (e.g. System.UnauthorizedAccessException before
# System.IO.FileNotFoundException).
sub etype_sort {
	my @aparts = split /\./, $a;
	my @bparts = split /\./, $b;

	my $acnt = scalar @aparts;
	my $bcnt = scalar @bparts;

	# return shortest path first, except for Mono.* (which is always last)
	if ($acnt < $bcnt) {
		return -1 if $aparts [0] eq "System";
		return  1 if $bparts [0] eq "System";
		return $aparts [$acnt] cmp  $bparts [$acnt];
	}
	elsif ($acnt == $bcnt) {
		return -1 if ($aparts [0] eq "System" and $bparts [0] eq "Mono");
		return  1 if ($aparts [0] eq "Mono" and $bparts [0] eq "System");
		return $a cmp $b;
	}
	else { # $bcnt < $acnt;
		return  1 if $aparts [0] eq "System";
		return -1 if $bparts [0] eq "System";
		return $aparts [$acnt] cmp  $bparts [$acnt];
	}
}

sub min {
	my ($a, $b) = @_;
	return ($a < $b) ? $a : $b;
}

