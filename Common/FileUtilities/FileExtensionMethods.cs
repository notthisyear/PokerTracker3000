using System;
using System.IO;

namespace PokerTracker3000.Common.FileUtilities
{
    internal static class FileExtensionMethods
    {
        public static (bool success, string path, Exception? e) SerializeWriteToJsonFile<T>(this T obj, string path, bool addExtension)
        {
            var (s, e) = obj.SerializeToJsonString(convertPascalCaseToSnakeCase: true, indent: true);
            if (e != default)
                return (false, string.Empty, e);

            if (addExtension)
                path = Path.ChangeExtension(path, "json");

            var writer = new FileTextWriter(s!, path);
            if (!writer.SuccessfulWrite)
            {
                if (File.Exists(path))
                    File.Delete(path);
            }

            return (writer.SuccessfulWrite, path, writer.WriteException);
        }
    }
}
