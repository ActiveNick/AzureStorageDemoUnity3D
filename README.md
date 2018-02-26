# AzureStorageDemoUnity3D
Sample Unity project used to demonstrate the use of Azure Storage services in games and Mixed Reality projects. This app shows how you can upload/download blobs from Azure Storage using the basic Azure Storage SDK (I’m using the Win8 DLL for UWP), and you'll also have the ability to download blobs by segments of 1MB (configurable) which opens the door for tracking progress during the download.

* **Unity version: 2017.2.1p4** [download here](https://beta.unity3d.com/download/1992a1ed2d78/UnityDownloadAssistant-2017.2.1p4.exe?_ga=2.144542141.908717835.1519654169-26152681.1510325491)
* **Mixed Reality Toolkit version: 2017.2.1.1** [download here](https://github.com/Microsoft/MixedRealityToolkit-Unity/releases/tag/2017.2.1.1)

# Demo Scenes
* AzureBlobStorageTest: Simple scene to demonstrate how to upload & download files to/from Azure Blob Storage, showcasing the Azure Transfer ProgressBar and the debug window. The spheres demonstrate the use of the BlobStorageTextureDownloader script.
* AzureBlobStorageVideoTest: Simple movie theater scene used to play video files downloaded from Azure Blob Storage. This scene reuses components and scripts from the [Unity Graphics Live Training: Playing Video in Unity](https://unity3d.com/learn/tutorials/topics/graphics/introduction-and-session-goals?playlist=17102).

## Implementation Notes
* The plugin DLL files are included in the source since they are specific versions selected for this project based on compatibility. The [Azure Storage DLL](https://www.nuget.org/packages/WindowsAzure.Storage/) is the Win8 version for the UWP build, and the .NET 4.5 version for the Unity editor.
* Note that this requires the .NET Scripting backend and support for .NET 4.6 Experimental in Unity. I have not tested this demo with IL2CPP yet.
* When running in the editor you CANNOT use https to connect to Blob Storage due to a certificates limitation in Unity, but https works fine in the UWP build.
* There is an extra button that showcases a blob download example script using the [Azure Storage Data Movement library](https://www.nuget.org/packages/Microsoft.Azure.Storage.DataMovement) and while it currently runs fine in the Unity editor, note that the UWP build export doesn't work. This is because Unity cannot handle a 16299 UWP DLL, so I decided to shelve DMLib for now until Unity 2018.1 adds support for .NET Standard 2.0.
* If you want to dig into the techniques used for Azure Storage in this Unity demo, check out my [UWP XAML Test Client for Azure Storage here](https://github.com/ActiveNick/AzStorageDataMovementTest).

## Reference Links
* [Azure SDKs for Game Developers](https://docs.microsoft.com/sandbox/gamedev/)
* [Azure Solutions for Gaming](https://azure.microsoft.com/solutions/gaming/)
* [Azure Storage API Docs for .NET](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/storage?view=azure-dotnet)
* [Microsoft Azure Storage team's blog](http://blogs.msdn.com/b/windowsazurestorage/) 

## Follow Me
* Twitter: [@ActiveNick](http://twitter.com/ActiveNick)
* Blog: [AgeofMobility.com](http://AgeofMobility.com)
* SlideShare: [http://www.slideshare.net/ActiveNick](http://www.slideshare.net/ActiveNick)
