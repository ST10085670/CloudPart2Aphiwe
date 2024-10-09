using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using POE_Model_Library;


using Microsoft.Azure.Functions.Worker.Http;
using Azure.Storage.Queues;

namespace FunctionApp_CLDV6212_PART_TWO
{
    public  class Function1
    {
        private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        private static readonly BlobService blobService = new BlobService(connectionString);
        private static readonly QueueService queueService = new QueueService(connectionString, "inventory");
        private static readonly TableStorageService tableService = new TableStorageService(connectionString, "Product");
        private static readonly FileShareService fileShareService = new FileShareService(connectionString, "productshare");


        [FunctionName("UploadToBlobStorage")]
        public static async Task<IActionResult> UploadToBlobStorage(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "uploadBlob")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Received request to upload file to Blob Storage.");

            var formdata = await req.ReadFormAsync();
            var file = formdata.Files["file"];
            if (file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("File is missing.");
            }

            using (var stream = file.OpenReadStream())
            {
                var blobUri = await blobService.UploadsAsync(stream, file.FileName);
                return new OkObjectResult($"File uploaded successfully: {blobUri}");
            }
        }

        //[FunctionName("SendMessageToQueue")]
        //public static async Task<IActionResult> SendMessageToQueue(
        //    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "sendQueueMessage")] HttpRequest req,
        //    ILogger log)
        //{
        //    log.LogInformation("Received request to send message to queue.");

        //    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //    dynamic data = JsonConvert.DeserializeObject(requestBody);
        //    string message = data?.message;

        //    if (string.IsNullOrEmpty(message))
        //    {
        //        return new BadRequestObjectResult("Message is missing.");
        //    }

        //    await queueService.SendMessage(message);
        //    return new OkObjectResult($"Message sent to queue: {message}");
        //}

        [FunctionName("ReadMessageFromQueue")]
        public static async Task<IActionResult> ReadMessageFromQueue(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "readQueueMessage")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Received request to read message from queue.");

            // Call the new ReadMessageAsync method
            var message = await queueService.ReadMessageAsync();
            if (message == null)
            {
                return new NotFoundObjectResult("No messages in the queue.");
            }

            return new OkObjectResult($"Message read from queue: {message.ToString()}");
        }

        [FunctionName("AddProductToTable")]
        public static async Task<IActionResult> AddProductToTable(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "addProduct")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Received request to add product to Table Storage.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Product product;
            try
            {
                product = JsonConvert.DeserializeObject<Product>(requestBody);
            }
            catch (JsonException)
            {
                return new BadRequestObjectResult("Invalid product data format.");
            }


            product.PartitionKey = "ProductPartition";
            product.RowKey = Guid.NewGuid().ToString();

            await tableService.AddProductAsync(product);
            return new OkObjectResult($"Product added to Table Storage: {product.Product_Name}");
        }

        [FunctionName("UploadToFileShare")]
        public static async Task<IActionResult> UploadToFileShare(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = "uploadFileShare")] HttpRequest req,
           ILogger log)
        {
            log.LogInformation("Received request to upload file to Azure File Share.");

            // Read form data from the request
            var formdata = await req.ReadFormAsync();
            var file = formdata.Files["file"];
            if (file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("File is missing.");
            }

            // Upload the file to the file share
            var tempFilePath = Path.GetTempFileName();
            try
            {
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                await fileShareService.UploadFileAsync(tempFilePath, file.FileName);
                return new OkObjectResult($"File uploaded successfully to file share: {file.FileName}");
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);  // Clean up the temp file
                }
            }

        }
    }
}
