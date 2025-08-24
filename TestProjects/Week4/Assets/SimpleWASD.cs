using UnityEngine;

public class SimpleWASD : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        Vector3 movement = Vector3.zero;
        
        // W / S 前后
        if (Input.GetKey(KeyCode.W)) movement.z += 1f;
        if (Input.GetKey(KeyCode.S)) movement.z -= 1f;
        
        // A / D 左右转向
        if (Input.GetKey(KeyCode.A)) transform.Rotate(0, -90f * Time.deltaTime, 0);
        if (Input.GetKey(KeyCode.D)) transform.Rotate(0, 90f * Time.deltaTime, 0);
        
        // 只有需要移动时才 Translate
        if (movement != Vector3.zero)
        {
            transform.Translate(movement.normalized * moveSpeed * Time.deltaTime, Space.Self);
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);
    }
}
