using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BlobStorageTextureDownloader : MonoBehaviour {

    [Tooltip("Name of texture file to automatically download from Azure Blob Storage")]
    public string TextureFile = "earth_8k.jpg";

    // Use this for initialization
    async void Start () {

        string localimagefile = await AzureBlobStorageClient.instance.DownloadStorageBlockBlobSegmentedOperationAsync(TextureFile);

        if (localimagefile.Length > 0)
        {
            // Create a texture. Texture size does not matter, since
            // LoadImage will replace with with incoming image size.
            Texture2D LoadedImage = new Texture2D(1024, 1024);
            byte[] byteFile = File.ReadAllBytes(localimagefile);
            LoadedImage.LoadImage(byteFile);
            GetComponent<Renderer>().material.mainTexture = LoadedImage;
    }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
