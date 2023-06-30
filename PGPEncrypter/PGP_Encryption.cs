using System.IO;
using Azure.Storage.Blobs;
using DidiSoft.Pgp;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace PGP_Encryption
{
    [StorageAccount("BlobConnectionString")]

    public class PGP_Encryption
    {
        [FunctionName("PGP_Encryption")]
        public static void Run([BlobTrigger("upload/{name}")] Stream blobStream,
        string name)

        {
            var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            string publicKeyPath = config["HiddenPublicKey"];

            string tempFilePath = Path.GetTempFileName();

            using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
            {
                blobStream.CopyTo(fileStream);
            }

            string encryptedFilePath = Path.GetTempFileName();

            PGPLib pgp = new PGPLib();

            pgp.EncryptFile(tempFilePath, publicKeyPath , "good" ,encryptedFilePath, true, true);

            // Upload the encrypted file
            var encryptedBlobContainer = new BlobContainerClient(config["AzureWebJobsStorage"], "encrypted/");

            using (var encryptedFileStream = new FileStream(encryptedFilePath, FileMode.Open))
            { 
                encryptedBlobContainer.UploadBlob(name, encryptedFileStream);
            }

            File.Delete(tempFilePath);
            File.Delete(encryptedFilePath);
        }
    }
}