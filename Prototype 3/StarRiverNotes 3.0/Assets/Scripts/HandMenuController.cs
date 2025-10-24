using UnityEngine;
using UnityEngine.InputSystem;

public class HandMenuController : MonoBehaviour
{
    [Header("要控制的菜单")]
    [Tooltip("在捏合时要显示/隐藏的手腕菜单")]
    public GameObject wristMenuCanvas;

    [Header("手势输入动作")]
    [Tooltip("引用左手 'Pinch' 动作")]
    public InputActionReference leftHandPinchAction;

    // isMenuVisible 字段依然有用，用于记录当前状态
    private bool isMenuVisible = false;

    void OnEnable()
    {
        if (leftHandPinchAction != null)
        {
            // 【核心修改】我们现在只监听 Performed 事件
            leftHandPinchAction.action.performed += ToggleMenu;
        }

        // 确保菜单初始是隐藏的
        if (wristMenuCanvas != null)
        {
            wristMenuCanvas.SetActive(false);
            isMenuVisible = false; // 同步状态变量
        }
    }

    void OnDisable()
    {
        if (leftHandPinchAction != null)
        {
            // 同样，只注销 Performed 事件
            leftHandPinchAction.action.performed -= ToggleMenu;
        }
    }

    /// <summary>
    /// 【核心修改】这个新方法会在每次触发Pinch动作时，反转菜单的显示状态
    /// </summary>
    private void ToggleMenu(InputAction.CallbackContext context)
    {
        // 反转状态变量
        isMenuVisible = !isMenuVisible;

        // 根据新的状态来设置菜单的激活状态
        if (wristMenuCanvas != null)
        {
            wristMenuCanvas.SetActive(isMenuVisible);
        }

        Debug.Log("ToggleMenu called. Menu is now: " + (isMenuVisible ? "Visible" : "Hidden"));
    }
}