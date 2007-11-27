#!/usr/bin/perl

# This program sets up the sound control file for sound effects in the game.
# Like create-credits.pl, this uses SVN properties to associate a given
# file with a given sound effect.
#
#
# The following properties are used:
#
#   cutegod:sound - The sound keys
#
# Sound keys are typically of a given mode:
#
#   Block Landed
#   Block Grabbed
#
# Some of them, have additional "- Block Type - Block Type" to allow
# for more detailed controled. For example: "Block Landed - Water
# Block - Grass Block" or "Block Landed - Water Block". In this case,
# the first one is the block that has been landed on, the second is
# the block doing the landing.
#
# If there are multiple sounds, they will be separated by a "\s*,\s*"

#
# Setup
#

# Directives
use strict;
use warnings;

#
# Start by getting the directory and looping through the files
# inside that directory.
#

# Check for a directory
die "You must supply a directory name" if (@ARGV == 0) || (! -d $ARGV[0]);
my $root = $ARGV[0];

# Top-Level
my %categories = ();

# Find all the files in the directory
my $cmd = "find '$root' -type f";
open PIPE, "$cmd |";

while (<PIPE>)
{
    # Clean up the line and get the file
    chomp;
    next if /\.svn/ || /_svn/;
    my $filename = $_;

    # Look for properties
    my $sound = get_svn_prop("cutegod:sound", $_);

    # Don't bother if we don't have at least minimum data
    next unless $sound ne "";

    # Split out the sounds
    my @sounds = split(/\s*,\s*/, $sound);

    foreach $sound (@sounds)
    {
	# Adjust the filename
	$filename =~ s@^$root/@@;

	# Get the category
	my $cat = $categories{$sound};
	$cat = () unless defined $cat;

	# Add the sound
	push @$cat, $filename;
	$categories{$sound} = $cat;
    }

    # Say we processed it
    print "$filename\n";
}

close PIPE;

# Gets a propery from the SVN tree
sub get_svn_prop
{
    my $prop = shift;
    my $file = shift;
    my $cmd = "svn propget $prop '$file'";
    my $value = `$cmd`;
    $value =~ s/^\s+//sg;
    $value =~ s/\s+$//sg;
    return $value;
}

#
# Create the credits file
#

# Create the file
open CREDITS, ">$root/sounds.xml" or die;
print CREDITS "<sounds>\n";

# Go through the categories, alphabetical
foreach my $category (sort keys %categories)
{
    # Make some noise
    my $cat = $categories{$category};
    print "Generating: $category\n";
    print CREDITS "  <category title='$category'>\n";

    # Go through the credtors
    foreach my $sound (@$cat)
    {
	# Add the sound
	print "  $sound\n";
	print CREDITS "    <sound>$sound</sound>\n";
    }

    # Finish up
    print CREDITS "  </category>\n";
}

# Close the file
print CREDITS "</sounds>\n";
close CREDITS;
