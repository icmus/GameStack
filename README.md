# GameStack Overview

GameStack is a multi-platform library for building interactive media and gaming
applications for iOS, Android, Mac, and Linux. It is as much a conglomeration of
other open source libraries as it is original tools and resources. GameStack
brings many technologies together that would otherwise have to be obtained
separately and integrated.

GameStack's primary focus is on the developer. It uses a code-first approach and
is aimed at maximum flexibility and control for the app developer, rather than
forcing developers to work within a rigid structure dictated by design tools.

The primary dependency for mobile development is Xamarin Studio. GameStack views
can be mixed with native mobile controls using Xamarin's bindings. In this way,
GameStack can be used to augment mobile apps, but developers retain control of
the main loop.

GameStack can be used on Mac and Linux without Xamarin Studio, but we recommend
MonoDevelop for most scenarios.

# Run-time Prerequisites

The following components are required for execution of GameStack apps on desktop
platforms:

* Mono 3.2+ [OSX installer](http://www.go-mono.com/mono-downloads/download.html)
* SDL 2 [OSX installer](http://www.libsdl.org/download-2.0.php)
* OpenAL (provided by OSX; needs package install on Linux)

Machines with Xamarin Studio or MonoDevelop installed should already have a
suitable version of Mono.

In Arch Linux, simply use:

```
pacman -S mono sdl2 openal
```

You also need to have installed video/sound drivers and an appropriate display
server (e.g., Xorg). See your distribution's documentation for additional info.

# Building GameStack

GameStack.Desktop can be built on either Linux or Mac OSX, but GameStack.iOS
requires OSX.

## Prerequisites

After cloning, GameStack can download and build all of its dependencies,
provided that all of the prerequisites are present on your system.  If any of
the builds fail, it is likely that something is missing.  When building on Mac
OSX, we recommend installing common prerequisites via
[Homebrew](http://brew.sh/). For Linux, these packages should all be available
via your distribution's package manager.

Most of these prerequisites are required to build GameStack's dependencies
rather than GameStack itself. You do not need to install them all on machines
that are merely going to run GameStack-based apps. See below for the list of
run-time requirements.

### Common prerequisites (all platforms)

* git
* svn (Subversion)
* wget
* unzip
* cmake

### Linux prerequisites

* Base dev tools: gcc, g++, make, autoconf, patch
* Mono 3.2+

Cheat code for Arch:

```
pacman -S git base-devel wget unzip subversion mono cmake
```

### Mac OSX prerequisites

* Xcode 5.1+ with command-line tools installed
* iPhone SDK 7.1+
* Xamarin 3+ (Mono 3.2+)

## Building prerequisites

```
make libs
```

You can also enter the lib folder and use the Makefile targets to build
dependencies individually.

## Building GameStack libraries

```
make desktop         # build GameStack.Desktop
make ios             # build GameStack.iOS (needs OSX)
make android         # build GameStack.Android
make samples         # build sample app
make install         # perform all below install actions
make install-gac     # install assemblies ito gac
make install-libs    # install assemblies into library folder
make install-addin   # install MonoDevelop/XamarinStudio project templates
```
Each install target also has an uninstall target. Some install actions may
require sudo. By default, libraries are installed in $(PREFIX)/lib/GameStack
and pkgconfig files are added to $(PREFIX)/lib/pkgconfig. PREFIX defaults
to /usr/local and may be overridden when running make.

After a build, all library build output will be located in the ./bin/ folder, 
and further divided into sub-folders for each platform.

# Dependencies and acknowledgments

GameStack relies on the following open source libraries:

* [Mono 3+](http://www.mono-project.com/Main_Page)
* [SDL2](http://www.libsdl.org/)
* [SDL2#](https://github.com/flibitijibibo/SDL2-CS)
* [OpenTK](http://www.opentk.com/)
* [OpenALSoft](http://kcat.strangesoft.net/openal.html)
* [Assimp Open Asset Import Library](http://assimp.sourceforge.net/)
* [liblodepng](http://lodev.org/lodepng/)
* [Json.NET](http://james.newtonking.com/json)
* [tar-cs](https://code.google.com/p/tar-cs/)
* [Autosprite](https://github.com/ricmrodrigues/autosprite)
* [libogg](http://www.xiph.org/)
* [libopus and libopusfile](http://www.opus-codec.org/)
