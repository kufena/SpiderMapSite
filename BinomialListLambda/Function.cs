using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;
using System.Transactions;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.VisualBasic.CompilerServices;
using SharedTypes;
using TrieTree;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BinomialListLambda;

public class Functions
{
    private IAmazonS3 S3Client { get; set; }
    private string AraneaeFile { get; set; }
    private string AraneaeBucket { get; set; }
    private Trie<Species>? TrieTree { get; set; }
    private Dictionary<string, List<Species>>? SpeciesDetails { get; set; }
    private bool loaded;

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public Functions()
    {
        S3Client = new AmazonS3Client();
        AraneaeFile = "Araneae.json";
        AraneaeBucket = "thegatehousewereham.home";
        loaded = false;
    }

    public Functions(IAmazonS3 s3Client)
    {
        S3Client = s3Client;
        AraneaeFile = "Araneae.json";
        AraneaeBucket = "thegatehousewereham.home";
        loaded = false;
    }

    public async Task load()
    {
        var objectResponse = await S3Client.GetObjectAsync(new GetObjectRequest() { BucketName = AraneaeBucket, Key = AraneaeFile });
        ReadData(objectResponse.ResponseStream);
        loaded = true;
    }


    /// <summary>
    /// A Lambda function to respond to HTTP Get methods from API Gateway
    /// </summary>
    /// <param name="request"></param>
    /// <returns>The API Gateway response.</returns>
    public async Task<APIGatewayProxyResponse> Get(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation("Get Request\n");

        if (!loaded)
        {
            await load();
        }

        string prefix = request.PathParameters["prefix"];
        int limit = Int32.Parse(request.PathParameters["limit"]);

        var result = TrieTree.ElaborateAfterPrefix(prefix, 0);
        int resultCount = result.Count();

        if (result.Count > limit)
        {
            result = new List<Species>();
        }

        var responseBody = new BodyResponse(resultCount, result);
        var document = JsonSerializer.Serialize(responseBody);
        
        var response = new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = document,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };

        return response;
    }

    private void ReadData(Stream stream)
    {
        //TextReader tr = new StreamReader(stream);
        //string sss = "";
        //string str;
        //while ((str = tr.ReadLine()) != null)
        //{
        //    sss += str;
        //    //Console.WriteLine(str);
        //}
        SpeciesDetails = JsonSerializer.Deserialize<Dictionary<string, List<Species>>>(stream);
        TrieTree = new Trie<Species>();
        foreach (var (k, l) in SpeciesDetails)
        {
            foreach (var s in l)
            {
                TrieTree.Add(s.GenusName + " " + s.SpeciesName, 0, s);
            }
        }

    }
}

record BodyResponse
{
    public int Count { init; get; }
    public List<Species> Details { init; get; }

    public BodyResponse(int Count, List<Species> Details)
    {
        this.Count = Count;
        this.Details = Details;
    }
}