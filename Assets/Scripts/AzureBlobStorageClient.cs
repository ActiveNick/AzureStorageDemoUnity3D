// Namespaces: Mono | .NET
using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
#if WINDOWS_UWP
using Windows.Storage;
// Required for Byte[].AsBuffer
using System.Runtime.InteropServices.WindowsRuntime;
#endif
// Namespaces: Unity
using UnityEngine;
using UnityEngine.UI;
// Namespaces: Azure
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

// This code was designed to work in the Unity editor and in a UWP build.
// There is currently no support for cross-platform to support other Unity targets (e.g. Android, iOS, etc.)
public class AzureBlobStorageClient : MonoBehaviour
{
    public static AzureBlobStorageClient instance;

    // Set these in the inspector
    // Note that due to a Unity limitation, you cannot use https, so make sure your endpoint connection string
    // only uses http. THIS MEANS YOUR CONNECTION WILL NOT BE ENCRYPTED. We are working on that.
    public string ConnectionString = string.Empty;
    public string BlockBlobContainerName = "mediacontainerblockblob";  // The blob container where we read from and write to

    private CloudStorageAccount StorageAccount;

    // Used for logging
    private Text _myText;  // The Text field on the canvas used to output messages in this demo
    private bool IsDebugTextEnabled = false;

    private void Awake()
    {
        // Allows this class instance to behave like a singleton
        instance = this;
    }

    // Constructor
    void Start()
    {
        _myText = GameObject.Find("DebugText").GetComponent<Text>();
        IsDebugTextEnabled = (_myText != null);

        StorageAccount = CloudStorageAccount.Parse(ConnectionString);
    }

    // Clears the Canvas output text
    public void ClearOutput()
    {
        if (IsDebugTextEnabled)
        {
            _myText.text = string.Empty;
        }
    }

    // Appends a string to a new line in the canvas output text
    public void WriteLine(string s)
    {
        if (IsDebugTextEnabled)
        {
            if (_myText.text.Length > 20000)
                _myText.text = string.Empty + "-- TEXT OVERFLOW --";

            _myText.text += s + "\r\n"; 
        }
    }

    // This function uploads a file to a block blob in an Azure storage container using a single operation,
    // which means it should be avoided for very large media files like videos since there is no way to 
    // track progress on single operations. Look at the code in BlobTransferDM.cs to upload/download while
    // tracking progress since it relies on the Azure Storage Data Movement Library.
    public async Task BasicStorageBlockBlobUploadOperationsAsync(string MediaFile)
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

        // Get a BlockBlob reference for the file to upload to the newly created container
        WriteLine("2. Uploading BlockBlob...");
        CloudBlockBlob blockBlob = container.GetBlockBlobReference(MediaFile);

        var sw = Stopwatch.StartNew();
#if WINDOWS_UWP
		StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(Application.streamingAssetsPath.Replace('/', '\\'));
		StorageFile sf = await storageFolder.GetFileAsync(TestMediaFile);
		await blockBlob.UploadFromFileAsync(sf);
#else
        await blockBlob.UploadFromFileAsync(Path.Combine(Application.streamingAssetsPath, MediaFile));
#endif
        sw.Stop();
        TimeSpan time = sw.Elapsed;

        WriteLine(string.Format("3. File uploaded in {0}s", time.TotalSeconds.ToString()));

