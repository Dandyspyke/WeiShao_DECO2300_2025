using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;
using System.Collections; // 【必需】引入协程命名空间
using System.Collections.Generic;

public class VideoManager : MonoBehaviour
{
    
    public NotebookManager notebookManager;

    
    [Header("视频播放组件")]
    public VideoPlayer videoPlayer;

    [Header("截图专用组件")]
    [Tooltip("专门用于截图的隐藏相机")]
    public Camera screenshotCamera;
    [Tooltip("截图相机渲染的目标 RenderTexture")]
    public RenderTexture captureRenderTexture; // 这就是ScreenshotCamera的Target Texture

    [Header("截图与图库UI")]
    public GameObject galleryPanel;
    public Transform galleryContent;
    public GameObject screenshotUIPrefab;

    private string screenshotFolderPath;
    private bool isCapturing = false;
    void Awake()
    {
        screenshotFolderPath = Path.Combine(Application.persistentDataPath, "Screenshots");
        if (!Directory.Exists(screenshotFolderPath))
        {
            Directory.CreateDirectory(screenshotFolderPath);
        }
    }
    
  

    public void TogglePlayPause()
    {
        if (videoPlayer == null) return;

        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            Debug.Log("视频已暂停");
        }
        else
        {
            videoPlayer.Play();
            Debug.Log("视频已播放");
        }
    }

    /// <summary>
    /// 这个公共方法现在只负责启动协程
    /// </summary>
    public void TakeScreenshot()
    {
        // 使用状态锁防止重复截图导致问题
        if (!isCapturing)
        {
            StartCoroutine(CaptureScreenshotCoroutine());
        }
    }


    private IEnumerator CaptureScreenshotCoroutine()
    {
        isCapturing = true;

        // 1. 启用截图相机，它会在下一帧自动渲染
        screenshotCamera.gameObject.SetActive(true);

        // 2. 等待一帧，确保相机完成渲染
        yield return new WaitForEndOfFrame();

        // 3. 现在，captureRenderTexture 中包含了视频画面的“照片”
        Texture2D screenshotTexture = new Texture2D(
            captureRenderTexture.width, 
            captureRenderTexture.height, 
            TextureFormat.RGB24, 
            false);

        RenderTexture.active = captureRenderTexture;
        screenshotTexture.ReadPixels(new Rect(0, 0, captureRenderTexture.width, captureRenderTexture.height), 0, 0);
        screenshotTexture.Apply();
        RenderTexture.active = null;

        // 4. 立刻禁用截图相机，节省性能
        screenshotCamera.gameObject.SetActive(false);

        // 5. 保存文件
        byte[] pngBytes = screenshotTexture.EncodeToPNG();
        Destroy(screenshotTexture);

        string fileName = "Screenshot_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
        string filePath = Path.Combine(screenshotFolderPath, fileName);
        
        File.WriteAllBytes(filePath, pngBytes);
        Debug.Log("截图成功！已通过专用相机精确截取并保存至: " + filePath);
        
        // 通知 NotebookManager 关联这张截图
        if (notebookManager != null)
        {
            notebookManager.AddScreenshotToCurrentPlanet(filePath);
        }
        isCapturing = false;
    }
    // ... [ToggleGallery 和 RefreshGallery 方法保持不变] ...
    public void ToggleGallery()
    {
        if (galleryPanel == null) return;
        bool isActive = galleryPanel.activeSelf;
        galleryPanel.SetActive(!isActive);
        if (!isActive)
        {
            RefreshGallery();
        }
    }

    private void RefreshGallery()
    {
        if (galleryContent == null) return;
        foreach (Transform child in galleryContent)
        {
            Destroy(child.gameObject);
        }
        string[] screenshotFiles = Directory.GetFiles(screenshotFolderPath, "*.png");
        foreach (string filePath in screenshotFiles)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D loadedTexture = new Texture2D(2, 2);
            loadedTexture.LoadImage(fileData); 
            GameObject screenshotUI = Instantiate(screenshotUIPrefab, galleryContent);
            RawImage imageComponent = screenshotUI.GetComponent<RawImage>();
            if (imageComponent != null)
            {
                imageComponent.texture = loadedTexture;
            }
        }
    }
}