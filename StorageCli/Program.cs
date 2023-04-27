
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder();
builder.AddCommandLine(args);

var config = builder.Build();

var connectionString = config["connectionString"];
var filePath = config["filePath"];
var container = config["container"];

if(connectionString != null && filePath != null)
{
    var serviceClient = new BlobContainerClient(connectionString, container);
    await serviceClient.CreateIfNotExistsAsync();

    var fileStream = new FileInfo(filePath);

    if (fileStream.Exists)
    {

        var blobClient = new BlobClient(connectionString, container, Path.GetFileName(filePath));

        long totalSize = fileStream.Length;
        var progressHandler = new Progress<long>();
        var lastPercent = 0;

        progressHandler.ProgressChanged += (object? sender, long e) =>
        {
            var percent = (int)Math.Ceiling(((decimal)e / totalSize) * 100);
            if(lastPercent != percent)
            {
                lastPercent = percent;
                Console.WriteLine($"uploaded {percent}% of {totalSize} bytes");
            }
        };

        await blobClient.UploadAsync(fileStream.OpenRead(), progressHandler: progressHandler, accessTier: AccessTier.Cool);
        Console.WriteLine("File Uploaded Successfuly");

    }
    else
        Console.WriteLine("File doesn't exist");
}
else
{
    Console.WriteLine("Invalid Flags. Check values for --connectionString, --filePath, --container");
}
