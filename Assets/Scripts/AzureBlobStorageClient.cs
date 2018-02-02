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

// Place this script in a script manager object in your scene or use the AzureBlobStorageManager prefab
// IMPORTANT NOTE: This script was *ONLY* designed to work in the Unity editor and in a UWP build.
//   While this should work in standalone Mono/.NET targets for desktops, it has not been tested yet.
//   There is currently no support for cross-platform to support other Unity targets (e.g. Android, iOS, etc.)
public class AzureBlobStorageClient : MonoBehaviour
{
    public static AzureBlobStorageClient instance;

    // Set these in the inspector
    // Note that due to a Unity limitation, you cannot use https, so make sure your endpoint connection string
    // only uses http. THIS MEANS YOUR CONNECTION WILL NOT BE ENCRYPTED. We are working on that.
    public string ConnectionString = string.Empty;
    public string BlockBlobContainerName = "mediacontainerblockblob";  // The blob container where we read from and write to
    [Tooltip("Determines blob downloads will overwrite existing files by default (Can be overriden on each call in code)")]
    public bool OverwriteFilesByDefault = false;

    private CloudStorageAccount StorageAccount;

    // HOW TO LOG RESULTS: Make sure there is a UI Text gameObject named "DebugText" in your scene
    private Text _myText;  // The Text field on the canvas used to output messages in this demo
    private bool IsDebugTextEnabled = false;

    private void Awake()
    {
        // Allows this class instance to behave like a singleton
        instance = this;
    }

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

    #region === BASIC STORAGE OPERATIONS WITHOUT TRACKING (REQUIRES ONLY AZURE STORAGE LIBRARY) ===
    // This function uploads a file to a block blob in an Azure storage container using a single operation,
    // which means it should be avoided for very large media files like videos since there is no way to 
    // track progress on single operations. Look at the code in BlobTransferDM.cs to upload/download while
    // tracking progress since it relies on the Azure Storage Data Movement Library.
    public async Task BasicStorageBlockBlobUploadOperationsAsync(string MediaFile)
    {
        try
        {
            // Create a blob client for interacting with the blob service.
            CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();

            // Create a container for organizing blobs within the storage account.
            WriteLine("Creating Blob Container in Azure Storage.");
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
            WriteLine("Uploading BlockBlob, please wait...");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(MediaFile);

            // Start the timer to measure performance
            var sw = Stopwatch.StartNew();
#if WINDOWS_UWP
		StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(Application.streamingAssetsPath.Replace('/', '\\'));
		StorageFile sf = await storageFolder.GetFileAsync(MediaFile);
		await blockBlob.UploadFromFileAsync(sf);
#else
            await blockBlob.UploadFromFileAsync(Path.Combine(Application.streamingAssetsPath, MediaFile));
#endif
            // Stop the timer and report back on completion + performance
            sw.Stop();
            TimeSpan time = sw.Elapsed;
            WriteLine(string.Format("File uploaded to Azure Storage in {0}s.", time.TotalSeconds.ToString()));
        }
        catch (Exception ex)
        {
            // Woops!
            WriteLine(string.Format("Error while downloading file {0}.", MediaFile));
            WriteLine("Error: " + ex.ToString());
            WriteLine("Error: " + ex.InnerException.ToString());
        }
    }

    /// <summary>
    /// BasicStorageBlockBlobDownloadOperationsAsync
    /// This function downloads a block blob from an Azure storage container into a file using a single operation,
    /// which means it should be avoided for very large media files like videos since there is no way to 
    /// track progress on single operations. 
    /// Use StorageBlockBlobDownloadWithProgressTrackingAsync instead to download using blob segments, which
    /// allows the download progress to be tracked and reported to the user in a UI.
    /// </summary>

    // Overload: If no file overwrite parameter was passed, use the default setting at the class level
    public async Task BasicStorageBlockBlobDownloadOperationsAsync(string MediaFile)
    {
        await BasicStorageBlockBlobDownloadOperationsAsync(MediaFile, OverwriteFilesByDefault);
    }

