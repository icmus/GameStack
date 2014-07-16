# GameStack Overview

GameStack is a toolkit for building games and interactive displays with Mono, 
Xamarin.iOS and Xamarin.Android. GameStack supports Mac, Linux, iOS and Android
build targets and can be used with MonoDevelop/Xamarin Studio or other development
environments. It can be used with Xamarin to publish on mobile app stores, or
with embedded Linux PCs to drive interactive displays.

GameStack pulls in a number of open source libraries and provides recipes to
build those libraries for each target platform. It also includes a framework
that brings all the pieces together into a coherent API, consistent across all
platforms. A sample app demonstrates some GameStack features with a match-3 
style game, using the new shared project type for total code sharing.

When using Xamarin.iOS or Xamarin.Android, GameStack views can be mixed with
native views to integrate advanced visualizations into an app.

GameStack's main features include:

* *Tools for 2D graphics:* Spritesheets, 9-slice, batching, bitmap fonts (bmfont format)
* *Tools for 3D graphics:* Animated 3D models, frame buffer objects for post
  effects, custom materials and custom shaders (sprite shaders and basic Phong
  shader built in)
* *Tools for UI:* Hierarchical UI views, layout helpers, basic UI controls like
  button and toggle, layout lambdas for flexible, elastic layouts
* *Tools for game logic and scene management:* Iterator coroutines, scene
  transitions (simple fade transition built in), event handling
* *Tools for input:* Touch on all devices, gestures on mobile, keyboard/mouse on desktop
* *Tools for audio:* Stream Ogg Opus assets, load sound effects, or control audio
  buffers directly with OpenAL. No limit on the number of audio channels.
* *NEW:* Draw on textures using Cairo's drawing API, with custom font support. Great for 
  drawing widgets and then applying post effects with a shader. This is an optional 
  feature for mobile due to the added size.
 
GameStack comes with MonoDevelop/Xamarin Studio templates for easier project setup.
The DLLs can also be added to an existing project to use GameStack's facilities.

# Run-time Prerequisites (desktop only)

The following components are required for execution of GameStack apps on desktop
platforms:

* Mono 3.4+ [OSX installer](http://www.go-mono.com/mono-downloads/download.html)
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
and GameStack.Android can only be used on Mac OSX, as they depend on Xamarin.iOS
and Xamarin.Android respectively.

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
* Mono 3.4+

Cheat code for Arch:

```
pacman -S git base-devel wget unzip subversion mono cmake
```

### Mac OSX prerequisites

* Xcode 5.1+ with command-line tools installed
* iPhone SDK 7.1+
* Android NDK rd8+
* Xamarin Studio 5+ (Mono 3.4+)

## Building prerequisite libs

This must be completed before building GameStack.

```
make libs
```

You can also enter the lib folder and use the Makefile targets to build
dependencies individually.

If you are on a Mac and the iOS or Android libraries do not build, it
may be that the Makefile couldn't find the SDK(s). You may need to
adjust the following values in lib/Makefile: ANDROID_NDK_VER,
ANDROID_VER, IOS_SDK_VER, and in extreme cases, IOS_ROOT and
NDK_ROOT. By default, the Android NDK installed with Xamarin.Android
will be used.

## Building GameStack libraries

```
make                 # build all targets below
make desktop         # build GameStack.Desktop
make ios             # build GameStack.iOS (needs OSX)
make android         # build GameStack.Android
make import          # build content pipeline and import tool
make addin           # prepare MonoDevelop addin for templates
```
After a build, all library build output will be located in the ./bin/ folder, 
and further divided into sub-folders for each platform. Instead of using make,
you can also open the various .sln files with Xamarin Studio or MonoDevelop.

## Installing

You can optionally install GameStack libraries and project templates. You
can skip this step if you wish to reference the GameStack assemblies
manually.

```
make install         # perform all install actions below
make install-gac     # install assemblies ito gac
make install-libs    # install assemblies into library folder
make install-addin   # install MonoDevelop/XamarinStudio project templates
```
Each install target also has an uninstall target. Some install actions may
require sudo. By default, libraries are installed in $(PREFIX)/lib/GameStack
and pkgconfig files are added to $(PREFIX)/lib/pkgconfig. PREFIX defaults
to /usr/local and may be overridden when running make.

In short, GameStack is installed by default to /usr/local/lib/GameStack.

# Dependencies and acknowledgments

GameStack relies on the following open source libraries:

* [Mono](http://www.mono-project.com/Main_Page)
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
* [pixman](http://www.pixman.org/)
* [freetype](http://www.freetype.org/)
* [cairo](http://cairographics.org/)
