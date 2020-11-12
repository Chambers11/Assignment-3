using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestIndicatorRegister : MonoBehaviour
{

    [SerializeField] float destroyTimer = 20.0f;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("Register", Random.Range(0, 8));
    }
   

}
