using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.DynamoDBv2.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SpeciesRecordAddGridRefs;

public class Function
{
    private AmazonDynamoDBClient Client;

    private string TableName = "SpiderMapsDynamo";
    private string IndexName = "latitude-longitude-index";


    public Function()
    {
        Client = new AmazonDynamoDBClient();
    }

    public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {
        context.Logger.LogInformation($"Beginning to process {dynamoEvent.Records.Count} records...");
        int count = 0;

        foreach (var record in dynamoEvent.Records)
        {
            count++;

            context.Logger.LogInformation($"Event ID: {record.EventID}");
            context.Logger.LogInformation($"Event Name: {record.EventName}");

            string Guid = record.Dynamodb.Keys["Guid"].S;
            string User = record.Dynamodb.Keys["User"].S;

            if (record.EventName.Equals("INSERT"))
            {
                var key = new Dictionary<string, AttributeValue>();
                key.Add("Guid", new AttributeValue(Guid));
                key.Add("User", new AttributeValue(User));

                double latitude;
                double longitude;
                try
                {
                    if (!double.TryParse(record.Dynamodb.NewImage["latitude"].N, out latitude))
                    {
                        context.Logger.LogError("Error whilst parsing latitude");
                        continue;
                    }

                    if (!double.TryParse(record.Dynamodb.NewImage["longitude"].N, out longitude))
                    {
                        context.Logger.LogError("Error whilst parsing longitude");
                        continue;
                    }
                }
                catch (Exception e)
                {
                    context.Logger.LogError("Error whilst parsing latitude/longitude");
                    context.Logger.LogError(e.ToString());
                    continue;
                }

                var GridRefs = GeoLogic.MakeGridReferences(latitude, longitude);

                Dictionary<string, AttributeValueUpdate> newValues = new Dictionary<string, AttributeValueUpdate>();
                newValues.Add("GridRefSixFigures", new AttributeValueUpdate(new AttributeValue(GridRefs.GridRefSixFig), AttributeAction.PUT));
                newValues.Add("GridRefFourFigures", new AttributeValueUpdate(new AttributeValue(GridRefs.GridRefFourFig), AttributeAction.PUT));

                UpdateItemRequest uir = new UpdateItemRequest(TableName, key, newValues);
                var response = await Client.UpdateItemAsync(uir);
                if (response != null)
                {
                    context.Logger.LogInformation($"Updated lat/lon on item, response was {response.HttpStatusCode}");
                }
                else
                {
                    context.Logger.LogError("Update lat/lon on item, response was NULL");
                }
            }
        }

        context.Logger.LogInformation("Stream processing complete.");

        //string buildIt(AttributeValue vt)
        //{
        //    if (vt.N != null)
        //        return vt.N;
        //    return vt.S;
        //}
    }
}