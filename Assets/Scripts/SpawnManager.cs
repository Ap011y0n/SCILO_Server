using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpawnManager : MonoBehaviour
{
    int thresholds;
    int counter;

    public GameObject enemy;
    public List<GameObject> spawns = new List<GameObject>();
    private GameObject server;

    // Start is called before the first frame update
    void Start()
    {
        thresholds = 0;
        counter = 0;
        server = GameObject.Find("server");
    }

    // Update is called once per frame
    void Update()
    {
        /*if(counter >= 3000)
        {
            SpawnEnemies();
            counter = 0;
        }

        counter++;*/
    }

    public void AddThreshold()
    {
        thresholds++;
        SpawnEnemies();
    }

    public int GetThreshold()
    {
        return thresholds;
    }

    public void SetCounter(int count)
    {
        counter = count;
    }

    public void SpawnEnemies()
    {
        Vector3 pos;

        switch (thresholds)
        {
            case 1:
                pos = spawns[0].transform.position;
                SpawnEnemy(pos);
                break;
            case 2:
                pos = spawns[1].transform.position;
                SpawnEnemy(pos);
                pos = spawns[2].transform.position;
                SpawnEnemy(pos);
                break;
            case 3:
                pos = spawns[2].transform.position;
                SpawnEnemy(pos);
                pos = spawns[3].transform.position;
                SpawnEnemy(pos);
                break;
            case 4:
                pos = spawns[3].transform.position;
                SpawnEnemy(pos);
                pos = spawns[4].transform.position;
                SpawnEnemy(pos);
                pos = spawns[5].transform.position;
                SpawnEnemy(pos);
                break;
            case 5:
                pos = spawns[6].transform.position;
                SpawnEnemy(pos);
                pos = spawns[7].transform.position;
                SpawnEnemy(pos);
                break;
            case 6:
                pos = spawns[7].transform.position;
                SpawnEnemy(pos);
                pos = spawns[8].transform.position;
                SpawnEnemy(pos);
                pos = spawns[9].transform.position;
                SpawnEnemy(pos);
                pos = spawns[10].transform.position;
                SpawnEnemy(pos);

                break;
            case 7:
                pos = spawns[8].transform.position;
                SpawnEnemy(pos);
                pos = spawns[9].transform.position;
                SpawnEnemy(pos);
                pos = spawns[11].transform.position;
                SpawnEnemy(pos);
                pos = spawns[12].transform.position;
                SpawnEnemy(pos);
                break;
            case 8:
                pos = spawns[12].transform.position;
                SpawnEnemy(pos);
                pos = spawns[13].transform.position;
                SpawnEnemy(pos);
                pos = spawns[14].transform.position;
                SpawnEnemy(pos);
                break;
            case 9:
                pos = spawns[14].transform.position;
                SpawnEnemy(pos);
                pos = spawns[15].transform.position;
                SpawnEnemy(pos);
                pos = spawns[16].transform.position;
                SpawnEnemy(pos);
                pos = spawns[17].transform.position;
                SpawnEnemy(pos);
                pos = spawns[18].transform.position;
                SpawnEnemy(pos);
                break;
            case 10:
                pos = spawns[17].transform.position;
                SpawnEnemy(pos);
                pos = spawns[18].transform.position;
                SpawnEnemy(pos);
                pos = spawns[19].transform.position;
                SpawnEnemy(pos);
                pos = spawns[20].transform.position;
                SpawnEnemy(pos);
                pos = spawns[21].transform.position;
                SpawnEnemy(pos);
                pos = spawns[22].transform.position;
                SpawnEnemy(pos);
                break;
        }
    }

    public void SpawnEnemy(Vector3 pos)
    {
        CustomClasses.Spawn newSpawn = new CustomClasses.Spawn();
        GameObject NewEnemy = Instantiate(enemy, pos, Quaternion.identity);
        newSpawn.name = "1";
        newSpawn.setGO(NewEnemy);
        newSpawn.position = NewEnemy.transform.position;
        newSpawn.rotation = NewEnemy.transform.rotation;
        newSpawn.guid = Guid.NewGuid().ToString();
        server.GetComponent<ServerUDP>().waitingforspawn.Add(newSpawn);

        NewEnemy.GetComponent<enemy>().guid = newSpawn.guid;


        CustomClasses.SceneObject newSceneObject = new CustomClasses.SceneObject();
        newSceneObject.setGO(newSpawn.getGO());
        newSceneObject.name = newSpawn.getGO().name;
        newSceneObject.position = newSpawn.getGO().transform.position;
        newSceneObject.setModDate(System.DateTime.UtcNow.Second);
        newSceneObject.setMod(false);
        newSceneObject.guid = newSpawn.guid;
        server.GetComponent<ServerUDP>().DynamicGameObjects.Add(newSceneObject);

        Debug.Log("Spawning enemy..");
    }
}
