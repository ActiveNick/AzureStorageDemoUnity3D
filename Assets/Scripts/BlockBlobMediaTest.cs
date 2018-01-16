﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using UnityEngine;
using Random = System.Random;

#if WINDOWS_UWP
using Windows.Storage;
#endif

public class BlockBlobMediaTest : BaseStorage
{
    public string BlockBlobContainerName = "mediacontainerblockblob";
    public string TestMediaFile = "earth_8k.jpg";

    public async void BlockBlobMediaUpload()
    {
        ClearOutput();
        WriteLine("-- Uploading to Blob Storage --");
        await BasicStorageBlockBlobUploadOperationsAsync();
    }

    public async void BlockBlobMediaDownload()
    {
        ClearOutput();
        WriteLine("-- Downloading from Blob Storage --");
        await BasicStorageBlockBlobDownloadOperationsAsync();
    }

    private async Task BasicStorageBlockBlobUploadOperationsAsync()
    {
        WriteLine("Testing BlockBlob Upload");

        // Create a blob client for interacting with the blob service.
        CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();

        // Create a container for organizing blobs within the storage account.
        WriteLine("1. Creating Container");
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

        // To view the uploaded blob in a browser, you have two options. The first option is to use a Shared Access Signature (SAS) token to delegate 
        // access to the resource. See the documentation links at the top for more information on SAS. The second approach is to set permissions 
        // to allow public access to blobs in this container. Uncomment the line below to use this approach. Then you can view the image 
        // using: https://[InsertYourStorageAccountNameHere].blob.core.windows.net/democontainer/HelloWorld.png
        // await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

        // Upload a BlockBlob to the newly created container
        WriteLine("2. Uploading BlockBlob");
        CloudBlockBlob blockBlob = container.GetBlockBlobReference(TestMediaFile);

#if WINDOWS_UWP
		StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(Application.streamingAssetsPath.Replace('/', '\\'));
		StorageFile sf = await storageFolder.GetFileAsync(TestMediaFile);
		await blockBlob.UploadFromFileAsync(sf);
#else
        await blockBlob.UploadFromFileAsync(Path.Combine(Application.streamingAssetsPath, TestMediaFile));
#endif

        WriteLine("-- Upload Test Complete --");
    }

    private async Task BasicStorageBlockBlobDownloadOperationsAsync()
    {
        WriteLine("Testing BlockBlob Download");

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


        // List all the blobs in the container 
        WriteLine("2. Get Specific Blob in Container");

        //CloudBlockBlob blockBlob = null;
        //BlobContinuationToken token = null;
        //BlobResultSegment list = await container.ListBlobsSegmentedAsync(token);
        //foreach (IListBlobItem blob in list.Results)
        //{
        //    // Blob type will be CloudBlockBlob, CloudPageBlob or CloudBlobDirectory
        //    // Use blob.GetType() and cast to appropriate type to gain access to properties specific to each type
        //    WriteLine(string.Format("- {0} (type: {1})", blob.Uri, blob.GetType()));

        //    // This next line doesn't work, need to check for the name on the specific blob type
        //    if (blob == TestMediaFile)
        //    {
        //        blockBlob = (CloudBlockBlob)blob;
        //    }
        //}

        CloudBlockBlob blockBlob = container.GetBlockBlobReference(TestMediaFile);

        if (blockBlob != null)
        {
            // Download a blob to your file system
            string path;
            WriteLine(string.Format("3. Download Blob from {0}", blockBlob.Uri.AbsoluteUri));
            string fileName = string.Format("CopyOf{0}", TestMediaFile);

#if WINDOWS_UWP
            StorageFolder storageFolder = ApplicationData.Current.TemporaryFolder;
            StorageFile sf = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            path = sf.Path;
            await blockBlob.DownloadToFileAsync(sf);
#else
        path = Path.Combine(Application.temporaryCachePath, fileName);
        await blockBlob.DownloadToFileAsync(path, FileMode.Create);
#endif
            WriteLine(string.Format("4. Blob file downloaded to {0}", path));

            //WriteLine("File written to " + path);

            //// Clean up after the demo 
            //WriteLine("5. Delete block Blob");
            //await blockBlob.DeleteAsync();

            //// When you delete a container it could take several seconds before you can recreate a container with the same
            //// name - hence to enable you to run the demo in quick succession the container is not deleted. If you want 
            //// to delete the container uncomment the line of code below. 
            //WriteLine("6. Delete Container -- Note that it will take a few seconds before you can recreate a container with the same name");
            //await container.DeleteAsync();
        }

        WriteLine("-- Download Test Complete --");
    }
}
