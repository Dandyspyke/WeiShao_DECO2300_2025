using UnityEngine;

public class ProximityHighlighter : MonoBehaviour
{
    public float proximityDistance = 3f;

    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    private Renderer objectRenderer;
    private bool isPlayerNearby = false;

    private GameObject player;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer)
        {
            // 注意：.material 会实例化材质，教学演示 OK
            objectRenderer.material.color = normalColor;
        }

        // 优先通过 Tag 找
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[ProximityHighlighter] 未找到 Tag=Player 的物体。请先给玩家打 Tag，或在 Inspector 手动拖引用（见下方可选做法）。");
        }
    }

    void Update()
    {
        if (player == null) return; // 没玩家引用就先不执行，避免空指针

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        bool wasNearby = isPlayerNearby;
        isPlayerNearby = distanceToPlayer <= proximityDistance;

        if (objectRenderer == null) return;

        if (isPlayerNearby && !wasNearby)
        {
            objectRenderer.material.color = highlightColor;
            Debug.Log(gameObject.name + " 高亮！");
        }
        else if (!isPlayerNearby && wasNearby)
        {
            objectRenderer.material.color = normalColor;
            Debug.Log(gameObject.name + " 恢复正常颜色");
        }
    }
}
