// See https://aka.ms/new-console-template for more information

using System.Formats.Asn1;
using System.Text;
using ExcelDataReader;
using ExcelDataReader.Core;
using System.Text.Json;
using SharedTypes;

Console.WriteLine($"Reading in from {args[0]}!");

using (var stream = File.Open(args[0], FileMode.Open, FileAccess.Read))
{
    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

    ExcelReaderConfiguration configuration = new ExcelReaderConfiguration();

    //configuration.FallbackEncoding = Encoding.GetEncoding(1252);
    
    using (var reader = ExcelReaderFactory.CreateReader(stream, configuration))
    {
        reader.Read(); // skip the header.
        string family = "";
        int count = 0;
        string tag;
        bool reachedLists = false;

        Dictionary<string, List<Species>> familiesToSpecies = new Dictionary<string, List<Species>>();
        
        do
        {
            while (reader.Read())
            {
                tag = reader.GetString(0);
                if (tag.StartsWith("List"))
                {
                    reachedLists = true;
                    break;
                }

                if (tag.StartsWith("Family"))
                {
                    family = tag.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1];
                    familiesToSpecies.Add(family, new List<Species>());
                }
                else
                {
                    count++;
                    var splits = tag.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    string genus = splits[0];
                    string species = splits[1];
                    string comment = "";
                    for(int i = 2; i < splits.Length; i++) comment += splits[i] + " ";

                    var sp = new Species(genus, species, comment);
                    familiesToSpecies[family].Add(sp);
                }
            }

            if (reachedLists)
                break;

        } while (reader.NextResult());

        using (var ostream = File.OpenWrite(args[1]))
        {
            var sw = new StreamWriter(ostream);
            JsonSerializer.Serialize(ostream, familiesToSpecies);
            /*
            foreach (var (k, v) in familiesToSpecies)
            {
                foreach (var sp in v)
                {
                    sw.WriteLine($"{k},{sp.GenusName},{sp.SpeciesName},{sp.Comment}");
                }
            }
            */
        }
    }
}

record Family
{
    public Family(string n, string c)
    {
        Name = n;
        Comment = c;
    }

    private string Name { init; get; }
    private string Comment { init; get; }
}