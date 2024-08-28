# Description

This package provides lightweight TableClient wrapper that helps to create repositories around simple data stored in azure tables.
But of cause, you can use TableClient directly and use this package only as a code example.
This package is created to be used in azure functions which by default uses storage account.

This package also replaces previous 'AzureTableManager' package which was not optimally designed and had very bad performance for azure tables containing more than 1000 records.

This package consists of the **AzureTableRepository** that allows you to perform basic operations with Azure table
and a couple of extensions to simplify registering all required services to easily inject 'AzureTableRepository'.

# Examples

## Define TableEntity class

For example, let's assume that there is a need to store 'DataTableEntity' class instances in azure table.
For convenience  let's reuse **EntityBase** class from the package to not implement basic ITableEntity fields in custom table entity class.

```csharp
public class DataTableEntity: EntityBase
{
    public string CustomData {get; set;}
}
```

## Register repository

The register corresponding repository.
When no 'AzureTableRepositoryOptions' passed to 'AddAzureTableRepository' extension method then
a repository creates new table with a name equal to the name of the class in azure function default storage account.

```csharp
services.AddAzureTableRepository<DataTableEntity>()
```

'AzureTableRepositoryOptions' class allows to specify connection name of the alternative storage account
and allows to provide custom table name. Both settings are optional.

```csharp
public class AzureTableRepositoryOptions
{
     public string Connection { get; set; }
     public string TableName { get; set; }
}
```

Then registration code could look like below.

```csharp
services.AddAzureTableRepository<DataTableEntity>(new AzureTableRepositoryOptions 
{
    Connection = "CustomStorage",
    TableName = "customdata"
});
```

When Connection name is provided then application configuration should have corresponding connection string defined.

```json
{
    "ConnectionStrings:CustomStorage": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount;"
}
```

When connection is not specified that connectionstring is read from **AzureWebJobsStorage** environment variable.
That is what makes this package specific to azure functions.

It is possible to register as many repositories as required with different or same options using 'AddAzureTableRepository' method.

The package also provides more convenient method to register many repositories with different options.

```csharp

services.AddAzureTableRepositoriesFromOptions(new List<Type> { typeof(TableEntityType1), typeof(TableEntityType2) )

```

It reads options defined in application configuration.

```json
"AzureTableRepositoryOptions:TableEntityType1:Connection": "Connection1",
"AzureTableRepositoryOptions:TableEntityType1:TableName": "table1",

"AzureTableRepositoryOptions:TableEntityType2:Connection": "Connection2",
"AzureTableRepositoryOptions:TableEntityType2:TableName": "table2",
    
"ConnectionStrings:Connection1": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
"ConnectionStrings:Connection2": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount2;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount2;"

```

When no options provided then default connection and table names are used.


## Inject repository

Inject IAzureTableRepository to your a service

```csharp

public MyService(IAzureTableRepository<DataTableEntity> repository)
{    
}

```