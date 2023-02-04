# Open Performance Issue CLI

## Description

Example code for a CLI to access Open Performance Issue (OPI).

## Get started

* Build & run it:

```shell
dotnet run
```

* Run it with a private WebAPI


```shell
dotnet run -- --Endpoint='http://localhost:5041'    # First -- to separate dotnet CLI arguments with OPICLI arguments.
```

## Understand the parameters

There are 2 ways ot set parameters, in `appsettings.jsonc` or provide by command line. Refer to [appsettings.jsonc](./appsettings.jsonc) for all available options.