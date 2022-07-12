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
  var uploader = new BlobStorageUploader(storageAccountKey, storageAccountName);
  await uploader.Upload(args[0], "azimgvizcontainer");
}
else
{
  Console.WriteLine("Provide path to files as an argument.");
}
