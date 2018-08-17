using System.Collections.Generic;
using System.IO;

namespace ExcelReaderStandardLibrary
{
    public interface ITableReader
    {
        List<Dictionary<string, string>> Read(Stream inputStream, int worksheetIndex, int headerRowIndex);
    }
}