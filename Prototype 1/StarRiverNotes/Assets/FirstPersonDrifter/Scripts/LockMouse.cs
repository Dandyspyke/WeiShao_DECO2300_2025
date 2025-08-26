// by @torahhorse

using UnityEngine;
using System.Collections;

public class LockMouse : MonoBehaviour
{
    private bool key;
	void Start()
	{
        key = true;
		LockCursor(key);
        key = !key;
	}

    void Update()
    {
    	// lock when mouse is clicked
    	//if( Input.GetMouseButtonDown(0) && Time.timeScale > 0.0f )
    	//{
    	//	LockCursor(true);
    	//}
    
    	// unlock when escape is hit
        if  ( Input.GetKeyDown(KeyCode.Escape) )
        {
        	LockCursor(key);
            key = !key;
        }
    }
    
    public void LockCursor(bool lockCursor)
    {
        Cursor.lockState = lockCursor? CursorLockMode.Locked: CursorLockMode.None;
    }
}