using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading.Tasks;
using NLog;

namespace AzureBlobBackupSolution
{
    class Program
    {
        // Azure Blob Storage connection string
        private static string connectionString = "your_storage_account_connection_string";
        // Blob container name
        private static string containerName = "backups";
        // File path to the backup file
        private static string filePath = @"C:\backups\sample_backup.bak";
        // AES Encryption key (32 bytes for AES-256)
        private static readonly byte[] aesKey = Convert.FromBase64String("your_32_byte_base64_key");
        // AES Initialization Vector (IV)
        private static readonly byte[] aesIV = Convert.FromBase64String("your_16_byte_base64_iv");

        private static Logger logger = LogManager.GetCurrentClassLogger();

        static async Task Main(string[] args)
        {
            logger.Info("Starting backup process...");

            try
            {
                // Encrypt and compress the file before uploading
                string encryptedFilePath = EncryptAndCompressFile(filePath);

                // Initialize BlobServiceClient
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

                // Get reference to the container
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // Create the container if it doesn't exist
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
                logger.Info($"Blob container '{containerName}' is ready.");

                // Get reference to the blob (file) to upload
                string blobName = Path.GetFileName(encryptedFilePath);
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                // Upload the encrypted and compressed file
                using (FileStream uploadFileStream = File.OpenRead(encryptedFilePath))
                {
                    logger.Info($"Uploading encrypted file: {blobName}");
                    await blobClient.UploadAsync(uploadFileStream, overwrite: true);
                    uploadFileStream.Close();
                }

                logger.Info("File uploaded successfully.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Backup process failed.");
            }
        }

        // Encryption and Compression
        private static string EncryptAndCompressFile(string filePath)
        {
            string outputFilePath = $"{Path.GetFileNameWithoutExtension(filePath)}_encrypted.zip";

            using (FileStream inputFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (FileStream outputFileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            using (Aes aes = Aes.Create())
            {
                aes.Key = aesKey;
                aes.IV = aesIV;

                // Create encryption stream
                using (CryptoStream cryptoStream = new CryptoStream(outputFileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    // Compress and encrypt the file
                    using (GZipStream gzipStream = new GZipStream(cryptoStream, CompressionMode.Compress))
                    {
                        inputFileStream.CopyTo(gzipStream);
                    }
                }
            }

            logger.Info($"File '{filePath}' encrypted and compressed.");
            return outputFilePath;
        }
    }

    public class BackupFunction
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [FunctionName("ScheduledBackup")]
        public async Task Run([TimerTrigger("0 0 2 * * *")] TimerInfo timer, ILogger log)  // Runs daily at 2 AM
        {
            logger.Info("Scheduled backup function started.");

            try
            {
                // Call the backup logic (encrypt, compress, and upload)
                await BackupDataToAzureBlobStorage();
                logger.Info("Scheduled backup completed successfully.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Scheduled backup process failed.");
            }
        }

        private static async Task BackupDataToAzureBlobStorage()
        {
            // Backup logic (refer to backup process in Main)
        }
    }
}