        WriteLine("-- Upload Test Complete --");
    }

    // This function downloads a block blob from an Azure storage container into a file using a single operation,
    // which means it should be avoided for very large media files like videos since there is no way to 
    // track progress on single operations. Look at the code in BlobTransferDM.cs to upload/download while
    // tracking progress since it relies on the Azure Storage Data Movement Library.
    public async Task BasicStorageBlockBlobDownloadOperationsAsync(string MediaFile)
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

        // Access a specific blob in the container 
        WriteLine("2. Get Specific Blob in Container");

        // NOTE: The following code isn't needed for now because we assume the client app knows which asset to
        // download by name, so there is no need to iterate through all the blobs.
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

        CloudBlockBlob blockBlob = container.GetBlockBlobReference(MediaFile);

        if (blockBlob != null)
        {
            // Download a blob to your file system
            string path;
            WriteLine(string.Format("3. Download Blob from {0}...", blockBlob.Uri.AbsoluteUri));
            string fileName = string.Format("CopyOf{0}", MediaFile);

            var sw = Stopwatch.StartNew();
#if WINDOWS_UWP
            StorageFolder storageFolder = ApplicationData.Current.TemporaryFolder;
            StorageFile sf = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            path = sf.Path;
            await blockBlob.DownloadToFileAsync(sf);
#else
            path = Path.Combine(Application.temporaryCachePath, fileName);
            await blockBlob.DownloadToFileAsync(path, FileMode.Create);
#endif
            sw.Stop();
            TimeSpan time = sw.Elapsed;

            WriteLine(string.Format("4. Blob file downloaded to {0} in {1}s", path, time.TotalSeconds.ToString()));

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

    #region === REQUIRES ONLY AZURE STORAGE LIBRARY (WITH PROGRESS TRACKING) ===
    /// <summary>
    /// Download a blob using standard Azure Storage library (with progress tracking)
    /// </summary>
    /// <returns></returns>
    public async Task StorageBlockBlobDownloadWithProgressTrackingAsync(string MediaFile)
    {
        try
        {
            WriteLine("Testing BlockBlob Download with Progress Tracking");

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

            // Access a specific blob in the container 
            WriteLine("2. Get Specific Blob in Container and its size");

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(MediaFile);
            int segmentSize = 1 * 1024 * 1024;//1 MB chunk

            if (blockBlob != null)
            {
                // Obtain the size of the blob
                await blockBlob.FetchAttributesAsync();
                long blobSize = blockBlob.Properties.Length;
                long blobLengthRemaining = blobSize;
                long startPosition = 0;
                WriteLine("3. Blob size (bytes):" + blobLengthRemaining.ToString());

                // Download a blob to your file system
                string path;
                WriteLine(string.Format("4. Download Blob from {0}...", blockBlob.Uri.AbsoluteUri));
                string fileName = string.Format("CopyOf{0}", MediaFile);

                var sw = Stopwatch.StartNew();
#if WINDOWS_UWP
                    StorageFolder storageFolder = ApplicationData.Current.TemporaryFolder;
                    StorageFile sf = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                    path = sf.Path;
                    var fs = await sf.OpenAsync(FileAccessMode.ReadWrite);

                    do
                    {
                        long blockSize = Math.Min(segmentSize, blobLengthRemaining);
                        byte[] blobContents = new byte[blockSize];
                        using (MemoryStream ms = new MemoryStream())
                        {
                            await blockBlob.DownloadRangeToStreamAsync(ms, (long)startPosition, blockSize);
                            ms.Position = 0;
                            ms.Read(blobContents, 0, blobContents.Length);
                            fs.Seek((ulong)startPosition);
                            await fs.WriteAsync(blobContents.AsBuffer());
                        }
                        WriteLine("Completed: " + ((float)startPosition / (float)blobSize).ToString("P"));
                        startPosition += blockSize;
                        blobLengthRemaining -= blockSize;
                    }
                    while (blobLengthRemaining > 0);
                    WriteLine("Completed: 100.00%");
                    fs = null;
#else
                path = Path.Combine(Application.temporaryCachePath, fileName);
                // This technique has not been implemented for us in the editor yet
                //await blockBlob.DownloadToFileAsync(path, FileMode.Create);
#endif
                sw.Stop();
                TimeSpan time = sw.Elapsed;

                WriteLine(string.Format("5. Blob file downloaded to {0} in {1}s", path, time.TotalSeconds.ToString()));
            }

            WriteLine("-- Download Test Complete --");
        }
        catch (Exception ex)
        {
            // Woops!
            WriteLine("Error: " + ex.ToString());
            WriteLine("Error: " + ex.InnerException.ToString());
        }
    }
    #endregion
}
