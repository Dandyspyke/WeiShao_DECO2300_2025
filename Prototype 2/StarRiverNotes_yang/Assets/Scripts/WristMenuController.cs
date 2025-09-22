using UnityEngine;
using UnityEngine.InputSystem; // 引入新的输入系统命名空间

public class WristMenuController : MonoBehaviour
{
    [Tooltip("要控制显示/隐藏的菜单Canvas对象")]
    public GameObject menuCanvas;

    [Tooltip("引用输入操作(Input Action)来触发菜单")]
    public InputActionReference menuToggleActionReference;

    void Awake()
    {
        // 确保一开始菜单是隐藏的
        if (menuCanvas != null)
        {
            menuCanvas.SetActive(false);
        }
    }

    void OnEnable()
    {
        // 注册事件：当玩家执行绑定的操作时，调用ToggleMenu方法
        if (menuToggleActionReference != null)
        {
            menuToggleActionReference.action.performed += ToggleMenu;
        }
    }

    void OnDisable()
    {
        // 注销事件：当脚本被禁用或销毁时，移除监听，防止内存泄漏
        if (menuToggleActionReference != null)
        {
            menuToggleActionReference.action.performed -= ToggleMenu;
        }
    }

    /// <summary>
    /// 切换菜单的激活状态
    /// </summary>
    /// <param name="context">输入操作的回调上下文，我们这里用不到但方法签名需要</param>
    private void ToggleMenu(InputAction.CallbackContext context)
    {
        if (menuCanvas != null)
        {
            // 将菜单的激活状态设置为当前状态的反面（开 -> 关，关 -> 开）
            menuCanvas.SetActive(!menuCanvas.activeSelf);
        }
    }
}