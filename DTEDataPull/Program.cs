// See https://aka.ms/new-console-template for more information


using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

//Console.WriteLine("Hello, World!");

//string url = "https://usagedata.dteenergy.com/link/DA1EBC06-AB43-4C4C-A301-723B228F994C";

string url = args[0];
string serviceAddress = args[1].Replace("\"'",string.Empty);
string accountNumber = args[2];
string buildingName = args[3];



Task getTask = getData(url, serviceAddress, accountNumber, buildingName);

getTask.Wait();

 static async Task getData(string url, string serviceAddress,string accountNumber,string buildingName)
{
    using (CsvFileWriter gwriter = new CsvFileWriter(@"\\city.a2\shared\AppDev\Production\data\OSI\DTEBillProcessing\dtedata.csv"))
    {
        using (var client = new HttpClient())
        {
            string line = "";
            int entries = 0;
            XmlSerializer serializerResp = new XmlSerializer(typeof(feed));
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var xml = await response.Content.ReadAsStringAsync();
                StringReader doc = new StringReader(xml);

                feed respData = (feed)(serializerResp.Deserialize(doc));

                foreach (feedEntry entry in respData.entry)
                {
                    if (entry.content[0].IntervalBlock is not null)
                       foreach (IntervalBlock IReading in entry.content[0].IntervalBlock)
                        {
                            foreach (IntervalBlockIntervalReading bReading in IReading.IntervalReading)
                            {
                             line = serviceAddress + "," + accountNumber + "," + buildingName + "," + 
                             DateTimeOffset.FromUnixTimeSeconds(long.Parse(bReading.timePeriod[0].start)).ToString().Replace("+00:00","") + "," +
                             DateTimeOffset.FromUnixTimeSeconds(long.Parse(bReading.timePeriod[0].start) + long.Parse(bReading.timePeriod[0].duration)).ToString().Replace("+00:00", "") + "," +
                             (float.Parse(bReading.value) / 1000).ToString() ;
                                gwriter.WriteLine(line);
                            }

                        }

                  //  Console.WriteLine("test");
                }


            }


        }

    }

}
public class CsvRow : List<string>
{
    public string LineText { get; set; }
}
public class CsvFileWriter : StreamWriter
{
    public CsvFileWriter(Stream stream)
        : base(stream)
    {
    }

    public CsvFileWriter(string filename)
        : base(filename)
    {
    }

    /// <summary>
    /// Writes a single row to a CSV file.
    /// </summary>
    /// <param name="row">The row to be written</param>
    public void WriteRow(CsvRow row)
    {
        StringBuilder builder = new StringBuilder();
        bool firstColumn = true;
        foreach (string value in row)
        {
            // Add separator if this isn't the first value
            if (!firstColumn)
                builder.Append(',');
            // Implement special handling for values that contain comma or quote
            // Enclose in quotes and double up any double quotes
            if (value.IndexOfAny(new char[] { '"', ',' }) != -1)
                builder.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
            else
                builder.Append(value);
            firstColumn = false;
        }
        row.LineText = builder.ToString();
        WriteLine(row.LineText);
    }
}

