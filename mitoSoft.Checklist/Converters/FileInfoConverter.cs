using System.IO;

namespace mitoSoft.Checklist.Converters;

public static class FileInfoConverter
{
    public static FileInfo ToFileInfo(this string fileName)
    {
        return new FileInfo(fileName);
    }
}