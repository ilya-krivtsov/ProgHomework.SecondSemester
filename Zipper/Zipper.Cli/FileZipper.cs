namespace Zipper.Cli;

using System.Buffers;
using System.Diagnostics;

/// <summary>
/// Provides methods and properties used to compress and decompress files.
/// </summary>
internal class FileZipper : IDisposable
{
    private const int BufferSize = 512 * 1024;
    private static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Create();

    private readonly string? outputFileName;
    private readonly string? outputFileNameTempA;
    private readonly string? outputFileNameTempB;

    private readonly Stream readFrom;
    private readonly Stream writeTo;
    private readonly Stream? writeToAlt;
    private readonly long inputFileSize;
    private readonly byte[] buffer;

    private long bytesReadFromInput;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileZipper"/> class.
    /// </summary>
    /// <param name="mode">Mode to use.</param>
    /// <param name="inputFilePath">File to read data from.</param>
    /// <param name="outputFilePath">File to write compressed/decompressed data to.</param>
    public FileZipper(ZipperMode mode, string inputFilePath, string outputFilePath)
    {
        inputFileSize = new FileInfo(inputFilePath).Length;

        var inputFile = File.OpenRead(inputFilePath);

        if (mode == ZipperMode.Compress)
        {
            outputFileName = outputFilePath;
            outputFileNameTempA = Path.GetTempFileName();
            outputFileNameTempB = Path.GetTempFileName();

            var outputFileA = File.Create(outputFileNameTempA);
            var outputFileB = File.Create(outputFileNameTempB);

            readFrom = inputFile;
            writeTo = new ZipperStream(outputFileA, ZipperStream.MaxBlockSize, mode);
            writeToAlt = new ZipperStream(outputFileB, ZipperStream.MaxBlockSize, mode, useBwt: true);
        }
        else
        {
            var outputFile = File.Create(outputFilePath);
            readFrom = new ZipperStream(inputFile, ZipperStream.MaxBlockSize, mode);
            writeTo = outputFile;
        }

        bytesReadFromInput = 0;
        buffer = BufferPool.Rent(BufferSize);

        EndOfFile = false;
    }

    /// <summary>
    /// Gets progress as value between 0 and 1.
    /// </summary>
    public float Progress => (float)bytesReadFromInput / inputFileSize;

    /// <summary>
    /// Gets a value indicating whether end of file was reached.
    /// </summary>
    public bool EndOfFile { get; private set; }

    /// <summary>
    /// Compresses or decompresses part of input file.
    /// </summary>
    public void ReadAndWriteSingleBuffer()
    {
        int bytesRead = readFrom.Read(buffer, 0, BufferSize);

        if (bytesRead == 0)
        {
            EndOfFile = true;
            return;
        }

        bytesReadFromInput += bytesRead;

        writeTo.Write(buffer, 0, bytesRead);
        writeToAlt?.Write(buffer, 0, bytesRead);
    }

    /// <summary>
    /// Disposes all used files.
    /// </summary>
    public void Dispose()
    {
        BufferPool.Return(buffer);

        readFrom.Dispose();
        writeTo.Dispose();
        writeToAlt?.Dispose();

        if (outputFileName != null)
        {
            Debug.Assert(outputFileNameTempA != null, $"{nameof(outputFileNameTempA)} is null");
            Debug.Assert(outputFileNameTempB != null, $"{nameof(outputFileNameTempB)} is null");

            var tempLengthA = new FileInfo(outputFileNameTempA).Length;
            var tempLengthB = new FileInfo(outputFileNameTempB).Length;

            if (tempLengthA < tempLengthB)
            {
                File.Move(outputFileNameTempA, outputFileName, true);
            }
            else
            {
                File.Move(outputFileNameTempB, outputFileName, true);
            }
        }
    }
}
