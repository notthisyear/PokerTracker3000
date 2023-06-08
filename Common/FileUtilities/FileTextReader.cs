using System;
using System.IO;
using System.Text;

namespace PokerTracker3000.Common.FileUtilities
{
    public class FileTextReader
    {
        ///<value>Property <c>AllText</c> contains all read characters from the provided stream or file path.</value>.
        public string AllText { get; private set; } = string.Empty;

        public bool SuccessfulRead { get; private set; } = false;

        public Exception? ReadException { get; private set; }

        /// <summary>
        /// Open a file in ReadOnly mode, reads all text in the file and stores the content in AllText property.
        /// The properties SuccessfulRead and ReadException provide status information.
        /// <para>The default encoding is UTF-8.</para>
        /// </summary>
        /// <param name="fileName">The name of the file to read, can be a relative or absolute path.</param>
        /// <param name="encoding">The encoding of the source file. The default is UTF-8.</param>
        /// <param name="maxFileSizeBytes">The maximum file size to read (raw file size in bytes).</param>
        public FileTextReader(string fileName, Encoding? encoding = default, long maxFileSizeBytes = 10_000_000)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                ReadException = new ArgumentNullException(nameof(fileName));
                return;
            }

            encoding = encoding == default ? Encoding.UTF8 : encoding;

            try
            {
                FileInfo fileInfo = new(fileName);
                var fileSize = fileInfo.Length;
                if (fileSize > maxFileSizeBytes)
                {
                    var msg = $"FileTextReader(): File size ({fileSize} bytes) is larger " +
                        $"than max size ({maxFileSizeBytes} bytes)";
                    ReadException = new InvalidOperationException(msg);
                    return;
                }

            }
            catch (Exception e)
            {
                ReadException = e;
                return;
            }

            try
            {
                // Note: File.ReadAllText eventually calls the StreamReader constructor,
                // which opens the file in ReadOnly mode
                AllText = File.ReadAllText(fileName, encoding);
                SuccessfulRead = true;
            }
            catch (Exception e)
            {
                ReadException = e;
            }
        }

        /// <summary>
        /// Reads all characters from the provided stream and stores the content in the AllText property.
        /// The properties SuccessfulRead and ReadException provide status information.
        /// <para>The default encoding is UTF-8.</para>
        /// </summary>
        /// <param name="fileStream">The raw stream to read from.</param>
        /// <param name="encoding">The encoding of the source file. The default is UTF-8.</param>
        /// <param name="maxFileSizeBytes">The maximum file size to read (raw file size in bytes).</param>
        public FileTextReader(Stream fileStream, Encoding? encoding = default, long maxFileSizeBytes = 10_000_000)
        {
            try
            {
                var fileSize = fileStream.Length;
                if (fileSize > maxFileSizeBytes)
                {
                    var msg = $"FileTextReader(): File size ({fileSize} bytes) is larger " +
                        $"than max size ({maxFileSizeBytes} bytes)";
                    ReadException = new InvalidOperationException(msg);
                    return;
                }

            }
            catch (Exception e)
            {
                ReadException = e;
                return;
            }

            try
            {
                using StreamReader reader = new(fileStream, encoding == default ? Encoding.UTF8 : encoding);
                AllText = reader.ReadToEnd();
                SuccessfulRead = true;
            }
            catch (Exception e)
            {
                ReadException = e;
            }
        }
    }
}
