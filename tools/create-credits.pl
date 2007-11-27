#!/usr/bin/perl

# This program creates a credits.xml file in a given directory. The
# credits file uses SVN properties to generate and organize the files
# appropriately to let a program create a movie-style credit mode.
#
# The following properties are used:
#   creator - The name of the person or group or source
#   asset - The title used used
#   url - The URL to find the piece
#   license - The license of the piece
#   category - The category for the license
#   title - The title of the creator
#
# This should be used to update the credits file for CuteGod.

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
    next if m@/bin/@;
    next if m@~$@;
    my $filename = $_;

    # Look for properties
    my $creator = get_svn_prop("creator", $_);
    my $asset = get_svn_prop("asset", $_);
    my $title = get_svn_prop("title", $_);
    my $license = get_svn_prop("license", $_);
    my $url = get_svn_prop("url", $_);
    my $category = get_svn_prop("category", $_);

    # Don't bother if we don't have at least minimum data
    next unless $creator ne "";
    next if ($title eq "" && $asset eq "");

    # Normalize the category and get the hash
    $category = "Other" unless $category ne "";

    my $cat_ref = $categories{$category};
    $cat_ref = {} unless defined $cat_ref;
    $categories{$category} = $cat_ref;

    # Look for the creator
    my $creator_ref = $$cat_ref{$creator};
    $creator_ref = {} unless defined $creator_ref;
    $$cat_ref{$creator} = $creator_ref;

    # If we have a title, put it in
    my %attrs = ();
    $attrs{url} = $url if $url ne "";
    $attrs{license} = $license if $license ne "";

    if ($title ne "")
    {
	$$creator_ref{"title:$title"} = \%attrs;
    }

    if ($asset ne "")
    {
	$$creator_ref{"asset:$asset"} = \%attrs;
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
open CREDITS, ">$root/credits.xml" or die;
print CREDITS "<credits>\n";

# Go through the categories, alphabetical
foreach my $category (sort keys %categories)
{
    # Make some noise
    my $cat = $categories{$category};
    print "Generating: $category\n";
    print CREDITS "  <category title='$category'>\n";

    # Go through the credtors
    foreach my $creator (sort keys %$cat)
    {
	# Start the creator stuff
	print "  Creator: $creator\n";
	print CREDITS "    <creator name='$creator'>\n";

	# Go through the values
	my $cref = $$cat{$creator};
	while (my ($key, $ref) = each(%$cref))
	{
	    # Figure out what we are doing
	    if ($key =~ s/^asset://)
	    {
		print "    Asset: $key\n";
		print CREDITS "      <asset";
		print_attributes($ref);
		print CREDITS ">$key</asset>\n";
	    }

	    if ($key =~ s/^title://)
	    {
		print "    Title: $key\n";
		print CREDITS "      <title";
		print_attributes($ref);
		print CREDITS ">$key</title>\n";
	    }
	}

	# Finish up
	print CREDITS "    </creator>\n";
    }

    # Finish up
    print CREDITS "  </category>\n";
}

# Close the file
print CREDITS "</credits>\n";
close CREDITS;

sub print_attributes
{
    my $ref = shift;

    if (defined $$ref{url})
    {
	print CREDITS " href='$$ref{url}'";
    }

    if (defined $$ref{license})
    {
	print CREDITS " license='$$ref{license}'";
    }
}
