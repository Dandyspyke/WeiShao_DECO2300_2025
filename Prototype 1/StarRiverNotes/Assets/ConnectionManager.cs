using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance;

    public Material lineMaterial;
    public Color lineColor = Color.white;
    public float lineWidth = 0.1f;
    public float connectionSpeed = 5f; // ���߶����ٶ�

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
        if (selectedObjects.Contains(clickedObject))
        {
            selectedObjects.Remove(clickedObject);
            HighlightObject(clickedObject, false);
            return;
        }

        selectedObjects.Add(clickedObject);
        HighlightObject(clickedObject, true);

        if (selectedObjects.Count == 2)
        {
            CreateConnection(selectedObjects[0], selectedObjects[1]);
            ClearSelection();
        }
    }

    void CreateConnection(GameObject obj1, GameObject obj2)
    {
        GameObject connectionObj = new GameObject("Connection");
        connectionObj.transform.SetParent(transform);

        LineRenderer lineRenderer = connectionObj.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;

        Vector3 startPos = obj1.transform.position;
        Vector3 endPos = obj2.transform.position;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, startPos); 

        activeConnections.Add(lineRenderer);

        Coroutine animationCoroutine = StartCoroutine(AnimateConnection(lineRenderer, startPos, endPos));
        activeAnimations.Add(animationCoroutine);

        Debug.Log($"�������� {obj1.name} �� {obj2.name}");
    }

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

        lineRenderer.SetPosition(1, end);
        Debug.Log($"�������: {start} -> {end}");
    }

    void HighlightObject(GameObject obj, bool highlight)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (highlight)
            {
                if (!obj.GetComponent<ObjectInfo>())
                {
                    obj.AddComponent<ObjectInfo>().originalMaterial = renderer.material;
                }
                renderer.material.color = Color.yellow; // ������ɫ
            }
            else
            {
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
        foreach (Coroutine animation in activeAnimations)
        {
            if (animation != null)
            {
                StopCoroutine(animation);
            }
        }
        activeAnimations.Clear();

        foreach (LineRenderer connection in activeConnections)
        {
            Destroy(connection.gameObject);
        }
        activeConnections.Clear();

        ClearSelection();

        Debug.Log("�������������");
    }

    public void CreateStaticConnection(GameObject obj1, GameObject obj2)
    {
        GameObject connectionObj = new GameObject("StaticConnection");
        connectionObj.transform.SetParent(transform);

        LineRenderer lineRenderer = connectionObj.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, obj1.transform.position);
        lineRenderer.SetPosition(1, obj2.transform.position);

        activeConnections.Add(lineRenderer);

        Debug.Log($"�Ѵ�����̬���� {obj1.name} �� {obj2.name}");
    }
}

// Using ChatGPT to help write code
public class ObjectInfo : MonoBehaviour
{
    public Material originalMaterial;
}