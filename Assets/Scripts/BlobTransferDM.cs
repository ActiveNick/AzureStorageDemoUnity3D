using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;

using UnityEngine;
using Random = System.Random;

#if WINDOWS_UWP
using Windows.Storage;
#endif

// Use these functions to upload/download large media files since they allow you to
// track progress since it relies on the Azure Storage Data Movement Library.
// This code was designed to work in the Unity editor and in a UWP build.
// There is currently no support for cross-platform to support other Unity targets (e.g. Android, iOS, etc.)
public class BlobTransferDM : BaseStorage
{
    // Set these in the inspector
    public string BlockBlobContainerName = "mediacontainerblockblob";  // The blob container where we read from and write to
    public string TestMediaFile = "earth_8k.jpg"; // The media file to upload or download

    public async void BlockBlobMediaDownload()
    {
        ClearOutput();
        WriteLine("-- Downloading from Blob Storage --");
        await StorageDataMovementBlockBlobDownloadAsync();
    }

    // TO DO: Add support for uploads

    // Download using the Azure Storage Data Movement Library.
    private async Task StorageDataMovementBlockBlobDownloadAsync()
    {
        WriteLine("Downloading BlockBlob with ASDM Library");

        // Create a blob client for interacting with the blob service.
        CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();

        // Create a container for organizing blobs within the storage account.
        WriteLine("1. Opening Blob Container");
        CloudBlobContainer container = blobClient.GetContainerReference(BlockBlobContainerName);
        try
        {
            await container.CreateIfNotExistsAsync();
        }
        catch (StorageException)
        {
            WriteLine("If you are running with the default configuration please make sure you have started the storage emulator. Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
            throw;
        }

        // Get a reference to the blob we want in the container 
        WriteLine("2. Get Specific Blob in Container");

        CloudBlockBlob blockBlob = container.GetBlockBlobReference(TestMediaFile);

        if (blockBlob != null)
        {
            // Setup the number of the concurrent operations
            TransferManager.Configurations.ParallelOperations = 64;
            // Setup the transfer context and track the upload progress
            SingleTransferContext context = new SingleTransferContext();
            context.ProgressHandler = new Progress<TransferStatus>((progress) =>
            {
                WriteLine("  Bytes downloaded: " + progress.BytesTransferred.ToString());
            });

            // Download a blob to your file system
            string path;
            WriteLine(string.Format("3. Download Blob from {0}", blockBlob.Uri.AbsoluteUri));
            string fileName = string.Format("CopyOf{0}", TestMediaFile);

#if WINDOWS_UWP
            // TO DO: UWP code is not complete and has not been tested yet. This is the old single operation code 
            // I have only tested Data Movement in the Unity editor
            StorageFolder storageFolder = ApplicationData.Current.TemporaryFolder;
            StorageFile sf = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            path = sf.Path;
            Stream sfs = await sf.OpenStreamForWriteAsync();
            
            // Download a local blob with progress updates
            DownloadOptions dOptions = new DownloadOptions();
            dOptions.DisableContentMD5Validation = true;  // TO DO: Need to test if MD5 works, currently disabled
            await TransferManager.DownloadAsync(blockBlob, sfs, dOptions, context, CancellationToken.None);
#else
            path = Path.Combine(Application.temporaryCachePath, fileName);

            // Download a local blob with progress updates
            DownloadOptions dOptions = new DownloadOptions();
            dOptions.DisableContentMD5Validation = true;  // TO DO: Need to test if MD5 works, currently disabled
            await TransferManager.DownloadAsync(blockBlob, path, dOptions, context, CancellationToken.None);
#endif
            WriteLine(string.Format("4. Blob file downloaded with ADSM to {0}", path));
        }
    }
}
