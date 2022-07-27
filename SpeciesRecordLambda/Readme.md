## To deploy

First use Infrastructure/database.json to create the DynamoDB stack.  Remember the stack name.  Let's say it's called SpiderMapsDynamoStack.
Then use the following command:
 
     dotnet lambda deploy-serverless -sn FredFernackerpan -sb thegatehousewereham.home -tp SpiderMapsDynamo=SpiderMapsDynamoStack
 
This will deploy the lambda function, as a stack, that listens for add and get species records.