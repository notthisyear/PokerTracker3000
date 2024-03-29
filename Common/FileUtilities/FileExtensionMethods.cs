using System;
using System.IO;

namespace PokerTracker3000.Common.FileUtilities
{
    internal static class FileExtensionMethods
    {
        public static (bool success, string path, Exception? e) SerializeWriteToJsonFile<T>(this T obj, string path)
        {
            var (s, e) = obj.SerializeToJsonString(convertPascalCaseToSnakeCase: true, indent: true);
            if (e != default)
                return (false, string.Empty, e);

            var fullPath = Path.ChangeExtension(path, "json");
            var writer = new FileTextWriter(s!, fullPath);
            return (writer.SuccessfulWrite, fullPath, writer.WriteException);
        }
    }
}
