using UnityEngine;
using UnityEngine.UI;

public class ClickableObject : MonoBehaviour
{
    public string st;
    public Text tt;
    void OnMouseDown()
    {
        ConnectionManager.Instance.OnObjectClicked(gameObject);
        Debug.LogError(transform.name);
        tt.text = st;


    }
}