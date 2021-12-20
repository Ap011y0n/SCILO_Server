using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatePos : MonoBehaviour
{
    private Vector3 lastpos;
    private GameObject server;
    private Quaternion lastRot;
    // Start is called before the first frame update
    void Start()
    {
        server = GameObject.Find("server");
        lastpos = transform.position;
        lastRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (lastpos != transform.position || lastRot != transform.rotation)
        {
            for (int i = 0; i < server.GetComponent<ServerUDP>().DynamicGameObjects.Count; i++)
            {
                if (server.GetComponent<ServerUDP>().DynamicGameObjects[i].getGO() == gameObject)
                {
                    server.GetComponent<ServerUDP>().DynamicGameObjects[i].position = transform.position;
                    server.GetComponent<ServerUDP>().DynamicGameObjects[i].rotation = transform.rotation;
                    server.GetComponent<ServerUDP>().DynamicGameObjects[i].setMod(true);
                }
            }
            lastpos = transform.position;
        }
    }
}
