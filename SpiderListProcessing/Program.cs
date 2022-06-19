// See https://aka.ms/new-console-template for more information

using HtmlAgilityPack;
using System.IO;
using System.Text;

Console.WriteLine($"Processing {args[0]}");
Console.WriteLine($"Writing output to {args[1]}");

var tr = File.OpenText(args[0]);

//TextWriter tw = new StringWriter(args[1]);

// Ugh, this is painful
string html = "";
while (tr.ReadLine() is { } s)
{
    html += s;
}

// Well we've got the html, let's create a document.
var x = new HtmlDocument();
x.LoadHtml(html);

List<Uri> links = new List<Uri>();
findLinks(links, x.DocumentNode);


//
// Goes through an HTML document tree to find links and a elements, and
// returns a list of, hopefully unique links.
static void findLinks(List<Uri> links, HtmlNode node)
{
    //if (node.NodeType == HtmlNodeType.Element)
    //    Console.WriteLine(node.Name);
    if (node.NodeType == HtmlNodeType.Element && node.Name == "link")
    {
        var refs = node.Attributes.AttributesWithName("href");
        foreach (var r in refs)
        {
            Console.WriteLine($"We've found {r.Value}");
        }
    }
    if (node.NodeType == HtmlNodeType.Element && node.Name == "a")
    {
        var refs = node.Attributes.AttributesWithName("href");
        foreach (var r in refs)
        {
            Console.WriteLine($"We've found {r.Value}");
        }
    }

    if (node.FirstChild != null)
    {
        findLinks(links, node.FirstChild);
    }

    if (node.NextSibling != null)
        findLinks(links, node.NextSibling);
}
