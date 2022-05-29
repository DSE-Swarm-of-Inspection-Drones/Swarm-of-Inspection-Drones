using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImagesForCompression : MonoBehaviour
{
    // image resolution
    public int resWidth = 8192;
    public int resHeight = 6144;

    // configurations of the Format of the image
    public enum Format { RAW, JPG, PNG, PPM };
    public Format format = Format.PPM;

    // folder to write to
    public string folder;

    // private variables for the screenshot
    private Rect rect;
    private RenderTexture renderTexture;
    private Texture2D screenShot;
    private int counter = 0; // image #

    // commands
    public bool captureScreenshot = false;


    public string ScreenShotName(int width, int height)
    {
        // check if folder is created or not
        if (folder == null || folder.Length == 0)
        {
            folder = Application.persistentDataPath;
            folder += "/screenshots";
            System.IO.Directory.CreateDirectory(folder);
        }

        // check the amount of screenshots in the folder and update the counter
        string mask = string.Format("screen_{0}x{1}*.{2}", width, height, format.ToString().ToLower());
        counter = Directory.GetFiles(folder, mask, SearchOption.TopDirectoryOnly).Length;

        // unique filename
        var filename = string.Format("{0}/screen_{1}x{2}_{3}.{4}", folder,
            width, height, counter, format.ToString().ToLower());

        // update counter
        ++counter;

        return filename;
    }

    public void CaptureScreenshot()
    {
        captureScreenshot = true;
    }

    public void CreateScreenshot()
    {
        if (captureScreenshot)
        {
            captureScreenshot = false;

            //create the texture
            if (renderTexture == null)
            {
                rect = new Rect(0, 0, resWidth, resHeight);
                renderTexture = new RenderTexture(resWidth, resHeight, 30);
                screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            }
            // Camera stuff, need to change for different cameras probably
            Camera camera = this.GetComponent<Camera>();
            camera.targetTexture = renderTexture;
            camera.Render();

            // render texture active and read the pixels
            RenderTexture.active = renderTexture;
            screenShot.ReadPixels(rect, 0, 0);

            // reset camera texture and render texture
            camera.targetTexture = null;
            RenderTexture.active = null;

            // get unique filename
            string filename = ScreenShotName(resWidth, resHeight);

            //
            byte[] fileHeader = null;
            byte[] fileData = null;
            if (format == Format.RAW)
            {
                fileData = screenShot.GetRawTextureData();
            }
            else if (format == Format.PNG)
            {
                fileData = screenShot.EncodeToPNG();
            }
            else if (format == Format.JPG)
            {
                fileData = screenShot.EncodeToJPG();
            }
            else // ppm
            {
                string headerStr = string.Format("P6\n{0} {1}\n225\n", rect.width, rect.height);
                fileHeader = System.Text.Encoding.ASCII.GetBytes(headerStr);
                fileData = screenShot.GetRawTextureData();
            }
            hofman(fileHeader, fileData);
            //new System.Threading.Thread(() =>
            //{
            //    var f = System.IO.File.Create(filename);
            //    if (fileHeader != null) f.Write(fileHeader, 0, fileHeader.Length);
            //    f.Write(fileData, 0, fileData.Length);
            //    f.Close();
            //    Debug.Log(string.Format("Wrote screenshot {0} of size {1}", filename, fileData.Length));
            //}).Start();
        }
    }

    // trying hofman coding
    public void hofman(byte[] header, byte[] data)
    {
        Debug.Log(string.Format("data is {0}", data.Length));
    }
}
