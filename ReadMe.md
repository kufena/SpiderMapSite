## Spider Mapping App ##

An app that allows you to mark, on a map, spider records, and store information about the record such as date of finding, and so on.

Here's my current architectural thinking on this.

First, a record contains species data, latitude and longitude, who recorded it, when it was recorded and when it was added to the database.  A user has to login in to store this data.
I'm currently trying to keep everything in the serverless realm, so there's a dynamodb to store this data.  Records are given a Guid.  I think species will need an id too, to account
for later or existing changes in taxonomy.

Secondly, there's a database that maps records to OS Grid references at some level of precision.  There will be a second API and a lambda listening on an SQS queue for new records to convert.
I'm thinking six figure grid references and four figure grid references.
So my latitude/longitude gives six figure reference of TL667998 and four figure reference of TL6699.
If I put this data in a different dynamodb table I can index on the six and four figure references, and store record Guids with their references.
Why?  Well, it has two purposes; to hide exact reference data, and searching.

Searching on latitude/longitude wouldn't be too hard with a relational database, but seems much harder in dynamodb, so we could get records by searching on grid reference.
This will return Guids of species records for a particular grid reference, at either reference precision.

Secondly, if I'm recording in my garden, I might not want to identify where my garden is.  So, I think showing anonymised records by grid reference precision gives some anonymity.
The species record API, with species and date, can then be retrieved without latitude/longitude data, by any user, but only you will see your records at that precision.

This is a purely manufactured app, for the purposes of giving me some experience, so it might not be the best architecture, but it's ok for now.

## Installing the App, so far.

Note I have put all the templates in an Infrastructure folder.  This is fine, except that some of them need separate packaging commands, using

    dotnet lambda package <packagefile.zip>

in their respective folders, and a --package parameter passed to the create stack command.  I've used a mix of CloudFormation and SAM here.

In the Infrastructure directory, there is a commands.txt file that contains the kind of commands you'll need to use.
