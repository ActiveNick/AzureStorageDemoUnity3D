using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class VideoFileManager : MonoBehaviour {

    [Tooltip("Name of video file to automatically download from Azure Blob Storage")]
    public string VideoFilename = "SurfaceStudio.mp4";
    public WorldSpaceVideo player;

    // Use this for initialization
    async void Start () {
        string localvideofile = await AzureBlobStorageClient.instance.DownloadStorageBlockBlobSegmentedOperationAsync(VideoFilename);

        if (localvideofile.Length > 0)
        {
            player.PlayVideoFromFile(localvideofile);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
