namespace Zipper.Cli;

using System.Buffers;

/// <summary>
/// Provides methods and properties used to compress and decompress files.
/// </summary>
internal class FileZipper : IDisposable
{
    private const int BufferSize = 512 * 1024;
    private static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Create();

    private readonly Stream readFrom;
    private readonly Stream writeTo;
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
        var outputFile = File.Create(outputFilePath);

        if (mode == ZipperMode.Compress)
        {
            readFrom = inputFile;
            writeTo = new ZipperStream(outputFile, ZipperStream.MaxBlockSize, mode);
        }
        else
        {
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
    }

    /// <summary>
    /// Disposes all used files.
    /// </summary>
    public void Dispose()
    {
        BufferPool.Return(buffer);

        readFrom.Dispose();
        writeTo.Dispose();
    }
}