    // Overload: Override the default file overwrite setting at the class level for this specific file
    public async Task BasicStorageBlockBlobDownloadOperationsAsync(string MediaFile, bool overwrite)
    {
        try
        {
            // Create a blob client for interacting with the blob service.
            CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();

            // Create a container for organizing blobs within the storage account.
            WriteLine("Opening Blob Container in Azure Storage.");
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
            WriteLine("Getting Specific Blob in Container.");

            // We assume the client app knows which asset to download by name
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(MediaFile);

            if (blockBlob != null)
            {
                // Download a blob to your file system
                string path = "";
                WriteLine(string.Format("Downloading Blob from {0}, please wait...", blockBlob.Uri.AbsoluteUri));
                string fileName = MediaFile; // string.Format("CopyOf{0}", MediaFile);

                bool fileExists = false;
#if WINDOWS_UWP
                StorageFolder storageFolder = ApplicationData.Current.TemporaryFolder;
                StorageFile sf;
                try
                {
                    CreationCollisionOption collisionoption = (overwrite ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.FailIfExists);
                    sf = await storageFolder.CreateFileAsync(fileName, collisionoption);
                    fileExists = false; // if the file existed but we were allowed to overwrite it, let's treat it as if it didn't exist
                    path = sf.Path;
                }
                catch (Exception)
                {
                    // The file already exists and we're not supposed to overwrite it
                    fileExists = true;
                    sf = await storageFolder.GetFileAsync(fileName); // Necessary to avoid a compilation error below
                }
#else
                path = Path.Combine(Application.temporaryCachePath, fileName);
                fileExists = File.Exists(path);
#endif
                if (fileExists)
                {
                    if (overwrite)
                    {
                        WriteLine(string.Format("Already exists. Deleting file {0}", fileName));
#if WINDOWS_UWP
                        // Nothing to do here in UWP, we already Replaced it when we created the StorageFile
#else
                        File.Delete(path);
#endif
                    }
                    else
                    {
                        WriteLine(string.Format("File {0} already exists and overwriting is disabled. Download operation cancelled.", fileName));
                        return;
                    }
                }
                // Start the timer to measure performance
                var sw = Stopwatch.StartNew();
#if WINDOWS_UWP
                await blockBlob.DownloadToFileAsync(sf);
#else
                await blockBlob.DownloadToFileAsync(path, FileMode.Create);
#endif
                // Stop the timer and report back on completion + performance
                sw.Stop();
                TimeSpan time = sw.Elapsed;
                WriteLine(string.Format("Blob file downloaded to {0} in {1}s.", path, time.TotalSeconds.ToString()));
            }
            else
            {
                WriteLine(string.Format("File {0} not found in blob {1}.", MediaFile, blockBlob.Uri.AbsoluteUri));
            }
        }
        catch (Exception ex)
        {
            // Woops!
            WriteLine(string.Format("Error while downloading file {0}.", MediaFile));
            WriteLine("Error: " + ex.ToString());
            WriteLine("Error: " + ex.InnerException.ToString());
        }
    }
    #endregion

    #region === STORAGE OPERATIONS BY SEGMENTS WITH TRACKING (REQUIRES ONLY AZURE STORAGE LIBRARY) ===
    /// <summary>
    /// StorageBlockBlobDownloadWithProgressTrackingAsync:
    /// Download a blob using standard Azure Storage library using blob segments.
    /// This allows the download progress to be tracked and reported to the user in a UI.
    /// </summary>
    /// <returns></returns>

    // Overload: If no file overwrite parameter was passed, use the default setting at the class level
    public async Task StorageBlockBlobDownloadWithProgressTrackingAsync(string MediaFile)
    {
        await StorageBlockBlobDownloadWithProgressTrackingAsync(MediaFile, OverwriteFilesByDefault);
    }

