using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy : MonoBehaviour
{
    private GameObject server;
    private GameObject[] players;
    public string guid;
    public int gunCD = 3;
    private System.DateTime time;
    public GameObject bullet;

    // Start is called before the first frame update
    void Start()
    {
        server = GameObject.Find("server");
        players = GameObject.FindGameObjectsWithTag("Player");
        time = System.DateTime.UtcNow;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = 8;
        int targeted_player = -1;
        for (int i = 0; i < players.Length; i++)
        {
            float playerdistance = Mathf.Abs(transform.position.z - players[i].transform.position.z);
            if (distance > playerdistance)
            {
                distance = playerdistance;
                targeted_player = i;
            }
        }
        if(targeted_player != -1)
        {
            if(transform.position.z - players[targeted_player].transform.position.z > 0)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            if((System.DateTime.UtcNow - time).Seconds > 2)
            {
                time = System.DateTime.UtcNow;
                CustomClasses.Spawn newSpawn = new CustomClasses.Spawn();
                GameObject NewBullet = Instantiate(bullet, transform.position, transform.rotation);
                newSpawn.name = "2";
                newSpawn.setGO(NewBullet);
                newSpawn.position = NewBullet.transform.position;
                newSpawn.rotation = NewBullet.transform.rotation;
                newSpawn.guid = Guid.NewGuid().ToString();
                server.GetComponent<ServerUDP>().waitingforspawn.Add(newSpawn);
            }

        }
      
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            CustomClasses.Remove newRemoval = new CustomClasses.Remove();
            newRemoval.name = gameObject.name;

            //for(int i = 0; i < server.GetComponent<ServerUDP>().DynamicGameObjects.Count; i++)
            //{
            //    if (server.GetComponent<ServerUDP>().DynamicGameObjects[i].getGO() == gameObject)
            //    {

            //        guid = server.GetComponent<ServerUDP>().DynamicGameObjects[i].guid;
            //        //server.GetComponent<ServerUDP>().DynamicGameObjects.RemoveAt(i);
            //        //i--;
            //        newRemoval.guid = guid;

            //    }
            //}
            newRemoval.guid = guid;
            server.GetComponent<ServerUDP>().waitingforremoval.Add(newRemoval);
            Destroy(gameObject);
        }
    }
}
