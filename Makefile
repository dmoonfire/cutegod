BOOGAME_SOURCE = $(HOME)/src/boogame
LIBRARIES_SOURCE = $(HOME)/src/mfgames/Libraries/
UTILITY_SOURCE = $(LIBRARIES_SOURCE)/MfGames.Utility/bin/Debug/

all: refresh compile

compile: CuteGod/layouts.xml
	# Compile the code
	mono tools/prebuild.exe /target nant \
		/file prebuild.xml /FRAMEWORK MONO_2_0
	fnant build-debug

	# Copy the assets
	rsync -rC -f '- .svn' Assets/ CuteGod/bin/Debug/
	rsync -rC -f '- .svn' Assets/ BooGameDemos/bin/Debug/

update:
	tools/create-sound-control.pl CuteGod
	tools/create-credits.pl .
	mv credits.xml CuteGod

clean:
	find -name "*~" -o -name semantic.cache | xargs rm -f
	find -name bin | xargs rm -rf

refresh:
	cp $(BOOGAME_SOURCE)/src/Tao.FreeType/bin/Release/Tao.FreeType.dll lib
	cp $(BOOGAME_SOURCE)/src/BooGame/bin/Release/BooGame.*dll lib
	cp $(BOOGAME_SOURCE)/src/BooGame.Sdl/bin/Release/BooGame.*dll lib
	cp $(BOOGAME_SOURCE)/src/BooGame.FreeGlut/bin/Release/BooGame.*dll lib
	cp $(BOOGAME_SOURCE)/lib/mono-2.0/* lib/mono-2.0/
	cp $(BOOGAME_SOURCE)/lib/win32deps/* lib/win32deps/
	cp $(BOOGAME_SOURCE)/lib/net-2.0/* lib/net-2.0/
	cp $(UTILITY_SOURCE)/MfGames.Utility.dll lib/

CuteGod/layouts.xml: CuteGod/layouts.txt tools/cutegod-layouts.pl
	tools/cutegod-layouts.pl CuteGod/layouts.txt > CuteGod/layouts.xml