    // Overload: Override the default file overwrite setting at the class level for this specific file
    public async Task StorageBlockBlobDownloadWithProgressTrackingAsync(string MediaFile, bool overwrite)
    {
        try
        {
            // Create a blob client for interacting with the blob service.
            CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();

            // Create a container for organizing blobs within the storage account.
            WriteLine("Opening Blob Container in Azure Storage.");
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
            WriteLine("Get Specific Blob in Container and its size");

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(MediaFile);
            int segmentSize = 1 * 1024 * 1024;  //1 MB chunk

            if (blockBlob != null)
            {
                // Obtain the size of the blob
                await blockBlob.FetchAttributesAsync();
                long blobSize = blockBlob.Properties.Length;
                long blobLengthRemaining = blobSize;
                long startPosition = 0;
                WriteLine("3. Blob size (bytes):" + blobLengthRemaining.ToString());

                // Download a blob to your file system
                string path = "";
                WriteLine(string.Format("Downloading Blob from {0}, please wait...", blockBlob.Uri.AbsoluteUri));
                string fileName = MediaFile; // string.Format("CopyOf{0}", MediaFile);

                bool fileExists = false;
#if WINDOWS_UWP
                StorageFolder storageFolder = ApplicationData.Current.TemporaryFolder;
                StorageFile sf;
                try
                {
                    CreationCollisionOption collisionoption = (overwrite ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.FailIfExists);
                    sf = await storageFolder.CreateFileAsync(fileName, collisionoption);
                    fileExists = false; // if the file existed but we were allowed to overwrite it, let's treat it as if it didn't exist
                    path = sf.Path;
                }
                catch (Exception)
                {
                    // The file already exists and we're not supposed to overwrite it
                    fileExists = true;
                    sf = await storageFolder.GetFileAsync(fileName); // Necessary to avoid a compilation error below
                }
#else
                path = Path.Combine(Application.temporaryCachePath, fileName);
                fileExists = File.Exists(path);
#endif
                if (fileExists)
                {
                    if (overwrite)
                    {
                        WriteLine(string.Format("Already exists. Deleting file {0}", fileName));
#if WINDOWS_UWP
                        // Nothing to do here in UWP, we already Replaced it when we created the StorageFile
#else
                        File.Delete(path);
#endif
                    }
                    else
                    {
                        WriteLine(string.Format("File {0} already exists and overwriting is disabled. Download operation cancelled.", fileName));
                        return;
                    }
                }
#if WINDOWS_UWP
                var fs = await sf.OpenAsync(FileAccessMode.ReadWrite);
#else
                FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
#endif
                // Start the timer to measure performance
                var sw = Stopwatch.StartNew();
                do
                {
                    long blockSize = Math.Min(segmentSize, blobLengthRemaining);
                    byte[] blobContents = new byte[blockSize];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        await blockBlob.DownloadRangeToStreamAsync(ms, (long)startPosition, blockSize);
                        ms.Position = 0;
                        ms.Read(blobContents, 0, blobContents.Length);
#if WINDOWS_UWP
                        fs.Seek((ulong)startPosition);
                        await fs.WriteAsync(blobContents.AsBuffer());
#else
                        fs.Position = startPosition;
                        fs.Write(blobContents, 0, blobContents.Length);
#endif
                    }
                    WriteLine("Completed: " + ((float)startPosition / (float)blobSize).ToString("P"));
                    startPosition += blockSize;
                    blobLengthRemaining -= blockSize;
                }
                while (blobLengthRemaining > 0);
                WriteLine("Completed: 100.00%");
#if !WINDOWS_UWP
                fs.Close();  // Required
#endif
                fs = null;

                // Stop the timer and report back on completion + performance
                sw.Stop();
                TimeSpan time = sw.Elapsed;
                WriteLine(string.Format("5. Blob file downloaded to {0} in {1}s", path, time.TotalSeconds.ToString()));
            }
            else
            {
                WriteLine(string.Format("3. File {0} not found in blob {1}", MediaFile, blockBlob.Uri.AbsoluteUri));
            }

            WriteLine("-- Download Test Complete --");
        }
        catch (Exception ex)
        {
            // Woops!
            WriteLine(string.Format("Error while downloading file {0}", MediaFile));
            WriteLine("Error: " + ex.ToString());
            WriteLine("Error: " + ex.InnerException.ToString());
        }
    }
#endregion
}
