using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour
{
    [HideInInspector] public bool isGrabbed = false;
    [HideInInspector] public Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // 一些更稳定的物理默认值，可按需调整
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // 投掷时更稳
        rb.mass = Mathf.Max(0.1f, rb.mass); // 避免 0
    }

    // 可选：便捷切换抓取状态
    public void SetGrabbed(bool grabbed)
    {
        isGrabbed = grabbed;
        if (grabbed)
        {
            // 抓取时关闭重力 + 运动学，避免抖动
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }
        else
        {
            // 释放时恢复物理
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}
