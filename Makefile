BOOGAME_SOURCE = $(HOME)/src/boogame
LIBRARIES_SOURCE = $(HOME)/src/mfgames/Libraries/
UTILITY_SOURCE = $(LIBRARIES_SOURCE)/MfGames.Utility/bin/Debug/
SPRITE3_SOURCE = $(HOME)/src/mfgames/Sprite3/

all: refresh compile

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

refresh:
	cp $(BOOGAME_SOURCE)/src/Tao.FreeType/bin/Debug/Tao.FreeType.dll lib
	cp $(BOOGAME_SOURCE)/src/BooGame/bin/Debug/BooGame.*dll lib
	cp $(BOOGAME_SOURCE)/src/BooGame.Sdl/bin/Debug/BooGame.*dll lib
	cp $(BOOGAME_SOURCE)/src/BooGame.FreeGlut/bin/Debug/BooGame.*dll lib
	cp $(BOOGAME_SOURCE)/lib/mono-2.0/* lib/mono-2.0/
	cp $(BOOGAME_SOURCE)/lib/win32deps/* lib/win32deps/
	cp $(BOOGAME_SOURCE)/lib/net-2.0/* lib/net-2.0/
	cp $(UTILITY_SOURCE)/MfGames.Utility.dll lib/
