# Command-Line Path Manager
Do you hate GUIs but still find yourself using Windows for some reason? Do you get annoyed when you have to find where the little menu for modifying the User Path is again? Well, I've been told you can do this in Powershell with `setx` but I couldn't find an easy way. I'm probably a fool and it exists.

But what if, instead, you wanted to use some random tool you found that's slightly more convenient? Luckily for you, I wanted a taste of Microsoft Java and spent a few hours making a CLI for doing just what you need.

I also got this program to work on Linux, but it has limited capabilities due to the way $PATH works. Were I to aim for feature parity, I'd have to parse your `.bashrc` or what-have-you and find all the points at which a directory is added to your path. This is a pain, so instead `patma` just prints out the output path with the changes you specified.

This is fine, as you're not supposed to change $PATH on the regular anyway, but it makes `patma` rather useless on Linux; it's basically a glorified script to split your path components into separate lines, something you can easily do:
```sh
echo $PATH | sed 's/:/\n/g'
```
So it's not worth the meaty half gig of disk space required for the .NET SDK. But you can install it, if you want.

## Installation (Build from source)
You need [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) and the `dotnet` command. Easiest way to set up is by intializing a .NET console app project:
```ps1
mkdir patma
cd patma
dotnet new console
```
Then cloning the repo into a temporary directory:
```ps1
git clone https://github.com/RayOfSunDull/patma patma.tmp
```
And copying the only real source file:
```ps1
cp patma.tmp/Project.cs .
rm -rf patma.tmp
```
Now we can build the project:
```ps1
dotnet build --configuration Release
```
On Windows, we can use the program to add itself to the User Path like so:
```ps1
.\bin\Release\net8.0\patma.exe add .\bin\Release\net8.0
```
On Linux, we can copy the program to a standard location such as `$HOME/bin` or `$HOME/.local/bin`, whichever one you prefer:
```ps1
install ./bin/Release/net8.0/patma ~/bin
```

## Usage
List the directories in the Path:
```ps1
patma list
```
Add a directory to the Path:
```ps1
patma add dir_name
```
Remove an entry in the Path by index:
```ps1
patma pop idx
```
Get a directory from the Path by index:
```ps1
patma get idx
```
Set the value of an entry in the Path:
```ps1
patma set idx value
```

Use `patma help` if you need any reminders. Or just look at the miniscule amount of code this project consists of, either is fine.
