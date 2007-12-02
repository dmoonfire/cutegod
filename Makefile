BOOGAME_SOURCE = $(HOME)/src/boogame
LIBRARIES_SOURCE = $(HOME)/src/mfgames/Libraries/
UTILITY_SOURCE = $(LIBRARIES_SOURCE)/MfGames.Utility/bin/Debug/
SPRITE3_SOURCE = $(HOME)/src/mfgames/Sprite3/

# Get the assembly version
VERSION=$(shell cat CuteGod/Properties/AssemblyInfo.cs  | grep AssemblyVersion | cut -f 2 -d '"' | cut -f 1-3 -d .)
DATA_VERSION=$(shell cat Assets/version.txt)

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

release:
	# Make some noise
	echo Building version $(VERSION)

	# Compile the code
	mono tools/prebuild.exe /target nant \
		/file prebuild.xml /FRAMEWORK MONO_2_0
	fnant build-release
	cp tools/cutegod CuteGod/bin/Release

	# I hate that .dll's are executable
	find -name "*.dll" -print0 | xargs -0 chmod a-x

	# Create a tarball of the source code
	mv CuteGod/bin/Release cutegod-$(VERSION)
	tar -cjf cutegod-$(VERSION).tar.bz2 cutegod-$(VERSION)
	rm -rf cutegod-$(VERSION)

	# Create the release
	cp ChangeLog cutegod-$(VERSION).changelog

	# Create the data zip
	echo Building data package $(DATA_VERSION)
	rm -rf tmp-data
	mkdir tmp-data
	cp -r Assets tmp-data
	find tmp-data -name .svn -print0 | xargs -0 rm -rf
	find tmp-data -name "*.svg" -print0 | xargs -0 rm -rf
	cd tmp-data && zip -r ../cutegod-data-$(DATA_VERSION).zip Assets
	rm -rf tmp-data

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
