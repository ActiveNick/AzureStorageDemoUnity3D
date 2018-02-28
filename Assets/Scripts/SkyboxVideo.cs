using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

public class SkyboxVideo : MonoBehaviour {

    // The video to load as a 360 video
    public string videoClip;

    // Used to control playback, we get these from the current gameobject
    private VideoPlayer videoPlayer;
    private AudioSource audioSource;

    // Use this for initialization
    private void Awake () {
        // Obtain video playback components from current gameobject
        videoPlayer = GetComponent<VideoPlayer>();
        audioSource = gameObject.AddComponent<AudioSource>();
    }
	
	// Update is called once per frame
	async void Start () {
        // Need to release the video texture to clear the screen on start, 
        // otherwise the last played frame from the last session will stick around
        videoPlayer.targetTexture.Release();
        // Set current video clip to the first one in the array, downloads it if needed
        await PrepareVideoFromFile(videoClip);
    }

    /// <summary>
    /// Prepares a video file for playback by loading it in the video player + initializing audio.
    /// If the file, does not exist, it is downloaded from Azure blob storage first, using default override settings.
    /// All files are downloaded to the TempCache app folder, and played from there.
    /// </summary>
    /// <param name="videofile">The file name (no path) of the video to be played.</param>
    public async Task PrepareVideoFromFile(string videofile)
    {
        string localvideofile = await AzureBlobStorageClient.instance.DownloadStorageBlockBlobSegmentedOperationAsync(videofile);

        if (File.Exists(localvideofile))
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
            videoPlayer.url = localvideofile;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, audioSource);

            videoPlayer.Play();
        }
    }
}
