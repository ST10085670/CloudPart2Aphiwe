using System.Net.Http.Json;
using System.Text;
using POE_Model_Library;

namespace ConnectionToFunctionPART_Two
{
    internal class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string baseUrl = "http://localhost:7067/api";

        static async Task Main(string[] args)
        {
            await Menu();
        }

        private static async Task Menu()
        {
            while (true)
            {
                Console.WriteLine("Azure Function Console App");
                Console.WriteLine("1. Upload to Blob Storage");
                Console.WriteLine("2. Read from Queue");
                Console.WriteLine("3. Add Product to Table Storage");
                Console.WriteLine("4. Upload to Azure File Share");
                Console.WriteLine("Enter 'q' to quit.");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await UploadToBlob();
                        break;

                    case "2":
                        await ReadFromQueue();
                        break;

                    case "3":
                        await AddProductToTable();
                        break;

                    case "4":
                        await UploadFileToFileShare();
                        break;

                    case "q":
                        Console.WriteLine("Exiting...");
                        return;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        private static async Task UploadToBlob()
        {
            Console.WriteLine("Please enter the full path of the file you want to upload:");
            string filePath = Console.ReadLine();

            // Display the entered path for debugging purposes
            Console.WriteLine($"Entered file path: {filePath}");

            // Normalize the file path by replacing backslashes with forward slashes
            filePath = filePath.Replace("\\", "/");

            // Check if the file exists at the given path
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File does not exist at path: {filePath}");
                return;
            }

            try
            {
                // Read the file and prepare the content for upload
                var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(File.ReadAllBytes(filePath)), "file", Path.GetFileName(filePath));

                // Send the POST request to upload the file
                var response = await client.PostAsync("http://localhost:7067/api/uploadBlob", content);
                response.EnsureSuccessStatusCode();  // Throws if not successful

                Console.WriteLine("File uploaded successfully.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }


        static async Task UploadFileToFileShare()
        {
            Console.WriteLine("Please enter the full path of the file");
            string filePath = Console.ReadLine();

            // Log the entered path for debugging
            Console.WriteLine($"Entered file path: {filePath}");

            //changing backslashes
            filePath = filePath.Replace("\\", "/");

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File does not exist at path: {filePath}");
                return;
            }

            using var multipartFormContent = new MultipartFormDataContent();
            var fileStreamContent = new StreamContent(File.OpenRead(filePath));
            multipartFormContent.Add(fileStreamContent, "file", Path.GetFileName(filePath));

            try
            {
                HttpResponseMessage response = await client.PostAsync("http://localhost:7067/api/uploadFileShare", multipartFormContent);
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request error: {ex.Message}");
            }
        }

        private static async Task ReadFromQueue()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync("http://localhost:7067/api/readQueueMessage");
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Message from queue: {result.ToString()}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error reading message: {ex.Message}");
            }
        }

        private static async Task AddProductToTable()
        {
            var product = new
            {
                Product_ID = 25,
                Price = 100,
                Product_Name = "Sample Product",
                Product_Description = "Sample Description",
                ImageUrl = "https://st10085670storage.blob.core.windows.net/multimedia/Pens",23
                Stock_Level = "Available"
            };

            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync("http://localhost:7067/api/addProduct", product);
                response.EnsureSuccessStatusCode();
                Console.WriteLine("Product added successfully.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error adding product: {ex.Message}");
            }
        }
    }
}
// IAmTimCorey (2019). Intro to Azure Functions - What they are and how to create and deploy them. YouTube. Available at: https://www.youtube.com/watch?v=zIfxkub7CLY&t=3526s.
// Marczak, A. (2019). Azure Function Apps Tutorial | Introduction for serverless programming. YouTube. Available at: https://www.youtube.com/watch?v=Vxf-rOEO1q4.3
// 3‌