using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public ServerUDP server;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.F2))
        //{
        //    server.victory = true;
        //}
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
            server.victory = true;
    }
}
