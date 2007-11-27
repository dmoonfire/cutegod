#!/usr/bin/perl

# Implements a processor for converting layouts.txt to
# layouts.xml. The XML file is optimized for processing by the system,
# but the text layout file is easier to create layouts in a visual
# manner.
#
# The layout is:
# = Layout
#
# I I I
# I * I
# I I I
#
# I I I
# I I I
# I I I
#
# Where "I" is the initials of the layout file and "*" is a missing element.

#
# Setup
#

# Directives
use strict;
use warnings;

# Modules

# Variables
my $DEBUG = 0;

#
# Read the layouts
#

# Working variables
my $in_layout = 0;
my $new_paragraph = 0;
my @paragraph = ();
my @layout = ();
my %stats = ();

# Start the xml
print "<layouts>\n";

# Loop
while (<>)
{
    # Clean up the line and normalize it
    chomp;
    s/^\s+//;
    s/\s+$//;
    s/\s+/ /g;

    # Look for layout
    if (/^=/)
    {
	# If we have a layout, emit it
	emit_layout() if ($in_layout);

	# Create a new layout
	$in_layout = 1;
	$new_paragraph = 1;
	@layout = ();
	print STDERR "New Layout\n" if $DEBUG;

	# Finish up
	next;
    }

    # Look for paragraphs
    if (/^$/)
    {
	# If we are in a new paragraph, ignore it
	next if $new_paragraph;
	$new_paragraph = 1;

	# Save the layout
	my @pl = @paragraph;
	push @layout, \@pl;
	@paragraph = ();

	# Make some noise
	print STDERR "  Paragraph\n" if $DEBUG;	

	# Finish up
	next;
    }

    # Split out the elements
    my @p = split(/ /);
    print STDERR "    ", join(",", @p), "\n" if $DEBUG;
    push @paragraph, \@p;
    $new_paragraph = 0;
}

# Finish up
if (!$new_paragraph)
{
    print STDERR "  Final Paragraph\n" if $DEBUG;
    push @layout, \@paragraph;
    emit_layout();
}

# Finish the xml
print "</layouts>\n";

#
# Statistics
#

print STDERR join("\n",
		  "Layouts:"), "\n";

foreach my $s (keys %stats)
{
    print STDERR "  $s: " . $stats{$s} . "\n";
}

#
# Emitting
#

my %layout_columns = ();

sub emit_layout
{
    # Make some noise
    print STDERR "  Emitting...\n";

    # Clear the fields
    %layout_columns = ();

    # Go through the layout and rebuild them in the appropriate layout
    # for XML
    my $height = @layout;
    my $column_count = 0;
    my $row_count = 0;

    foreach my $pref (@layout)
    {
	# Make some noise
	$height--;
	print STDERR "    Paragraph $height\n" if $DEBUG;

	# Go through the rows
	my $rows = 0;
	$row_count = @$pref;

	foreach my $rref (@$pref)
	{
	    # Noise
	    print STDERR "      Row $rows\n" if $DEBUG;

	    # Go through the columns
	    my $columns = 0;
	    $column_count = @$rref;

	    foreach my $block (@$rref)
	    {
		# Everything but "*" is processed
		if ($block ne "*")
		{
		    # Noise
		    print STDERR "        Block $columns: $block\n" if $DEBUG;

		    # Register the block
		    assign_block($height, $rows, $columns, $block);
		}

		# Increment the column counter
		$columns++;
	    }

	    # Increment the counter
	    $rows++;
	}
    }

    # Create the XML
    $stats{"$row_count" . "x$column_count"}++;
    print "  <board columns='$column_count' rows='$row_count'>\n";

    # Go through the columns
    foreach my $ckey (sort numerically keys %layout_columns)
    {
	# Create the column
	print "    <column>\n";

	# Go through the rows
	my $cref = $layout_columns{$ckey};

	foreach my $rkey (sort numerically keys %{$layout_columns{$ckey}})
	{
	    my $rref = $$cref{$rkey};

	    print "      <row>\n        ";
	    print join("\n        ", @$rref), "\n";
	    print "      </row>\n";
	}

	# Finish the column
	print "    </column>\n";
    }

    # Finish up
    print "  </board>\n";
}

sub assign_block
{
    # Get the variables
    my $position = shift;
    my $y = shift;
    my $x = shift;
    my $initials = shift;

    # Map the block name
    my $name = get_block_name($initials);

    # Get the columns
    my $cref = $layout_columns{$x};
    $cref = {} unless defined $cref;

    # Get the row
    my $rref = $$cref{$y};
    $rref = () unless defined $cref;

    # Add the XML
    my $shadows = get_shadows($name);
    my $height = get_height($name);
    push @$rref, "<block name=\"$name\" position=\"$position\" "
	. "height=\"$height\" cast-shadows=\"$shadows\" />";

    # Put things back
    $$cref{$y} = $rref;
    $layout_columns{$x} = $cref;
}

sub get_block_name
{
    my $init = shift;

    return "Invisible"          if ($init eq "I");

    return "Grass Block"        if ($init eq "GB");
    return "Dirt Block"         if ($init eq "DB");
    return "Water Block"        if ($init eq "WB");

    return "Sealed Grass"       if ($init eq "SG");
    return "Sealed Dirt"        if ($init eq "SD");
    return "Sealed Water"       if ($init eq "SW");

    return "Stone Block"        if ($init eq "SB");
    return "Stone Block Tall"   if ($init eq "SBT");
    return "Brown Block"        if ($init eq "BB");

    return "Gem Blue"           if ($init eq "GB");
    return "Gem Green"          if ($init eq "GG");
    return "Gem Orange"         if ($init eq "GO");

    return "Roof North West"    if ($init eq "RNW");
    return "Roof South West"    if ($init eq "RSW");
    return "Roof North East"    if ($init eq "RNE");
    return "Roof South East"    if ($init eq "RSE");
    return "Roof North"         if ($init eq "RN");
    return "Roof South"         if ($init eq "RS");
    return "Roof East"          if ($init eq "RE");
    return "Roof West"          if ($init eq "RW");
    return "Rock"               if ($init eq "R");

    return "Ramp West"          if ($init eq "PW");
    return "Ramp East"          if ($init eq "PE");
    return "Ramp North"         if ($init eq "PN");
    return "Ramp South"         if ($init eq "PS");

    return "Window Tall"        if ($init eq "WT");
    return "Door Tall Closed"   if ($init eq "DTC");

    return "Tree Short"         if ($init eq "TS");
    return "Tree Tall"          if ($init eq "TT");
    return "Tree Ugly"          if ($init eq "TU");

    die "Don't know block: $init";
}

sub get_height
{
    my $name = shift;

    return 2 if $name =~ /Tall/;
    return 1;
}

sub get_shadows
{
    my $name = shift;
    
    return "False" if $name =~ /Tree/;
    return "True";
}

sub numerically
{
    return $a <=> $b;
}
