# Description

Dotnet tool to generate list of encrypted fields in a database.

# Installation

```
dotnet tool install --global --add-source ./nupkg Lueben.Microservice.Tools.Database.Encrypt --version 1.0.0
```

Check installation result using following command:

```
dotnet tool list -g
```

|Package Id|Version|Commands|
|---|---|---|
|Lueben.microservice.tools.database.encrypt|1.0.0|Lueben-encryptedcolumns|

To uninstall tool run:

```
dotnet tool -g uninstall Lueben.Microservice.Tools.Database.Encrypt
```

Read more how to manage global tools https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools

# Usage

Lueben-encryptedcolumns [Path_To_DbContext_Assembly]

Console output can be redirected to the file like on the example below.

Lueben-encryptedcolumns "C:\Projects\Leads\development\src\Application\Lueben.Leads.Data\bin\Debug\net6.0\Lueben.Leads.Data.dll" > encrypted_fields.json

# Output example

```json
{
  "Tables": [
    {
      "Name": "Lead",
      "Columns": [
        {
          "Name": "AddressLine1",
          "EncryptionType": "Deterministic"
        },
        {
          "Name": "ContactNumber",
          "EncryptionType": "Deterministic"
        },
        {
          "Name": "Email",
          "EncryptionType": "Deterministic"
        },
        {
          "Name": "FirstName",
          "EncryptionType": "Deterministic"
        },
        {
          "Name": "FullName",
          "EncryptionType": "Deterministic"
        },
        {
          "Name": "LastName",
          "EncryptionType": "Deterministic"
        },
        {
          "Name": "PostCode",
          "EncryptionType": "Deterministic"
        }
      ]
    }
  ]
}

```