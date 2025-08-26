using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance;

    public Material lineMaterial;
    public Color lineColor = Color.white;
    public float lineWidth = 0.1f;
    public float connectionSpeed = 5f; // 连线动画速度

    private List<GameObject> selectedObjects = new List<GameObject>();
    private List<LineRenderer> activeConnections = new List<LineRenderer>();
    private List<Coroutine> activeAnimations = new List<Coroutine>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnObjectClicked(GameObject clickedObject)
    {
        // 如果物体已被选择，则取消选择
        if (selectedObjects.Contains(clickedObject))
        {
            selectedObjects.Remove(clickedObject);
            HighlightObject(clickedObject, false);
            return;
        }

        // 选择新物体
        selectedObjects.Add(clickedObject);
        HighlightObject(clickedObject, true);

        // 如果已选择两个物体，创建连线
        if (selectedObjects.Count == 2)
        {
            CreateConnection(selectedObjects[0], selectedObjects[1]);
            ClearSelection();
        }
    }

    void CreateConnection(GameObject obj1, GameObject obj2)
    {
        // 创建连线对象
        GameObject connectionObj = new GameObject("Connection");
        connectionObj.transform.SetParent(transform);

        // 添加LineRenderer组件
        LineRenderer lineRenderer = connectionObj.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;

        // 初始设置连线位置
        Vector3 startPos = obj1.transform.position;
        Vector3 endPos = obj2.transform.position;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, startPos); // 初始终点设为起点

        // 保存连线引用
        activeConnections.Add(lineRenderer);

        // 启动连线动画
        Coroutine animationCoroutine = StartCoroutine(AnimateConnection(lineRenderer, startPos, endPos));
        activeAnimations.Add(animationCoroutine);

        Debug.Log($"正在连接 {obj1.name} 和 {obj2.name}");
    }

    // 连线动画协程
    IEnumerator AnimateConnection(LineRenderer lineRenderer, Vector3 start, Vector3 end)
    {
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * connectionSpeed;
            Vector3 currentEnd = Vector3.Lerp(start, end, progress);
            lineRenderer.SetPosition(1, currentEnd);
            yield return null;
        }

        // 确保最终位置准确
        lineRenderer.SetPosition(1, end);
        Debug.Log($"连接完成: {start} -> {end}");
    }

    void HighlightObject(GameObject obj, bool highlight)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (highlight)
            {
                // 保存原始材质并应用高亮
                if (!obj.GetComponent<ObjectInfo>())
                {
                    obj.AddComponent<ObjectInfo>().originalMaterial = renderer.material;
                }
                renderer.material.color = Color.yellow; // 高亮颜色
            }
            else
            {
                // 恢复原始材质
                ObjectInfo objectInfo = obj.GetComponent<ObjectInfo>();
                if (objectInfo != null && objectInfo.originalMaterial != null)
                {
                    renderer.material = objectInfo.originalMaterial;
                }
            }
        }
    }

    void ClearSelection()
    {
        foreach (GameObject obj in selectedObjects)
        {
            HighlightObject(obj, false);
        }
        selectedObjects.Clear();
    }

    public void ClearAllConnections()
    {
        // 停止所有动画
        foreach (Coroutine animation in activeAnimations)
        {
            if (animation != null)
            {
                StopCoroutine(animation);
            }
        }
        activeAnimations.Clear();

        // 删除所有连线
        foreach (LineRenderer connection in activeConnections)
        {
            Destroy(connection.gameObject);
        }
        activeConnections.Clear();

        ClearSelection();

        Debug.Log("已清除所有连线");
    }

    // 可选：添加一个方法来创建静态连线（无动画）
    public void CreateStaticConnection(GameObject obj1, GameObject obj2)
    {
        // 创建连线对象
        GameObject connectionObj = new GameObject("StaticConnection");
        connectionObj.transform.SetParent(transform);

        // 添加LineRenderer组件
        LineRenderer lineRenderer = connectionObj.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;

        // 设置连线位置
        lineRenderer.SetPosition(0, obj1.transform.position);
        lineRenderer.SetPosition(1, obj2.transform.position);

        // 保存连线引用
        activeConnections.Add(lineRenderer);

        Debug.Log($"已创建静态连接 {obj1.name} 和 {obj2.name}");
    }
}

// 辅助类，用于存储物体原始信息
public class ObjectInfo : MonoBehaviour
{
    public Material originalMaterial;
}