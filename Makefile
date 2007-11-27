BOOGAME_SOURCE = $(HOME)/src/boogame
LIBRARIES_SOURCE = $(HOME)/src/mfgames/Libraries/
UTILITY_SOURCE = $(LIBRARIES_SOURCE)/MfGames.Utility/bin/Debug/

all: compile

compile:
	# Compile the code
	mono tools/prebuild.exe /target nant \
		/file prebuild.xml /FRAMEWORK MONO_2_0
	fnant build-debug

#CuteGod/layouts.xml
update:
	tools/create-sound-control.pl CuteGod
	tools/create-credits.pl .
	mv credits.xml CuteGod

clean:
	find -name "*~" -o -name semantic.cache | xargs rm -f
	find -name bin | xargs rm -rf

CuteGod/layouts.xml: CuteGod/layouts.txt tools/cutegod-layouts.pl
	tools/cutegod-layouts.pl CuteGod/layouts.txt > CuteGod/layouts.xml
