using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using Photon.Pun;

// Get WebCam information from the browser
public class WebCam : MonoBehaviour
{
    private WebCamDevice[] devices;

    private WebCamTexture frontCam;

    private RawImage rawImage;

    // Initates when spawned by networking script
    IEnumerator Start()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.Log("webcam found");
            devices = WebCamTexture.devices;
            frontCam = new(devices[0].name, 1280, 720);
            Debug.Log("Screen: " + Screen.width + " - " + Screen.height);
            frontCam.Play();
            // Attach to the RawImage component on this game object
            rawImage = GetComponent<RawImage>();
            rawImage.texture = frontCam;
        }
        else
        {
            Debug.Log("no webcams found");
        }
    }

    public void SnapPhoto()
    {
        frontCam.Pause();
        DisplayImageLobby();
    }


    private void DisplayImageLobby()
    {
        // If connected and player number registered,
        // display the image on the shared canvas
        if (Networking.PlayerNumber > 0)
        {
            Texture imageTexture = rawImage.texture;

            // Create a new RenderTexture
            var renderTexture = new RenderTexture(imageTexture.width, imageTexture.height, 0);

            // Set the active RenderTexture
            RenderTexture.active = renderTexture;

            // Render the WebCamTexture to the RenderTexture
            Graphics.Blit(imageTexture, renderTexture);

            // Create a new Texture2D
            var texture2d = new Texture2D(imageTexture.width, imageTexture.height, TextureFormat.ARGB32, false);

            // Read the pixel data from the RenderTexture into the Texture2D
            texture2d.ReadPixels(new Rect(0, 0, imageTexture.width, imageTexture.height), 0, 0);

            // Apply the changes to the Texture2D
            texture2d.Apply();

            var photonView = FindObjectOfType<PhotonView>(); // there is only the one lobby instance
            photonView.RPC("RPC_DisplayImage", RpcTarget.AllBuffered, texture2d.EncodeToJPG(), Networking.PlayerNumber);

            // enable start button for current player if they are the master client
            if (PhotonNetwork.IsMasterClient)
            {
                GameObject.Find("Start").GetComponent<Button>().interactable = true;
            }

            // Destroy self, as camera is no longer needed
            Destroy(gameObject);


        }
    }

    public static void FlipTextureHorizontally(Texture2D original)
    {
        var originalPixels = original.GetPixels();

        var newPixels = new Color32[originalPixels.Length];

        var width = original.width;
        var rows = original.height;

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < rows; y++)
            {
                newPixels[x + y * width] = originalPixels[(width - x - 1) + y * width];
            }
        }

        original.SetPixels32(newPixels);
        original.Apply();
    }
}