using UnityEngine;

public class PlayerGrabber : MonoBehaviour
{
    [Header("Grab Settings")]
    public float grabDistance = 3f;         // 抓取判定半径
    public Transform holdPoint;             // 手持点（建议拖 Player/HoldPoint）
    public float holdSmooth = 20f;          // 抓取时插值跟随（防抖）
    
    [Header("Throw Settings")]
    public float throwImpulse = 10f;        // 投掷冲量
    public float upBias = 0.3f;             // 向上抛物线偏置（0~0.5）

    private Grabbable grabbed;              // 当前手里物体
    private Collider playerCollider;        // 用于在抓取时临时忽略碰撞（可选）
    private bool ignoringCollision = false;

    void Awake()
    {
        playerCollider = GetComponent<Collider>(); // 如果 Player 有 Collider（如 CapsuleCollider）
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (grabbed == null)
                TryGrabNearest();
            else
                ThrowObject();
        }

        // 抓取时让物体平滑跟随 HoldPoint（避免刚体+父子关系抖动）
        if (grabbed != null && holdPoint != null)
        {
            grabbed.transform.position = Vector3.Lerp(
                grabbed.transform.position,
                holdPoint.position,
                Time.deltaTime * holdSmooth
            );
            grabbed.transform.rotation = Quaternion.Lerp(
                grabbed.transform.rotation,
                holdPoint.rotation,
                Time.deltaTime * holdSmooth
            );
        }
    }

    void TryGrabNearest()
    {
        // 找出半径内最近的 Grabbable
        Grabbable[] all = FindObjectsOfType<Grabbable>();
        Grabbable best = null;
        float bestDist = Mathf.Infinity;

        foreach (var g in all)
        {
            if (g.isGrabbed) continue;
            float d = Vector3.Distance(transform.position, g.transform.position);
            if (d <= grabDistance && d < bestDist)
            {
                best = g;
                bestDist = d;
            }
        }

        if (best != null)
            Grab(best);
        else
            Debug.Log("附近没有可抓取物体。");
    }

    void Grab(Grabbable target)
    {
        grabbed = target;
        grabbed.SetGrabbed(true);

        // 立即把物体放到手持点附近，避免瞬间穿模
        if (holdPoint != null)
        {
            grabbed.transform.position = holdPoint.position;
            grabbed.transform.rotation = holdPoint.rotation;
        }

        // （可选）抓取时忽略与玩家的碰撞，避免晃动
        IgnoreCollisionWithPlayer(grabbed, true);

        Debug.Log("Grabbed: " + grabbed.name);
    }

    void ThrowObject()
    {
        if (grabbed == null) return;

        // 释放物体的物理锁定
        grabbed.SetGrabbed(false);

        // 计算投掷方向（带一点向上）
        Vector3 dir = (transform.forward + Vector3.up * upBias).normalized;

        // 施加冲量
        grabbed.rb.AddForce(dir * throwImpulse, ForceMode.Impulse);

        // 恢复与玩家的碰撞
        IgnoreCollisionWithPlayer(grabbed, false);

        Debug.Log("Threw: " + grabbed.name);
        grabbed = null;
    }

    void IgnoreCollisionWithPlayer(Grabbable g, bool ignore)
    {
        if (!playerCollider) return;
        Collider objCol = g.GetComponent<Collider>();
        if (!objCol) return;

        Physics.IgnoreCollision(playerCollider, objCol, ignore);
        ignoringCollision = ignore;
    }

    void OnDisable()
    {
        // 保底：脚本失效时恢复碰撞
        if (grabbed && ignoringCollision)
            IgnoreCollisionWithPlayer(grabbed, false);
    }
}
