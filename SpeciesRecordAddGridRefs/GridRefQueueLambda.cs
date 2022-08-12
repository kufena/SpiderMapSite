using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using SharedTypes;
using Amazon.DynamoDBv2.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SpeciesRecordAddGridRefs;

public class GridRefQueueLambda
{
    private AmazonDynamoDBClient Client;
    private AmazonSQSClient SQSClient;

    private string TableName;
    private string QueueURL;

    public GridRefQueueLambda()
    {
        Client = new AmazonDynamoDBClient();
        SQSClient = new AmazonSQSClient();

        var env = Environment.GetEnvironmentVariable("TableName");
        if (env != null) TableName = env;
        else TableName = ""; // THIS AINT RIGHT

        env = Environment.GetEnvironmentVariable("SQSURL");
        if (env != null) QueueURL = env;
        else QueueURL = "";
    }

    public async Task FunctionHandler(SQSEvent dynamoEvent, ILambdaContext context)
    {
        var deleteList = new List<DeleteMessageBatchRequestEntry>();
        int counter = 0;

        try
        {
            foreach (var message in dynamoEvent.Records)
            {
                try
                {
                    var speciesRecords = JsonSerializer.Deserialize<Dictionary<string, SpeciesRecord>>(message.Body);
                    if (speciesRecords != null)
                    {
                        foreach (var (guid, sp) in speciesRecords)
                        {
                            context.Logger.LogInformation($"{guid} to be handled.");
                            (var sixFigRef, var fourFigRef) = GeoLogic.MakeGridReferences(sp.Position.Latitude, sp.Position.Longitude);

                            context.Logger.LogInformation($"Grid Refs are {sixFigRef} and {fourFigRef}");

                            var toSave = new Dictionary<string, AttributeValue>();

                            toSave.Add("Guid", new AttributeValue(guid.ToString()));
                            toSave.Add("GridRefFourFigure", new AttributeValue(fourFigRef));
                            toSave.Add("GridRefSixFigure", new AttributeValue(sixFigRef));

                            context.Logger.LogInformation($"Sending Item to {TableName} and then Queue {QueueURL}");

                            var putRequest = new PutItemRequest(TableName, toSave);

                            context.Logger.LogInformation("We've done the create put item request.");
                            var putRespons = await Client.PutItemAsync(putRequest);
                            context.Logger.LogInformation("I've done the put and got a task back.");
                            
                            context.Logger.LogInformation("We are passed the await on the put item async");

                            if (putRespons.HttpStatusCode != System.Net.HttpStatusCode.OK)
                            {
                                context.Logger.LogError("ERROR ERROR ERROR ERROR ERROR!!!!");
                                context.Logger.LogError($"put response httpcode = {putRespons.HttpStatusCode}");
                            }
                            else
                            {
                                context.Logger.LogInformation($"Item put in {TableName} seems to have happened.");
                            }
                            context.Logger.LogInformation("We've checked the status code - you should have seen something!");

                        }
                    }
                }
                catch (Exception inex)
                {
                    context.Logger.LogError(inex.ToString());
                }

                context.Logger.LogInformation($"We are at message {counter}");
                var del = new DeleteMessageBatchRequestEntry($"{counter++}", message.ReceiptHandle);
                deleteList.Add(del);
            }
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex.ToString());
        }
        finally
        {
            var resp = await SQSClient.DeleteMessageBatchAsync(QueueURL, deleteList);
            context.Logger.LogInformation($"Deleting items from the queue: {deleteList.Count} messages");
            if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                context.Logger.LogError($"Error when removing {deleteList.Count} entries from the queue.");
                context.Logger.LogError($"{resp.HttpStatusCode}");
            }
        }
    }
}
