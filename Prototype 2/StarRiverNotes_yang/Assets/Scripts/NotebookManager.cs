using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

public class NotebookManager : MonoBehaviour
{
    [Header("核心组件引用")]
    [Tooltip("【必需】将场景中的 Near-Far Interactor 拖到这里")]
    public NearFarInteractor rightHandInteractor; 
    
    [Tooltip("【必需】将 Near-Far Interactor 下的 Curve Interaction Caster 子对象拖到这里")]
    public CurveInteractionCaster farCaster; // 【核心修复】将 farCaster 设为公共字段

    [Tooltip("【必需】引用我们创建的B键动作 (OpenNotebook)")]
    public InputActionReference openNotebookAction;
    
    [Header("UI 组件引用")]
    public GameObject associatedDataPanel;
    public TMP_InputField noteInputField;
    [Tooltip("【新增】用于实时显示笔记内容的Text组件")]
    public TMP_Text displayText; //displayText的公共引用
    public Transform galleryContent;
    public GameObject screenshotUIPrefab; // 沿用之前的截图UI预制件
    public Button saveButton;
    public Button closeButton;

    // 私有状态
    private PlanetIdentifier currentActivePlanet;
    private const string DATA_KEY_PREFIX = "PlanetData_"; // PlayerPrefs的键名前缀
    void OnEnable()
    {
        // 注册事件
        openNotebookAction.action.performed += OnOpenNotebookPressed;
        saveButton.onClick.AddListener(SaveCurrentPlanetData);
        closeButton.onClick.AddListener(ClosePanel);
        if (noteInputField != null)
        {
            noteInputField.onValueChanged.AddListener(UpdateDisplayText);
        }
    }
    void Start()
    {
        associatedDataPanel.SetActive(false);
    }
    void OnDisable()
    {
        // 注销事件
        openNotebookAction.action.performed -= OnOpenNotebookPressed;
        saveButton.onClick.RemoveListener(SaveCurrentPlanetData);
        closeButton.onClick.RemoveListener(ClosePanel);
        if (noteInputField != null)
        {
            noteInputField.onValueChanged.RemoveListener(UpdateDisplayText);
        }
    }
    
    private void UpdateDisplayText(string newText)
    {
        if (displayText != null)
        {
            displayText.text = newText;
        }
    }

    /// <summary>
    /// 当按下 OpenNotebook 动作（B键）时调用
    /// </summary>
   
    private void OnOpenNotebookPressed(InputAction.CallbackContext context)
    {
        if (farCaster == null)
        {
            Debug.LogError("NotebookManager中的 Far Caster 没有在Inspector中设置！");
            return;
        }

        Transform casterTransform = farCaster.transform;
        LayerMask casterMask = farCaster.raycastMask;
        float maxDistance = farCaster.castDistance;

        if (Physics.Raycast(casterTransform.position, casterTransform.forward, out RaycastHit hitInfo, maxDistance, casterMask))
        {
            PlanetIdentifier planet = hitInfo.collider.GetComponent<PlanetIdentifier>();
            if (planet != null)
            {
                OpenPanelForPlanet(planet);
            }
        }
    }

    /// <summary>
    /// 【核心入口】由星球的XR Simple Interactable调用
    /// </summary>
    public void OpenPanelForPlanet(PlanetIdentifier planet)
    {
        if (planet == null) return;

        currentActivePlanet = planet;
        Debug.Log("为星球 " + planet.planetID + " 打开笔记。");

        LoadPlanetData();
        associatedDataPanel.SetActive(true);
    }

    private void ClosePanel()
    {
        associatedDataPanel.SetActive(false);
        currentActivePlanet = null; // 关闭时，清除当前激活的星球
    }

    /// <summary>
    /// 加载当前激活星球的数据并填充UI
    /// </summary>
    private void LoadPlanetData()
    {
        string key = DATA_KEY_PREFIX + currentActivePlanet.planetID;
        string json = PlayerPrefs.GetString(key, "");

        PlanetData data;
        if (string.IsNullOrEmpty(json))
        {
            data = new PlanetData(); // 如果没有数据，创建一个新的空数据对象
        }
        else
        {
            data = JsonUtility.FromJson<PlanetData>(json); // 将JSON字符串反序列化为对象
        }

        noteInputField.text = data.noteText;
        UpdateDisplayText(data.noteText); 
        
        RefreshGallery(data.screenshotPaths);
    }

    /// <summary>
    /// 保存当前UI上的数据到当前激活的星球
    /// </summary>
    public void SaveCurrentPlanetData()
    {
        if (currentActivePlanet == null) return;

        string key = DATA_KEY_PREFIX + currentActivePlanet.planetID;
        // 从PlayerPrefs加载现有数据，以防截图列表被覆盖
        string existingJson = PlayerPrefs.GetString(key, "");
        PlanetData data = string.IsNullOrEmpty(existingJson) ? new PlanetData() : JsonUtility.FromJson<PlanetData>(existingJson);
        
        //只更新文本部分
        data.noteText = noteInputField.text;

        string newJson = JsonUtility.ToJson(data); // 将数据对象序列化为JSON字符串
        PlayerPrefs.SetString(key, newJson);
        PlayerPrefs.Save();

        Debug.Log("为星球 " + currentActivePlanet.planetID + " 保存了笔记。");
    }

    /// <summary>
    /// 【核心外部接口】由VideoManager调用，添加截图到当前星球
    /// </summary>
    public void AddScreenshotToCurrentPlanet(string screenshotPath)
    {
        if (currentActivePlanet == null)
        {
            Debug.LogWarning("没有打开任何星球的笔记，截图未关联。");
            return;
        }

        string key = DATA_KEY_PREFIX + currentActivePlanet.planetID;
        string json = PlayerPrefs.GetString(key, "");
        PlanetData data = string.IsNullOrEmpty(json) ? new PlanetData() : JsonUtility.FromJson<PlanetData>(json);

        data.screenshotPaths.Add(screenshotPath); // 添加新的截图路径

        string newJson = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(key, newJson);
        PlayerPrefs.Save();

        Debug.Log("截图 " + screenshotPath + " 已关联到星球 " + currentActivePlanet.planetID);
        
        // 立刻刷新图库显示
        RefreshGallery(data.screenshotPaths);
    }

    private void RefreshGallery(List<string> screenshotPaths)
    {
        foreach (Transform child in galleryContent)
        {
            Destroy(child.gameObject);
        }

        foreach (string path in screenshotPaths)
        {
            if (File.Exists(path))
            {
                byte[] fileData = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
                
                GameObject uiImage = Instantiate(screenshotUIPrefab, galleryContent);
                uiImage.GetComponent<RawImage>().texture = tex;
            }
        }
    }
}