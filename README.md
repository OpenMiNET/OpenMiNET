# OpenMiNET (Previously OpenAPI)
[![Build status](https://ci.appveyor.com/api/projects/status/rb6xfiogyt6sam30/branch/master?svg=true)](https://ci.appveyor.com/project/kennyvv/openapi/branch/master)
[![nuget](https://img.shields.io/nuget/v/OpenMiNET.OpenAPI.svg)](https://www.nuget.org/packages/OpenMiNET.OpenAPI/)

A Custom Plugin API for MiNET - Making it easier for multiple plugins to work together nicely.

Please feel free to join  our discord if you feel like contributing or need some help getting started.
* [Discord](https://discord.gg/J4ZfR87) - Join our Discord server

### I decided to rename OpenAPI to OpenMINET to avoid confusion.

## Setup & Getting Started

Great to see you are interested in using OpenMiNET.  
The steps required to get a server running using OpenMiNET are probably a little different then what you might expect.  
No worries tho! Just follow the following steps closely and you'll have your server up & running within a few minutes.  

#### Step 1.
Acquiring the required binaries.

OpenMiNET requires a custom MiNETServer instance to work properly, so the default MiNET.Server executables won't work.  
You can either compile OpenAPI.Server yourself or you can download pre-compiled binaries [HERE](https://ci.appveyor.com/project/kennyvv/openapi/branch/master/artifacts)

#### Step 2.
Setting up the server.

You'll find a file named "server.conf", if you open this file in a text editor. You should see the following line:

`PluginDirectory=Plugins`

This property tells OpenMiNET where to look for your installed plugins.  
If you choose not to change the default value, the server will look for a folder called "Plugins" (Without quotes) in the directory containg the server binary. (OpenAPI.Server)  

You can also provide multiple plugin folders by using the `;` character as a delimiter. 

Any other configuration options can be found on [MiNET's WIKI](https://github.com/NiclasOlofsson/MiNET/wiki/Configuration)
(Note: Some of these might not work.)

#### Step 3.
Starting up the server.

You can now start the server like you would with a normal MiNET server.  
Only the executable name will be `OpenAPI.Server` instead of `MiNET.Server`

## Documentation
Basic documentation is available through the following link: [View Documentation](https://openminet.github.io/OpenAPI/api/OpenAPI.html)

More documentation is coming soon.

Interesting projects to check out
----------------------------------
* [MiNET](https://github.com/NiclasOlofsson/MiNET) - The server software making all of this possible.
* [Alex](https://github.com/kennyvv/Alex) - A Minecraft Client written in C# aiming to be compatible with the Bedrock and Java edition.
