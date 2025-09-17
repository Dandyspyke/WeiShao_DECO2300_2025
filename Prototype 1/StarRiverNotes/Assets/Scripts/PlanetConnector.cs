using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

public class PlanetConnector : MonoBehaviour
{
    [Header("输入设置")]
    public InputActionReference connectActionReference;

    [Header("核心组件引用")]
    [Tooltip("场景中的 Near-Far Interactor，用于检查选择状态")]
    public NearFarInteractor nearFarInteractor;

    [Tooltip("Near-Far Interactor 下的远距离投射器 (Curve Interaction Caster)")]
    public CurveInteractionCaster farCaster;

    public GameObject connectionLinePrefab;

    private bool isConnecting = false;
    private Transform connectionStartPlanet;
    private LineRenderer tempConnectionLine;

    void OnEnable()
    {
        connectActionReference.action.performed += OnConnectPressed;
        connectActionReference.action.canceled += OnConnectReleased;
    }

    void OnDisable()
    {
        connectActionReference.action.performed -= OnConnectPressed;
        connectActionReference.action.canceled -= OnConnectReleased;
    }

    private void OnConnectPressed(InputAction.CallbackContext context)
    {
        if (nearFarInteractor == null || farCaster == null) return;
        if (nearFarInteractor.hasSelection) return;

        // --- 【最终核心修复：手动执行射线检测】 ---
        // 我们从 Far Caster 获取射线的起点、方向和设置
        Transform casterTransform = farCaster.transform;
        LayerMask casterMask = farCaster.raycastMask; // 获取Caster的物理层掩码
        float maxDistance = 3000f; // 您可以根据需要调整，或者尝试读取Caster的距离属性

        if (Physics.Raycast(casterTransform.position, casterTransform.forward, out RaycastHit hitInfo, maxDistance, casterMask))
        {
            Debug.Log("[Manual Raycast] SUCCESS: Hit object: " + hitInfo.collider.name);

            if (hitInfo.collider.CompareTag("Planet"))
            {
                isConnecting = true;
                connectionStartPlanet = hitInfo.transform;
                GameObject lineObj = Instantiate(connectionLinePrefab, Vector3.zero, Quaternion.identity);
                tempConnectionLine = lineObj.GetComponent<LineRenderer>();
                Debug.Log("开始连接，已选中行星：" + connectionStartPlanet.name);
            }
        }
        else
        {
            Debug.Log("[Manual Raycast] FAIL: No hit detected.");
        }
    }
    
    void LateUpdate()
    {
        if (isConnecting && tempConnectionLine != null && farCaster != null)
        {
            tempConnectionLine.SetPosition(0, connectionStartPlanet.position);
            
            Transform casterTransform = farCaster.transform;
            LayerMask casterMask = farCaster.raycastMask;
            float maxDistance = 3000f;

            if (Physics.Raycast(casterTransform.position, casterTransform.forward, out RaycastHit hitInfo, maxDistance, casterMask))
            {
                tempConnectionLine.SetPosition(1, hitInfo.point);
            }
            else
            {
                tempConnectionLine.SetPosition(1, casterTransform.position + casterTransform.forward * maxDistance);
            }
        }
    }

    private void OnConnectReleased(InputAction.CallbackContext context)
    {
        if (!isConnecting || farCaster == null) return;

        bool success = false;
        
        Transform casterTransform = farCaster.transform;
        LayerMask casterMask = farCaster.raycastMask;
        float maxDistance = 3000f;

        if (Physics.Raycast(casterTransform.position, casterTransform.forward, out RaycastHit hitInfo, maxDistance, casterMask))
        {
            if (hitInfo.collider.CompareTag("Star"))
            {
                Debug.Log("连接成功！");
                connectionStartPlanet.SetParent(hitInfo.transform);

                var permanentLine = tempConnectionLine.GetComponent<ConnectionLine>();
                if (permanentLine != null)
                {
                    permanentLine.target1 = connectionStartPlanet;
                    permanentLine.target2 = hitInfo.transform;
                }
                
                success = true;
            }
        }
        
        if (!success)
        {
            Debug.Log("连接取消。");
            if(tempConnectionLine != null)
                Destroy(tempConnectionLine.gameObject);
        }

        isConnecting = false;
        connectionStartPlanet = null;
        tempConnectionLine = null;
    }
}