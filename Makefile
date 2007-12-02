BOOGAME_SOURCE = $(HOME)/src/boogame
LIBRARIES_SOURCE = $(HOME)/src/mfgames/Libraries/
UTILITY_SOURCE = $(LIBRARIES_SOURCE)/MfGames.Utility/bin/Debug/
SPRITE3_SOURCE = $(HOME)/src/mfgames/Sprite3/

all: compile

compile: Resources/layouts.xml
	# Compile the code
	mono tools/prebuild.exe /target nant \
		/file prebuild.xml /FRAMEWORK MONO_2_0
	fnant build-debug

	# I hate that .dll's are executable
	find -name "*.dll" -print0 | xargs -0 chmod a-x

	# Copy the assets directory into the proper place
	# BUG: Tao.PhysFs doesn't seem to scan directories property
	if [ -d CuteGod/bin/Debug ]; then \
		rsync -a -f "- .svn" -f "- *.svg" \
			Assets/ CuteGod/bin/Debug/Assets/; \
	fi

#CuteGod/layouts.xml
update:
	tools/create-sound-control.pl CuteGod
	tools/create-credits.pl .
	mv credits.xml CuteGod

clean:
	find -name "*~" -o -name semantic.cache | xargs rm -f
	find -name bin | xargs rm -rf

CuteGod/layouts.xml: CuteGod/layouts.txt tools/cutegod-layouts.pl
	tools/cutegod-layouts.pl Resources/layouts.txt > Resources/layouts.xml
