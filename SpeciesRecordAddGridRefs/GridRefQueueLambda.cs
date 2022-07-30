using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SpeciesRecordAddGridRefs;

public class GridRefQueueLambda
{
    private AmazonDynamoDBClient Client;

    private string TableName = "SpiderMapsDynamo";
    private string IndexName = "latitude-longitude-index";


    public GridRefQueueLambda()
    {
        Client = new AmazonDynamoDBClient();
    }

    public async Task FunctionHandler(SQSEvent dynamoEvent, ILambdaContext context)
    {
    }
}
