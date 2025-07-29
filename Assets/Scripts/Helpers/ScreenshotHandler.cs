using UnityEngine;
using System.IO;

/// <summary>
/// 按下p键就可以将Game窗口中进行截图，存储在本地
/// </summary>
public class ScreenshotHandler : MonoBehaviour
{
    public string screenshotFolder = "Screenshots";
    public string screenshotName = "screenshot";
    public int resolutionMultiplier = 1;

    private void Update()
    {
        // 测试用，当按下“P”键时捕捉屏幕
        if (Input.GetKeyDown(KeyCode.P))
        {
            CaptureScreenshot();
        }
    }

    public void CaptureScreenshot()
    {
        string folderPath = Path.Combine(Application.dataPath, screenshotFolder);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filePath = Path.Combine(folderPath, screenshotName + "_" + timestamp + ".png");

        ScreenCapture.CaptureScreenshot(filePath, resolutionMultiplier);
        Debug.Log("Screenshot saved to: " + filePath);
    }
}