using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Amazon.SQS;
using Amazon.SQS.Model;

using System.Text.Json;
using SharedTypes;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SpeciesRecordLambda;

public class Functions
{
    private AmazonDynamoDBClient dbClient;
    private AmazonSimpleSystemsManagementClient ssmClient;
    private AmazonSQSClient amazonSQSClient;

    private string TableName = "SpiderMapsDynamo";
    private string QueueURL;

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public Functions()
    {
        dbClient = new AmazonDynamoDBClient();
        ssmClient = new AmazonSimpleSystemsManagementClient();
        amazonSQSClient = new AmazonSQSClient();

        var stageName = Environment.GetEnvironmentVariable("Stage");
        var queue = Environment.GetEnvironmentVariable("SQSURL");
        if (queue == null) throw new Exception("Can't find SQSURL Environment value");
        QueueURL = queue;

        Console.WriteLine($"{stageName} - {queue}");
    }


    /// <summary>
    /// A Lambda function to respond to HTTP Get methods from API Gateway
    /// </summary>
    /// <param name="request"></param>
    /// <returns>The API Gateway response.</returns>
    public async Task<APIGatewayProxyResponse> Get(APIGatewayProxyRequest request, ILambdaContext context)
    {
        string guid;
        APIGatewayProxyResponse response;

        if (!request.PathParameters.ContainsKey("guid") || (guid = request.PathParameters["guid"]) == null)
        {
            response = new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.NotFound
            };
        }
        else
        {
            context.Logger.LogInformation($"Get Request {guid}\n");

            var guidKeys = new Dictionary<string, AttributeValue>();
            guidKeys.Add("Guid", new AttributeValue(guid));
            guidKeys.Add("User", new AttributeValue("andy"));
            GetItemRequest gir = new GetItemRequest(TableName, guidKeys);
            var dbResponse = await dbClient.GetItemAsync(gir);

            if (dbResponse.HttpStatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine($"{dbResponse.IsItemSet} {dbResponse.ToString()}");

                var speciesRecord = ItemsToSpeciesRecord(dbResponse);

                string body = JsonSerializer.Serialize(speciesRecord);

                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = body,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
            else
            {
                response = new APIGatewayProxyResponse()
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }
        }

        return response;
    }

    public async Task<APIGatewayProxyResponse> Post(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation("Post Request\n");
        context.Logger.LogInformation(request.Body);
        var record = JsonSerializer.Deserialize<SpeciesRecord>(request.Body);

        if (record == null) return new APIGatewayProxyResponse { StatusCode = (int)HttpStatusCode.BadRequest };

        var guid = Guid.NewGuid();

        var toSave = new Dictionary<string, AttributeValue>();

        toSave.Add("latitude", new AttributeValue() { N = $"{record.Position.Latitude}" });
        toSave.Add("longitude", new AttributeValue() { N = $"{record.Position.Longitude}" });
        toSave.Add("User", new AttributeValue("andy"));
        toSave.Add("Guid", new AttributeValue(guid.ToString()));
        toSave.Add("Genus", new AttributeValue(record.Type.GenusName));
        toSave.Add("Species", new AttributeValue(record.Type.SpeciesName));
        toSave.Add("Date Added", new AttributeValue(record.RecordAddedTime.ToString("yyyy-MM-ddTHH:MM:ss"))); 
        toSave.Add("Date Recorded", new AttributeValue(record.RecordSeenDate.ToString("yyyy-MM-ddTHH:MM:ss")));

        var ct = new CancellationToken();

        var dbResponse = await dbClient.PutItemAsync(new PutItemRequest(TableName,toSave), ct);
        if (dbResponse.HttpStatusCode == HttpStatusCode.OK)
        {
            Dictionary<string, SpeciesRecord> vals = new Dictionary<string, SpeciesRecord>();
            vals.Add(guid.ToString(), record);
            var sqsMsgBody = JsonSerializer.Serialize(vals);
            context.Logger.LogInformation($"SQS Message is {sqsMsgBody}");
            SendMessageRequest smr = new SendMessageRequest(QueueURL, sqsMsgBody);
            var sqsresponse = await amazonSQSClient.SendMessageAsync(smr);

            if (sqsresponse.HttpStatusCode == HttpStatusCode.OK)
            {
                var okresponse = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                    Body = $"{{ \"guid\"=\"{guid.ToString()}\" }}"
                };

                return okresponse;
            }
            else
            {
                context.Logger.LogError("Error adding request to SQS Queue.");
                context.Logger.LogError($"{sqsresponse.HttpStatusCode}");
                context.Logger.LogError(sqsresponse.ToString());
            }
        }
        else {
            context.Logger.LogError("Error writing to the dababase.");
            context.Logger.LogError($"{dbResponse.HttpStatusCode}");
            context.Logger.LogError(dbResponse.ToString());
        }

        var errresponse = new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            Body = $"{{}}"
        };

        return errresponse;

    }

    public APIGatewayProxyResponse Delete(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation("Delete Request\n");

        var response = new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        };

        return response;
    }

    public APIGatewayProxyResponse Put(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation("Put Request\n");

        var response = new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        };

        return response;
    }

    //
    // Takes a GetItemResponse object and extracts a species record from the items returned.
    private static SpeciesRecord ItemsToSpeciesRecord(GetItemResponse dbResponse)
    {
        var items = dbResponse.Item;
        var speciesN = items["Species"].S;
        var genusN = items["Genus"].S;
        var user = items["User"].S;
        var latitude = Double.Parse(items["latitude"].N);
        var longitude = Double.Parse(items["longitude"].N);
        var dateRecorded = DateTime.Parse(items["Date Recorded"].S);
        var dateAdded = DateTime.Parse(items["Date Added"].S);
        var species = new Species(genusN, speciesN, "");
        var latLong = new LatLong(latitude, longitude);
        var recorder = new Recorder(user);
        var speciesRecord = new SpeciesRecord(dateRecorded, dateAdded, species, recorder, latLong);
        return speciesRecord;
    }
}