using UnityEngine;
using UnityEngine.UI;

public class ClickableObject : MonoBehaviour
{
    public string st;
    public Text tt;
    public InputField ln;
    void OnMouseDown()
    {
        ConnectionManager.Instance.OnObjectClicked(gameObject);
        Debug.LogError(transform.name);

        tt.text = PlayerPrefs.GetString(st);
        ln.onValueChanged.RemoveAllListeners();
        ln.onValueChanged.AddListener((value) =>
        {
           tt.text = value;
           PlayerPrefs.SetString(st,value);
        });
    }
}