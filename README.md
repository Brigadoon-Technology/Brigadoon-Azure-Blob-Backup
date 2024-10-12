# Brigadoon Enterprise Data Backup and Recovery Solution Using Azure Blob Storage

This project provides an **Enterprise Data Backup and Recovery Solution** that securely stores backup data in **Azure Blob Storage**. The solution is designed to handle scheduled backups, encrypt and compress backup files, and provide easy recovery options for enterprises in case of data loss. 

## Features

- **Scheduled Data Backup**: Automatically backs up data daily at 2 AM from SQL Server, Azure SQL Database, or on-prem systems.
- **Encryption and Compression**: Data is encrypted using AES encryption and compressed for secure and efficient storage.
- **Integration with Azure Services**: Seamless integration with Azure Blob Storage and Azure Backup for automated recovery processes.
- **Detailed Reporting**: Logs and reports are generated for each backup process, allowing you to monitor the status of backups and recovery.
  
## Technologies and Azure Services Used

- **Azure Blob Storage**: For storing encrypted and compressed backup data.
- **Azure Backup**: For managing automated recovery options.
- **Azure Functions**: To handle scheduled backup tasks.
- **Azure SQL Database**: Integration with Azure SQL for database backup.
- **Encryption (AES)**: Used to encrypt data before storing it in the cloud.
- **GZip Compression**: Compression of backup data to reduce storage size.

## Prerequisites

1. **Azure Subscription**: An active Azure account is required. If you don’t have one, you can create a free Azure account [here](https://azure.microsoft.com/en-us/free/).
2. **Azure Blob Storage Account**: You will need to create a storage account in Azure Blob Storage to store backup data.
3. **Visual Studio IDE* or Visual Studio Code**: We use VS IDE for local development and deployment.
4. **Azure Functions Core Tools**: Install this to run Azure Functions locally.

## Setup Instructions

### Step 1: Create Azure Blob Storage

1. Log in to the [Azure Portal](https://portal.azure.com).
2. Navigate to **Storage Accounts** and click **Create**.
3. Fill in the required details:
   - **Subscription**: Select your Azure subscription.
   - **Resource Group**: Create a new resource group or select an existing one. A resource group is a container that holds related resources for your Azure solution.
   - **Storage Account Name**: Enter a unique name for your storage account (e.g., `myuniquestorageaccount`). The name must be between 3 and 24 characters long and can contain only lowercase letters and numbers.
   - **Region**: Choose the Azure region where you want to create your storage account. It’s best to select a region close to your business to reduce latency.
   - **Performance**: Choose between **Standard** or **Premium** based on your performance needs. Standard is usually sufficient for most applications.
   - **Replication**: Select the replication option that best suits your needs. Options include LRS (Locally Redundant Storage), GRS (Geo-Redundant Storage), and others.
4. Click **Review + Create** to review your selections, then click **Create** to deploy the storage account.
5. Once created, navigate to the storage account and select **Containers** from the left menu.
6. Click **+ Container** to create a new container:
   - **Name**: Enter a name for your container (e.g., `backups`). The name must be lowercase and cannot contain special characters.
   - **Public access level**: Choose **Private (no anonymous access)** to keep your backup data secure.
7. Click **OK** to create the container for storing backup files.


### Step 2: Create Azure Function App

1. Log in to the Azure Portal.
2. Navigate to **Function App** and click **Create**.
3. Fill out the required details:
   - **Subscription**: Choose your subscription.
   - **Resource Group**: Create a new resource group or use an existing one.
   - **Function App Name**: Provide a unique name (e.g., enterprise-backup-function).
   - **Publish**: Select **Code**.
   - **Runtime Stack**: Choose **.NET**.
   - **Version**: Select **.NET 6.0** (or the latest LTS version).
   - **Region**: Choose the region closest to your business.
   - **Storage Account**: Azure will create a storage account for function app-related storage; ensure it matches your region.
   - **Plan**: Choose **Consumption Plan** for cost-efficient scaling or **App Service Plan** for more control over resources.
4. Click **Review + Create** and then **Create**.

### Step 3: Install Azure Functions Tools

1. Install **Azure Functions Core Tools** by running:
   ```bash
   npm install -g azure-functions-core-tools@4 --unsafe-perm true
   ```

2. Install the Azure Functions Extension in Visual Studio Code by searching for Azure Functions in the Extensions marketplace.

### Step 3: Create Azure Function Locally
 - Create a new Azure Functions project in Visual Studio:
 - Select Timer Trigger as the template.
 - Set the schedule to run daily at 2 AM (0 0 2 * * *).
 - Add the backup logic provided in this repository.
 
 3. Set your Azure Blob Storage connection string in the local.settings.json file:
    ```json
    {
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "AzureBlobStorageConnectionString": "your_azure_blob_storage_connection_string"
      }
    }

### Step 3: Configure Backup Logic

In the `ScheduledBackup.cs` file, you will need to configure the following settings:

#### 1. **Azure Blob Storage Connection String**

Replace the `connectionString` variable with your own Azure Blob Storage connection string:

  ```csharp
    string connectionString = "<Your_Azure_Storage_Connection_String>";
  ```
You can get the connection string from your Azure portal under Access Keys in your Storage Account.

2. Backup File Path
Ensure the filePath points to the correct location of the backup file on your system (SQL Server or other databases):
```csharp
  string filePath = @"C:\Backups\DatabaseBackup.bak";
```

This should be the local or network path where your backup files are stored.

3. Blob Storage Container Name
Specify the Azure Blob Storage container where the backup will be uploaded:
```csharp
  string containerName = "backups";
```

Ensure that the container exists in your Azure Blob Storage account or use the following line to create it programmatically:
```csharp
  await blobServiceClient.CreateBlobContainerAsync(containerName);
```

4. Encryption Configuration
You can configure AES encryption for the backup data. Update the encryption key and IV as shown below:
```csharp
  byte[] aesKey = Encoding.UTF8.GetBytes("<Your_32_Character_AES_Key>");
  byte[] aesIV = Encoding.UTF8.GetBytes("<Your_16_Character_IV>");
```

Make sure the key is exactly 32 characters long and the IV is 16 characters long.

5. Compression (Optional)
If you are implementing compression, you can compress the backup before uploading it to Azure Blob Storage. Here is a sample snippet for compression:
```csharp
using (FileStream originalFileStream = new FileStream(filePath, FileMode.OpenOrCreate))
using (FileStream compressedFileStream = File.Create($"{filePath}.gz"))
using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
{
    originalFileStream.CopyTo(compressionStream);
}
```
The backup will be compressed as a .gz file before uploading.

6. Upload to Blob Storage
Finally, ensure that the backup file is uploaded securely to Azure Blob Storage:
```csharp
BlobClient blobClient = blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(Path.GetFileName(filePath));
await blobClient.UploadAsync(fileStream, true);
```

This will upload the backup (or encrypted/compressed file) to the specified container in your Azure Blob Storage account.

### Step 6: Deploy the Azure Function
- **In Visual Studio:** 
  - Sign in to your Azure account.  
- **In Visual Studio Code:** 
  - Click "Sign In" in the Azure extension.
- Right-click on your function app and select **Deploy to Function App**.
- Select your previously created Function App from the list.


### Step 7: Monitor Your Backup Function
 - Go to your Function App in the Azure Portal.
 - Click Monitor to see execution logs and performance metrics.
 - You can also set up Application Insights for more detailed logging and monitoring.

