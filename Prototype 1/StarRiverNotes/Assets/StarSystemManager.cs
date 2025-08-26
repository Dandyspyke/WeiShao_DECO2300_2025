using System.Collections.Generic;
using UnityEngine;

public class StarSystemManager : MonoBehaviour
{
    public GameObject starPrefab;
    public LineRenderer linePrefab;
    public int starCount = 10;
    public float spawnRadius = 5f;

    private List<GameObject> stars = new List<GameObject>();
    private List<Star> selectedStars = new List<Star>();
    private LineRenderer currentLine;

    void Start()
    {
        GenerateStarSystem();
    }

    void GenerateStarSystem()
    {
        for (int i = 0; i < starCount; i++)
        {
            Vector3 randomPos = Random.onUnitSphere * spawnRadius;
            GameObject star = Instantiate(starPrefab, randomPos, Quaternion.identity, transform);
            star.name = $"Star_{i}";

            Star starComponent = star.GetComponent<Star>();
            if (starComponent == null)
            {
                starComponent = star.AddComponent<Star>();
            }

            starComponent.Initialize(this, i);
            stars.Add(star);
        }
    }

    public void OnStarClicked(Star star)
    {
        if (selectedStars.Contains(star))
        {
            // 取消选择
            selectedStars.Remove(star);
            star.SetSelected(false);
        }
        else
        {
            // 选择新星体
            selectedStars.Add(star);
            star.SetSelected(true);
        }

        UpdateConnections();
    }

    void UpdateConnections()
    {
        // 清除所有现有连线
        foreach (Transform child in transform)
        {
            if (child.name == "Connection")
            {
                Destroy(child.gameObject);
            }
        }

        // 创建新连线
        for (int i = 0; i < selectedStars.Count - 1; i++)
        {
            CreateConnection(selectedStars[i].transform.position, selectedStars[i + 1].transform.position);
        }
    }

    void CreateConnection(Vector3 from, Vector3 to)
    {
        LineRenderer connection = Instantiate(linePrefab, transform);
        connection.name = "Connection";
        connection.positionCount = 2;
        connection.SetPosition(0, from);
        connection.SetPosition(1, to);
    }

    public void ClearAllConnections()
    {
        selectedStars.ForEach(star => star.SetSelected(false));
        selectedStars.Clear();
        UpdateConnections();
    }
}

public class Star : MonoBehaviour
{
    private StarSystemManager manager;
    private int starId;
    private bool isSelected;
    private Material originalMaterial;
    public Material selectedMaterial;

    public void Initialize(StarSystemManager manager, int id)
    {
        this.manager = manager;
        this.starId = id;
        originalMaterial = GetComponent<Renderer>().material;
    }

    void OnMouseDown()
    {
        manager.OnStarClicked(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        GetComponent<Renderer>().material = selected ? selectedMaterial : originalMaterial;
    }
}