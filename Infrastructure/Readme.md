## How does this directory work?

I'm not sure if this is the right way to do things with Cloudformation, but here is how these templates are designed to work.

The principle is to have a bunch of templates, each implementing a specific area of infrastructure/architecture.

  - They are meant to be applied in an order.
  - They also inform each other of things that have been created.
  - They set SSM Parameter Store parameters for working code.
  - The only common parameter is a string identifying the STAGE, used for parameter access, API creation, naming, and so on.

These four principles are intended to ensure that no manual set-up is required to deploy any of this code.
However, it also means that changes to one particular template further up the tree, may require other templates dependent upon it to be
updated also.  This, to me, is the main argument against this approach.
