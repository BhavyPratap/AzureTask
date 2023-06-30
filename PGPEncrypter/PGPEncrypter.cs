using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using DidiSoft.Pgp;

namespace PGPEncrypter
{
    public class PGPEncrypter
    {
        [FunctionName("PGPEncrypter")]
        public void Run([BlobTrigger("upload/{name}", Connection = "AzureWebJobsStorage")]Stream Inputblob,
            [Blob("encrypted/{name}", FileAccess.Write, Connection = "AzureWebJobsStorage")] Stream Outputblob,
            string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {Inputblob.Length} Bytes");

            string publicKey = "Welcome to the Azure";

            PGPLib pgp = new PGPLib();

            pgp.EncryptStream(Inputblob,name, publicKey, Outputblob, true, true);
        }
    }
}