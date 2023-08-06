using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

// Get WebCam information from the browser
public class WebCam : MonoBehaviour
{
    private WebCamDevice[] devices;
    public RawImage rawImage;

    private WebCamTexture frontCam;

    // Use this for initialization
    IEnumerator Start()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.Log("webcam found");
            devices = WebCamTexture.devices;
            frontCam = new(devices[0].name, Screen.width, Screen.height);
            frontCam.Play();
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
        Texture texture = rawImage.texture;
        var texture2d = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
        Graphics.CopyTexture(texture, texture2d);
        FlipTextureHorizontally(texture2d);
        System.IO.File.WriteAllBytes("Assets/Images/Photo.png", ImageConversion.EncodeToPNG(texture2d));
        SceneManager.LoadScene("Game2");
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