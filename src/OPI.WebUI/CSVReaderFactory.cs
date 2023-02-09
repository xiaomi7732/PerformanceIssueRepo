using NReco.Csv;

namespace OPI.WebUI;

public class CSVReaderFactory
{
    public CsvReader CreateCSVReader(StreamReader streamReader)
    {
        return new CsvReader(streamReader);
    }
}