using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class RaycastTester : MonoBehaviour
{
    public XRRayInteractor rayInteractor;

    void Start()
    {
        // 启动时获取XR Ray Interactor组件
        //rayInteractor = GetComponent<XRRayInteractor>();
        if (rayInteractor == null)
        {
            Debug.LogError("RaycastTester: XRRayInteractor component not found on this GameObject!");
        }
    }

    void LateUpdate() 
    {
        if (rayInteractor == null) return;
        bool success = rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hitInfo);
        if (success)
        {
            Debug.Log("RaycastTester SUCCESS in LateUpdate: Hit object named: " + hitInfo.collider.name);
        }
    }
}