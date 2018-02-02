using UnityEngine;

// Button event handlers for Azure Blob Storage Client demo scene
public class BlockBlobMediaTest : MonoBehaviour
{
    public async void BlockBlobMediaUpload()
    {
        AzureBlobStorageClient.instance.ClearOutput();
        AzureBlobStorageClient.instance.WriteLine("-- Uploading to Blob Storage --");
        await AzureBlobStorageClient.instance.BasicStorageBlockBlobUploadOperationsAsync("earth_8k.jpg");
        AzureBlobStorageClient.instance.WriteLine("-- Upload Test Complete --");
    }

    public async void BlockBlobMediaDownload()
    {
        AzureBlobStorageClient.instance.ClearOutput();
        AzureBlobStorageClient.instance.WriteLine("-- Downloading from Blob Storage --");
        await AzureBlobStorageClient.instance.BasicStorageBlockBlobDownloadOperationsAsync("earth_8k.jpg");
        AzureBlobStorageClient.instance.WriteLine("-- Download Test Complete --");
    }

    public async void BlockBlobMediaDownloadBySegments()
    {
        AzureBlobStorageClient.instance.ClearOutput();
        AzureBlobStorageClient.instance.WriteLine("-- Downloading from Blob Storage by Segments --");
        await AzureBlobStorageClient.instance.StorageBlockBlobDownloadWithProgressTrackingAsync("earth_8k.jpg");
    }

}
