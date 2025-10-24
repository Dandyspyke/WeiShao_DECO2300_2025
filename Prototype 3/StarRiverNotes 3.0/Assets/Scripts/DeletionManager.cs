using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

public class DeletionManager : MonoBehaviour
{
    [Header("核心组件引用")]
    [Tooltip("【右手】的交互器，用于射线检测")]
    public NearFarInteractor rightHandInteractor;
    [Tooltip("【右手】的远距离投射器")]
    public CurveInteractionCaster rightFarCaster;
    [Tooltip("【右手】的B键动作引用 (用于删除)")]
    public InputActionReference deleteAction;
    [Tooltip("场景中的 NotebookManager，用于删除数据")]
    public NotebookManager notebookManager;

    [Header("删除设置")]
    [Tooltip("需要按住B键多长时间才能触发删除")]
    public float deleteHoldDuration = 1.0f;

    private Coroutine deleteHoldCoroutine;

    void OnEnable()
    {
        deleteAction.action.started += OnDeleteButtonPressed;
        deleteAction.action.canceled += OnDeleteButtonReleased;
    }

    void OnDisable()
    {
        deleteAction.action.started -= OnDeleteButtonPressed;
        deleteAction.action.canceled -= OnDeleteButtonReleased;
    }

    private void OnDeleteButtonPressed(InputAction.CallbackContext context)
    {
        // 按下B键时，开始计时
        deleteHoldCoroutine = StartCoroutine(CheckForDeleteHold());
    }

    private void OnDeleteButtonReleased(InputAction.CallbackContext context)
    {
        // 如果在计时完成前松开按键，则取消删除
        if (deleteHoldCoroutine != null)
        {
            StopCoroutine(deleteHoldCoroutine);
            deleteHoldCoroutine = null;
        }
    }

    private IEnumerator CheckForDeleteHold()
    {
        // 等待指定的时间
        yield return new WaitForSeconds(deleteHoldDuration);

        // 如果协程仍在运行（即按键没有被提前松开），则执行删除
        Debug.Log("右手B键长按已满足！");
        PerformDeletion();
        deleteHoldCoroutine = null;
    }

    private void PerformDeletion()
    {
        PlanetIdentifier planetToDelete = GetPlanetUnderRay();
        if (planetToDelete != null)
        {
            Debug.Log("准备删除星球: " + planetToDelete.name);

            // 1. 调用 NotebookManager 删除关联数据
            if (notebookManager != null)
            {
                notebookManager.DeletePlanetDataByID(planetToDelete.planetID);
            }

            // 2. 销毁星球的游戏对象
            Destroy(planetToDelete.gameObject);
        }
    }

    private PlanetIdentifier GetPlanetUnderRay()
    {
        if (rightFarCaster == null) return null;

        Transform casterTransform = rightFarCaster.transform;
        if (Physics.Raycast(casterTransform.position, casterTransform.forward, out RaycastHit hitInfo, rightFarCaster.castDistance, rightFarCaster.raycastMask))
        {
            return hitInfo.collider.GetComponent<PlanetIdentifier>();
        }
        return null;
    }
}