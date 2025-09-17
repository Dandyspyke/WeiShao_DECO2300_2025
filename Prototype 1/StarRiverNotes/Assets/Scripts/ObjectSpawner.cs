using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("预制件设置")]
    [Tooltip("要实例化的恒星预制件")]
    public GameObject starPrefab;

    [Tooltip("要实例化的行星预制件")]
    public GameObject planetPrefab;

    [Header("生成位置设置")]
    [Tooltip("玩家的主摄像机（头显）")]
    public Transform mainCameraTransform;

    [Tooltip("生成在摄像机前方多远的基础距离")]
    [Range(2f, 30f)]
    public float spawnDistance = 3f;

    [Tooltip("以此为半径的球体内随机生成，0表示不随机")]
    [Range(0f, 30f)]
    public float spawnRadius = 1.0f;

    // 创建恒星的公共方法，将被按钮调用
    public void SpawnStar()
    {
        SpawnObject(starPrefab);
    }

    // 创建行星的公共方法，将被按钮调用
    public void SpawnPlanet()
    {
        SpawnObject(planetPrefab);
    }

    // 核心生成逻辑
    private void SpawnObject(GameObject prefabToSpawn)
    {
        // 检查预制件和相机是否已设置，防止出错
        if (prefabToSpawn == null || mainCameraTransform == null)
        {
            Debug.LogError("预制件或主摄像机未在Inspector中设置！");
            return;
        }

        // 1. 计算基础生成点（在摄像机正前方）
        Vector3 baseSpawnPoint = mainCameraTransform.position + mainCameraTransform.forward * spawnDistance;

        // 2. 计算随机偏移量
        Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
        
        randomOffset.y = Mathf.Abs(randomOffset.y);

        // 3. 最终生成位置
        Vector3 finalSpawnPosition = baseSpawnPoint + randomOffset;

        // 4. 实例化物体
        Instantiate(prefabToSpawn, finalSpawnPosition, Quaternion.identity);

        Debug.Log($"已在 {finalSpawnPosition} 创建了一个 {prefabToSpawn.name}");
    }
}