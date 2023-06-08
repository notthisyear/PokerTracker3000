using System;
using System.IO;
using System.Text;

namespace PokerTracker3000.Common
{
    public class FileTextWriter
    {
        ///<value>Property <c>SuccessfulWrite</c> stores whether or not the write was successful.</value>.
        public bool SuccessfulWrite { get; private set; } = false;

        ///<value>Property <c>WriteException</c> saves the write exception (if there was any).</value>.
        public Exception? WriteException { get; private set; }

        /// <summary>
        /// Writes all text in a string to a file. If the file does not exists, it will be created. If it does exist, it will be overwritten.
        /// The properties SuccessfulWrite and WriteException provide status information.
        /// <para>The default encoding is UTF-8.</para>
        /// </summary>
        /// <param name="dataToWrite">The string that is to be written to file.</param>
        /// <param name="fileName">The name of the file to write to, can be a relative or absolute path.</param>
        /// <param name="encoding">The encoding of the newly created file. The default is UTF-8.</param>
        public FileTextWriter(string dataToWrite, string fileName, Encoding? encoding = default)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                WriteException = new ArgumentNullException(nameof(fileName));
                return;
            }

            SuccessfulWrite = true;
            if (string.IsNullOrEmpty(dataToWrite))
                return;

            encoding = (encoding == default) ? Encoding.UTF8 : encoding;

            try
            {
                File.WriteAllText(fileName, dataToWrite, encoding);
            }
            catch (Exception e)
            {
                WriteException = e;
                SuccessfulWrite = false;
            }
        }
    }
}
