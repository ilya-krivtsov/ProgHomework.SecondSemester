using System.Diagnostics;
using Zipper;
using Zipper.Cli;

const string helpMessage =
"""
Zipper - console tool for compressing and decompressing files

Usage: dotnet run -- <file> [options]
Options:
    -h -? --help    | Print this help message
    ------------------------------------------------
    -c --compress   | Compress specified file
    ------------------------------------------------
    -u --uncompress | Decompress
    -d --decompress | specified file
    ------------------------------------------------
    -f --force      | Overwrite files without asking

File path should be the first argument (unless --help specified)
Options can be specified in any order
Only either '--compress' or '--decompress' can be used at the same time
""";

args = [.. args.Select(x => x.Trim())];

if (args.Length == 0 || (args.Length == 1 && args[0] is "-h" or "--help" or "-?"))
{
    Console.WriteLine(helpMessage);

    return 0;
}

string filePath = args[0];
bool force = false;
ZipperMode? mode = null;

foreach (var arg in args.Skip(1))
{
    switch (arg)
    {
        case "-u" or "-d" or "--uncompress" or "--decompress":
            if (mode != null)
            {
                Console.Error.WriteLine("Error: '--compress' or '--decompress' option can only be specified once");
                return 1;
            }

            mode = ZipperMode.Decompress;
            break;

        case "-c" or "--compress":
            if (mode != null)
            {
                Console.Error.WriteLine("Error: '--compress' or '--decompress' option can only be specified once");
                return 1;
            }

            mode = ZipperMode.Compress;
            break;

        case "-f" or "--force":
            force = true;
            break;

        default:
            Console.Error.WriteLine("Error: unknown argument");
            return 1;
    }
}

if (mode == null)
{
    Console.Error.WriteLine("Error: neither '--compress' nor '--decompress' were specified");
    return 1;
}

if (!File.Exists(filePath))
{
    Console.Error.WriteLine($"Error: file '{filePath}' does not exist");
    return 1;
}

const string zippedExtension = ".zipped";
string? newFilePath = null;
if (mode == ZipperMode.Compress)
{
    newFilePath = $"{filePath}{zippedExtension}";
}
else
{
    if (!filePath.EndsWith(zippedExtension))
    {
        Console.Error.WriteLine($"Error: extension of the specified file is not {zippedExtension}");
        return 1;
    }

    newFilePath = filePath[..^zippedExtension.Length];
}

if (!force && File.Exists(newFilePath))
{
    Console.Write($"File '{newFilePath}' already exists, overwrite? (y/n): ");
    if (Console.ReadLine()?.Trim() != "y")
    {
        Console.WriteLine("Cancelled");
        return 0;
    }
}

const string hideCursorEscape = "\e[?25l";
const string showCursorEscape = "\e[?25h";
const string moveToLeftEscape = "\e[0G";
const string clearLineEscape = "\e[2K";
const string waitingSymblols = @"|/-\";

Console.Write(hideCursorEscape);

using (var fileZipper = new FileZipper(mode.Value, filePath, newFilePath))
{
    var stopwatch = Stopwatch.StartNew();
    var lastLoggedTime = stopwatch.Elapsed;
    int step = 0;
    while (!fileZipper.EndOfFile)
    {
        fileZipper.ReadAndWriteSingleBuffer();

        if (stopwatch.Elapsed - lastLoggedTime > TimeSpan.FromMilliseconds(4))
        {
            Console.Write(moveToLeftEscape);
            RenderProgress(fileZipper.Progress, stopwatch.Elapsed, step, waitingSymblols);
            lastLoggedTime = stopwatch.Elapsed;
        }

        step += 1;
    }
}

Console.Write(clearLineEscape);
Console.Write(moveToLeftEscape);
Console.Write(showCursorEscape);

if (mode == ZipperMode.Compress)
{
    var inputFileSize = new FileInfo(filePath).Length;
    var outputFileSize = new FileInfo(newFilePath).Length;
    var compressionRate = (float)inputFileSize / outputFileSize;

    Console.WriteLine($"Compression rate: {compressionRate}");
}

return 0;

static void RenderProgress(float progress, TimeSpan time, int step, string stepString)
{
    Console.Write($" {stepString[step % stepString.Length]} ");
    Console.Write("[");

    for (int i = 0; i <= 100; i++)
    {
        Console.Write(progress >= i / 100f ? '=' : ' ');
    }

    Console.Write("]");
    Console.Write($" {progress * 100,5:0.0} %");

    if (time.TotalMinutes < 1)
    {
        Console.Write($" {time.Seconds} s");
    }
    else if (time.TotalHours < 1)
    {
        Console.Write($" {time.Minutes} m {time.Seconds:00} s");
    }
    else
    {
        Console.Write($" {time.Hours} h {time.Minutes:00} m {time.Seconds:00} s");
    }
}
