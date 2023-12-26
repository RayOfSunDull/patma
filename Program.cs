using Microsoft.Win32;
using System.Text;
using System.IO;
using System;


public class PathObj {
    public int Size;
    public List<string> Elements;
    string Delimiter;

    public PathObj(string pathString, string delimiter) {
        Size = pathString.Length;
        Elements = pathString.Split(delimiter).ToList();
        Delimiter = delimiter;
    }

    public int NumElements() {
        return Elements.Count;
    }

    public string Get(int idx) {
        return Elements[idx];
    }

    public void Set(int idx, string element) {
        Elements[idx] = element;
    }

    public void List() {
        for (int i = 0; i < Elements.Count; i++) {
            Console.WriteLine($"{$"{i}.",-5} {Elements[i]}");
        }
    }

    public void Add(string element) {
        Elements.Add(element);
        Size += element.Length;
    }

    public void Pop(int idx) {
        Elements.RemoveAt(idx);
    }

    public override string ToString() {
        var pathBuilder = new StringBuilder(Elements[0], Size);

        for (int i = 1; i < Elements.Count; i++) {
            pathBuilder.Append(Delimiter);
            pathBuilder.Append(Elements[i]);
        }

        return pathBuilder.ToString();
    }

    public void RemoveDuplicates() {
        var result = new List<string>();

        for (int i = 0; i < Elements.Count; i++) {
            string element = Elements[i];
            if (result.Contains(element)) {
                continue;
            }
            result.Add(element);
        }

        Elements = result;
    }
}

public interface IPlatform {
    PathObj? GetPath();

    void SetPath(PathObj path);
}

public class WindowsPlatform : IPlatform {
    PathObj? IPlatform.GetPath() {
        string? pathString = (string?) Registry.GetValue(
            "HKEY_CURRENT_USER\\Environment", "Path", null
        );
        if (pathString == null) {
            return null;
        }

        return new PathObj(pathString, ";");
    }

    void IPlatform.SetPath(PathObj path) {
        path.RemoveDuplicates();
        Registry.SetValue(
            "HKEY_CURRENT_USER\\Environment", "Path", path.ToString()
        );
    }
}

public class UnixPlatform : IPlatform {
    PathObj? IPlatform.GetPath() {
        string? pathString = Environment.GetEnvironmentVariable("PATH");
        if (pathString == null) {
            return null;
        }

        return new PathObj(pathString, ":");
    }

    void IPlatform.SetPath(PathObj path) {
        path.RemoveDuplicates();
        // Since setting the PATH permanently is a pain on Unix, we just print it
        Console.WriteLine(path.ToString());
    }
}

class MainClass {
    static void Main(string[] args) {
        if (args.Length < 1) {
            Console.WriteLine("Error: No command specified!");
            Console.WriteLine("Use patma help for a list of commands");
            return;
        }
        IPlatform? platform = null;

        switch (Environment.OSVersion.Platform) {
        case PlatformID.Unix:
            platform = new UnixPlatform();
            break;
        case PlatformID.Win32NT:
            platform = new WindowsPlatform();
            break;
        default:
            Console.WriteLine($"Error: Platform {Environment.OSVersion.Platform} not supported.");
            break;
        }

        PathObj? path = platform.GetPath();
        if (path == null) {
            Console.WriteLine("Error: Unable to find the Path.");
            return;
        }

        string command = args[0];

        if (command == "help") {
            PrintHelp();
        }
        else if (command == "list") {
            path.List();
        }
        else if (command == "add") {
            if (args.Length < 2) {
                Console.WriteLine("Error: No path specified!");
                Console.WriteLine("Correct syntax is patma add [path]");
                return;
            }

            string newElement = args[1];
            if (!Directory.Exists(newElement)) {
                Console.WriteLine($"Error: Path {newElement} does not exist!");
                return;
            }

            path.Add(Path.GetFullPath(newElement));

            platform.SetPath(path);
        }
        else if (command == "pop") {
            if (args.Length < 2) {
                Console.WriteLine("Error: No path element index specified!");
                Console.WriteLine("Correct syntax is patma pop [index]");
                return;
            }

            int idx = ParseIdx(args[1], path.NumElements());
            if (idx < 0) {
                return;
            }

            path.Pop(idx);

            platform.SetPath(path);
        }
        else if (command == "get") {
            if (args.Length < 2) {
                Console.WriteLine("Error: No path element index specified!");
                Console.WriteLine("Correct syntax is patma get [index]");
                return;
            }

            int idx = ParseIdx(args[1], path.NumElements());
            if (idx < 0) {
                return;
            }

            Console.WriteLine(path.Get(idx));
        }
        else if (command == "set") {
            if (args.Length < 2) {
                Console.WriteLine("Error: No path element index specified!");
                Console.WriteLine("Correct syntax is patma set [index] [value]");
                return;
            }
            if (args.Length < 3) {
                Console.WriteLine("Error: No element to set specified!");
                Console.WriteLine("Correct syntax is patma set [index] [value]");
                return;
            }

            int idx = ParseIdx(args[1], path.NumElements());
            if (idx < 0) {
                return;
            }

            string newElement = args[2];
            if (!Directory.Exists(newElement)) {
                Console.WriteLine($"Error: PathObj {newElement} does not exist!");
                return;
            }

            path.Set(idx, Path.GetFullPath(newElement));

            platform.SetPath(path);
        }
    }

    static int ParseIdx(string idxString, int upper) {
        try {
            int idx = Int32.Parse(idxString);
            if (idx < 0 || idx > upper) {
                Console.WriteLine($"Error: Index {idx} is out of range!");
                return -1;
            }
            return idx;
        }
        catch (FormatException) {
            Console.WriteLine($"Error: Expected integer, got {idxString}!");
            return -1;
        }
    }


    static void PrintHelp() {
        Console.WriteLine("patma: A simple command line manager for your Path environment variable.");
        Console.WriteLine("\"Path\" refers to either $PATH on Linux or the User Path on Windows.");
        Console.WriteLine("syntax: patma [command] [arguments]\ncommands:");

        Console.WriteLine("    patma help");
        Console.WriteLine("        Prints the help message.");

        Console.WriteLine("    patma add [value]");
        Console.WriteLine("        Adds the specified directory to the end of the Path");

        Console.WriteLine("    patma list");
        Console.WriteLine("        Lists all directories that are part of the Path.");

        Console.WriteLine("    patma pop [index]");
        Console.WriteLine("        Removes the specified directory from the Path.");

        Console.WriteLine("    patma get [index]");
        Console.WriteLine("        Prints the specified directory.");

        Console.WriteLine("    patma set [index] [value]");
        Console.WriteLine("        Sets the specified entry to the given value.");
    }
}

