using UnityEngine;

public class PlanetIdentifier : MonoBehaviour
{
    [Tooltip("每个星球的唯一ID，在创建时自动生成")]
    public string planetID;

    void Awake()
    {
        // 如果这个星球还没有ID（通常是在第一次被创建时），就给它生成一个
        if (string.IsNullOrEmpty(planetID))
        {
            planetID = System.Guid.NewGuid().ToString();
        }
    }
}