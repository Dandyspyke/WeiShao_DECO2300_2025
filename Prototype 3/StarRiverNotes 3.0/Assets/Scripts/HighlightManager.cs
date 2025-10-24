using UnityEngine;
using HighlightingSystem; // 【必需】引入Highlighting System的命名空间
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class HighlightManager : MonoBehaviour
{
    [Header("左右手交互器引用")]
    [Tooltip("场景中的左手 Near-Far Interactor")]
    public NearFarInteractor leftHandInteractor;

    [Tooltip("场景中的右手 Near-Far Interactor")]
    public NearFarInteractor rightHandInteractor;

    // 用于存储当前正被高亮的那个物体的 Highlighter 组件
    private Highlighter currentlyHighlighted;

    void OnEnable()
    {
        // 为左手柄的悬浮事件注册监听
        if (leftHandInteractor != null)
        {
            leftHandInteractor.hoverEntered.AddListener(OnHoverEntered);
            leftHandInteractor.hoverExited.AddListener(OnHoverExited);
        }

        // 为右手柄的悬浮事件注册监听
        if (rightHandInteractor != null)
        {
            rightHandInteractor.hoverEntered.AddListener(OnHoverEntered);
            rightHandInteractor.hoverExited.AddListener(OnHoverExited);
        }
    }

    void OnDisable()
    {
        // 在脚本禁用时，注销监听，防止内存泄漏
        if (leftHandInteractor != null)
        {
            leftHandInteractor.hoverEntered.RemoveListener(OnHoverEntered);
            leftHandInteractor.hoverExited.RemoveListener(OnHoverExited);
        }

        if (rightHandInteractor != null)
        {
            rightHandInteractor.hoverEntered.RemoveListener(OnHoverEntered);
            rightHandInteractor.hoverExited.RemoveListener(OnHoverExited);
        }
    }

    /// <summary>
    /// 当任何一个手柄的射线开始悬浮在一个物体上时调用
    /// </summary>
    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        // 从事件参数中获取被悬浮的物体
        GameObject hoveredObject = args.interactableObject.transform.gameObject;

        // 尝试从该物体上获取 Highlighter 组件
        Highlighter highlighter = hoveredObject.GetComponent<Highlighter>();

        // 如果找到了 Highlighter 组件
        if (highlighter != null)
        {
            // 在高亮新物体之前，先确保关闭了上一个高亮（以防万一 HoverExited 事件丢失）
            if (currentlyHighlighted != null)
            {
                currentlyHighlighted.ConstantOff();
            }

            // 打开新物体的高光效果
            // ConstantOn() 会让高光持续显示，比 HoverOn() 效果更稳定
            highlighter.ConstantOn();

            // 记录下当前正在高亮的组件
            currentlyHighlighted = highlighter;
        }
    }

    /// <summary>
    /// 当任何一个手柄的射线离开一个物体时调用
    /// </summary>
    private void OnHoverExited(HoverExitEventArgs args)
    {
        GameObject exitedObject = args.interactableObject.transform.gameObject;
        Highlighter highlighter = exitedObject.GetComponent<Highlighter>();

        // 检查离开的这个物体是否就是我们当前记录的那个正在高亮的物体
        // 这样做可以防止因为射线快速移动导致的状态错乱
        if (highlighter != null && highlighter == currentlyHighlighted)
        {
            // 关闭高光
            highlighter.ConstantOff();

            // 清空记录
            currentlyHighlighted = null;
        }
    }
}