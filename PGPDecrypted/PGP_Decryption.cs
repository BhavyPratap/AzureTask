using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DidiSoft.Pgp;
using Azure.Storage.Blobs;

namespace PGP_Decryption
{
    [StorageAccount("AzureWebJobsStorage")]

    public class PGP_Decryption
    {
        [FunctionName("PGP_Decryption")]
        public static void Run([BlobTrigger("encrypt/{name}.pgp")] Stream blobStream,
        string name)

        {
            var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            string privateKeyPath = config["HiddenPrivateKey"];

            string tempFilePath = Path.GetTempFileName();

            using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
            {
                blobStream.CopyTo(fileStream);
            }

            string decryptedFilePath = Path.GetTempFileName();

            PGPLib pgp = new PGPLib();

            pgp.DecryptFile(tempFilePath, privateKeyPath, "Pass", decryptedFilePath);

            // Download the decrypted file
            var decryptedBlobContainer = new BlobContainerClient(config["AzureWebJobsStorage"], "decrypted/");

            using (var decryptedFileStream = new FileStream(decryptedFilePath, FileMode.Open))
            {
                decryptedBlobContainer.GetBlobClient(name);
            }

            File.Delete(tempFilePath);
            File.Delete(decryptedFilePath);
        }
    }
}

