# Substrate.NET.Metadata

[![Build & Analyse](https://github.com/SubstrateGaming/Substrate.NET.Metadata/actions/workflows/build.yml/badge.svg)](https://github.com/SubstrateGaming/Substrate.NET.Metadata/actions/workflows/build.yml)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=SubstrateGaming_Substrate.NET.Metadata&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=SubstrateGaming_Substrate.NET.Metadata)
[![GitHub issues](https://img.shields.io/github/issues/SubstrateGaming/Substrate.NET.Metadata.svg)](https://github.com/SubstrateGaming/Substrate.NET.Metadata/issues)
[![license](https://img.shields.io/github/license/SubstrateGaming/Substrate.NET.Metadata)](https://github.com/SubstrateGaming/Substrate.NET.Metadata/blob/origin/LICENSE)

Project to manage Substrate based blockchain Metadata from V9 to V14

## How to use ?

### Instanciate metadata classes

Please check the [getMetadataAsync()](https://github.com/SubstrateGaming/Substrate.NET.API/blob/master/Substrate.NetApi/Modules/State.cs#L57) method from the [Substrate.NET.API](https://github.com/SubstrateGaming/Substrate.NET.API) library allows you to get the hexadecimal string representation of the metadata for a given Substrate blockchain. 
Once you have the hexadecimal string representation of the metadata, you can instantiate the corresponding Metadata class by instanciating the right Metadata class.

```c#
  string hexMetadataFromSubstrateNetApi = "0x...";
  var v11 = new MetadataV11(hexMetadataFromSubstrateNetApi);
  var v12 = new MetadataV12(hexMetadataFromSubstrateNetApi);
  var v13 = new MetadataV13(hexMetadataFromSubstrateNetApi);
  var v14 = new MetadataV14(hexMetadataFromSubstrateNetApi);
``` 

If you are not sure of your metadata version you can call :
```c#
  string hexMetadataFromSubstrateNetApi = "0x...";
  var metadataInfo = new CheckRuntimeMetadata(hexMetadataFromSubstrateNetApi);

  // metadataInfo.MetaDataInfo.Version.Value -> 11 / 12 / 13 / 14
```

### Compare metadata

You can also compare metadata between each other, but it should be the same major version (compare V12 with V12 and V14 with V14)

The package also provides a MetadataService class that can be instantiated directly or used with dependency injection. 
Metadata comparison can provide a differential between two versions, including removed or added pallets, as well as more precise information such as function calls that have been renamed. The MetadataService class supports comparison from version 9 to 14.

## Dependencies

- [Substrate .NET API](https://github.com/SubstrateGaming/Substrate.NET.API)  
  Substrate .NET API Core for substrate-based nodes