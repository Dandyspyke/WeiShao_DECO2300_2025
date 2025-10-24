using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(PlanetIdentifier), typeof(XRSimpleInteractable))]
public class PlanetInteractionSetup : MonoBehaviour
{
    private XRSimpleInteractable simpleInteractable;
    private PlanetIdentifier planetIdentifier;

    void Awake()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
        planetIdentifier = GetComponent<PlanetIdentifier>();
        NotebookManager notebookManager = FindObjectOfType<NotebookManager>();

        if (notebookManager != null)
        {
            // 【核心修改】将监听的事件从 selectEntered 改为 activated
            simpleInteractable.activated.AddListener((activateEventArgs) => 
            {
                notebookManager.OpenPanelForPlanet(planetIdentifier);
            });
        }
        else
        {
            Debug.LogError("场景中没有找到 NotebookManager！星球的点击事件无法注册。");
        }
    }
}