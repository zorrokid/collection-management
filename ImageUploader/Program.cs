using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using ImageUploader.Uploader;
using Microsoft.Extensions.Configuration;


// See https://aka.ms/new-console-template for more information
Console.WriteLine("Image batch uploader");

var config = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddUserSecrets<Program>()
    .Build();
string storageAccountName = config.GetSection("StorageAccountName").Value;
string storageAccountKey = config.GetSection("StorageAccountKey").Value;

if (args.Length > 0)
{
  Console.WriteLine($"using {args[0]} as read path");
  var uploader = new BlobStorageUploader(args[0], storageAccountKey, storageAccountName);
}
else
{
  Console.WriteLine("Provide path to files as an argument.");
}
