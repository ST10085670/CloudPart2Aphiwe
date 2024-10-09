using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace POE_Model_Library
{
    public class FileShareService
    {
        private readonly string _connectionString;
        private readonly string _fileshareName;

        public FileShareService(string connectionString, string _fileshareName)
        {
            _connectionString = connectionString ??
                throw new ArgumentNullException(nameof(connectionString));
            _fileshareName = fileshareName ??
                throw new ArgumentNullException(nameof(fileshareName));
        }

        /// <summary>
        /// Uploads a file to the Azure File Share
        /// </summary>
        public async Task UploadFileAsync(string filePath, string fileName)
        {
            // Connect to the file share
            ShareClient share = new ShareClient(connectionString, shareName);
            await share.CreateIfNotExistsAsync();

            // Get the root directory of the share
            ShareDirectoryClient rootDirectory = share.GetRootDirectoryClient();

            // Create a file in the directory
            ShareFileClient file = rootDirectory.GetFileClient(fileName);

            // Upload the file from a local path
            using FileStream stream = File.OpenRead(filePath);
            await file.CreateAsync(stream.Length);
            await file.UploadRangeAsync(new HttpRange(0, stream.Length), stream);
        }

        /// <summary>
        /// Downloads a file from the Azure File Share
        /// </summary>
        public async Task DownloadFileAsync(string fileName, string downloadPath)
        {
            // Connect to the file share
            ShareClient share = new ShareClient(connectionString, shareName);

            // Get the root directory of the share
            ShareDirectoryClient rootDirectory = share.GetRootDirectoryClient();

            // Get the file from the directory
            ShareFileClient file = rootDirectory.GetFileClient(fileName);

            if (await file.ExistsAsync())
            {
                ShareFileDownloadInfo download = await file.DownloadAsync();
                using FileStream stream = File.OpenWrite(downloadPath);
                await download.Content.CopyToAsync(stream);
            }
            else
            {
                throw new FileNotFoundException("File not found in the file share");
            }
        }
    }
}
