using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SwarmerTarget : MonoBehaviour
{
    private static SwarmerTarget s_instance;
    public static SwarmerTarget Instance => s_instance;

    protected void Awake()
    {
        s_instance = this;
    }
}
