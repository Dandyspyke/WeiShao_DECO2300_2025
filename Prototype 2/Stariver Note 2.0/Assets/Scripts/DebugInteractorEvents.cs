using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // 引入XRI命名空间

public class DebugInteractorEvents : MonoBehaviour
{
    // 这个方法用于悬停，当前需求可以不用
    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        string objectName = args.interactableObject.transform.name;
        Debug.Log($"[右手射线] 悬停进入: {objectName}");
    }

    // ... 其他方法 ...

    /// <summary>
    /// 当选中(通常是按下扳机)某个物体时被调用
    /// 这个是我们这次需要的方法！
    /// </summary>
    /// <param name="args">包含了交互信息的事件参数</param>
    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        // 从事件参数中获取被选中的Interactable对象
        IXRSelectInteractable interactableObject = args.interactableObject;

        // 获取该对象的Transform，再通过Transform获取其GameObject的名称
        string objectName = interactableObject.transform.name;

        // 在控制台打印出来
        Debug.Log($"[右手射线] 按下扳机选中了: {objectName}");
    }
}