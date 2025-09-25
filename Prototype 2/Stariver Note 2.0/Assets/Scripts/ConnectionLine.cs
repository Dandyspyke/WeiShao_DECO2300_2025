using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ConnectionLine : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public Transform target1;
    public Transform target2;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        // 如果两个目标都存在，就更新线的位置
        if (target1 != null && target2 != null)
        {
            lineRenderer.SetPosition(0, target1.position);
            lineRenderer.SetPosition(1, target2.position);
        }
    }
}