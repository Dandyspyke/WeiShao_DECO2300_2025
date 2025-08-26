using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ani : MonoBehaviour
{
    public void kai()
    {

        transform.GetComponent<Animator>().SetBool("bai", true);
    }
    public void guan()
    {

        transform.GetComponent<Animator>().SetBool("bai", false);
    }
}
