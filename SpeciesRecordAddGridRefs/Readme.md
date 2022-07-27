## Deploy the stack
Which will listen on the DynamoDB for INSERT operations, and then calculate the OS grid references and add them to the record.

    dotnet lambda deploy-serverless -sn FredFernackerpanStream -sb thegatehousewereham.home -tp SpiderMapsDynamo=SpiderMapsDynamoStack --template-body file://.\serverless.template

At the moment, we get a recursive call as the 'Update' of the grid reference appears to come as another INSERT event.  Probably need to use a different approach here.
