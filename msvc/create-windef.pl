#!/usr/bin/perl -w

use strict;

my $outfile = shift || usage ();
my @symbols = ();
my %excludes = ();
my $cmd = "nm -D ../mono/mini/.libs/libmono-2.0.so";

@excludes {qw(
	mono_class_setup_vtable_general_new mono_debugger_init mono_debugger_main
	mono_once mono_pthread_key_for_tls
)} = ();

open (SYMS, "$cmd |") || die "Cannot run \$cmd': $!\n";
while (<SYMS>) {
	next unless / T (mono_.*)/;
	next if exists $excludes {$1};
	push @symbols, $1;
}
close (SYMS);
push @symbols, "MonoFixupCorEE";
@symbols = sort @symbols;

open (OUT, ">$outfile") || die "Cannot open '$outfile': $!\n";
print OUT "; file generated by create-windef.pl\n";
print OUT "LIBRARY mono.dll\nEXPORTS\n";
print OUT join ("\n", @symbols);

close (OUT);

sub usage {
	print "Usage: create-windef.pl output_file\n";
	exit (1);
}

