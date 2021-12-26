using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody body;
    public float jumpforce = 20f;
    public float MovSpeed = 10f;
    public GameObject gun;
    public GameObject bullet;
    public GameObject enemy;
    public GameObject otherPlayer;

    List<CustomClasses.Input> inputs = new List<CustomClasses.Input>();
    //List<string> states = new List<string>();
    private GameObject server;
    private GameObject spawnManager;
    enum State
    {
        IDLE,
        WALK_RIGHT,
        WALK_LEFT,
        JUMP
    }
    State state;

    void Start()
    {
        state = State.IDLE;
        body = this.gameObject.GetComponent<Rigidbody>();
        server = GameObject.Find("server");
        spawnManager = GameObject.Find("SpawnManager");
    }

    void changeState(State newState)
    {
        switch (newState)
        {
            case State.IDLE:
                break;
            case State.WALK_RIGHT:
                break;
            case State.WALK_LEFT:
                break;
            case State.JUMP:
                Vector3 force = new Vector3(0, jumpforce, 0);
                body.AddForce(force);
                state = State.JUMP;
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(transform.position.y <= -6.5)
        {
            Vector3 respawnPos = otherPlayer.transform.position;
            respawnPos.y += 3;
            transform.position = respawnPos;
            server.GetComponent<ServerUDP>().deathCounter++;
            server.GetComponent<ServerUDP>().addRespawn2Message = true;
        }
        
        ProcessInput();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("floor") && state == State.JUMP)
        {
            state = State.IDLE;
        }
    }

    private void ProcessInput()
    {
        //de momento, el movimiento va asi, pero habria que hacer dos estados nuevos, de moverse derecha e izquierda, teniendo en cuenta si se esta en el suelo o no
        if(Input.GetKeyDown(KeyCode.F1) && gameObject.name == "Player1")
        {
            
            Vector3 pos = transform.position;
            pos.z += 4;
            pos.x += 0.5f;

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


        }

        if (inputs.Count > 0)
        {
            List<CustomClasses.Input> tempinputs = new List<CustomClasses.Input>(inputs);
            inputs.Clear();

            foreach (CustomClasses.Input input in tempinputs)
            {

                Vector3 temp = gameObject.GetComponent<Rigidbody>().velocity;
                if (input.type != "")
                switch (input.type)
                {
                    case "KeyPressed":
                        switch (input.key)
                        {
                            case "D":
                                temp.z = MovSpeed;
                                gameObject.GetComponent<Rigidbody>().velocity = temp;
                                Debug.Log("D key Pressed");
                                state = State.WALK_RIGHT;
                                break;
                            case "A":
                                Debug.Log("A key Pressed");
                                temp.z = -MovSpeed;
                                gameObject.GetComponent<Rigidbody>().velocity = temp;
                                state = State.WALK_LEFT;
                                break;
                        }
                        break;
                    case "KeyUp":
                            Debug.Log("KEYUP");
                        switch (input.key)
                        {
                            case "D":
                                Debug.Log("D key Up");
                                temp = gameObject.GetComponent<Rigidbody>().velocity;
                                temp.z = 0;
                                gameObject.GetComponent<Rigidbody>().velocity = temp;
                                state = State.IDLE;
                                break;
                            case "A":
                                Debug.Log("A key Up");

                                temp = gameObject.GetComponent<Rigidbody>().velocity;
                                temp.z = 0;
                                gameObject.GetComponent<Rigidbody>().velocity = temp;
                                state = State.IDLE;
                                break;
                        }
                        break;
                    case "KeyDown":
                        switch (input.key)
                        {
                            case "Space":
                                if(state != State.JUMP)
                                changeState(State.JUMP);
                                break;
                        }
                        break;
                    case "MouseButtonDown":
                        switch(input.key)
                        {
                            case "0":
                                CustomClasses.Spawn newSpawn = new CustomClasses.Spawn();
                                GameObject NewBullet = Instantiate(bullet, gun.transform.position, gun.transform.rotation);
                                newSpawn.name = "0";
                                newSpawn.setGO(NewBullet);
                                newSpawn.position = NewBullet.transform.position;
                                newSpawn.rotation = NewBullet.transform.rotation;
                                newSpawn.guid = Guid.NewGuid().ToString();
                                server.GetComponent<ServerUDP>().waitingforspawn.Add(newSpawn);

                               
                                //CustomClasses.SceneObject newSceneObject = new CustomClasses.SceneObject();
                                //newSceneObject.setGO(newSpawn.getGO());
                                //newSceneObject.name = newSpawn.getGO().name;
                                //newSceneObject.position = newSpawn.getGO().transform.position;
                                //newSceneObject.setModDate(System.DateTime.UtcNow.Second);
                                //newSceneObject.setMod(false);
                                //server.GetComponent<ServerUDP>().AllGameObjects.Add(newSceneObject);
                                

                                break;
                        }
                        break;
                }
                //if(input.type != "MouseButtonDown")
                //for (int i = 0; i < server.GetComponent<ServerUDP>().AllGameObjects.Count; i++)
                //{
                //    if (server.GetComponent<ServerUDP>().AllGameObjects[i].name == gameObject.name)
                //    {
                //        server.GetComponent<ServerUDP>().AllGameObjects[i].position = transform.position;
                //        server.GetComponent<ServerUDP>().AllGameObjects[i].setMod(true);
                //    }
                //}
                
            }
            
            
        }
     
        //if (Input.GetKey(KeyCode.D))
        //{
        //    Vector3 temp = new Vector3(0, 0, MovSpeed * Time.deltaTime);
        //    gameObject.transform.position += temp;
        //}
        //if (Input.GetKey(KeyCode.A))
        //{
        //    Vector3 temp = new Vector3(0, 0, -MovSpeed * Time.deltaTime);
        //    gameObject.transform.position += temp;
        //}
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Instantiate(bullet, gun.transform.position, gun.transform.rotation);

        //}
        //if (Input.GetKeyDown(KeyCode.Space) && state != State.JUMP)
        //{
        //    changeState(State.JUMP);

        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("EnemyBullet"))
        {
            Vector3 respawnPos = otherPlayer.transform.position;
            respawnPos.y += 3;
            transform.position = respawnPos;
            server.GetComponent<ServerUDP>().deathCounter++;
            server.GetComponent<ServerUDP>().addRespawn2Message = true;
        }

        if (other.gameObject.CompareTag("Threshold"))
        {
            Debug.Log("Threshold " + spawnManager.GetComponent<SpawnManager>().GetThreshold() + " passed");
            spawnManager.GetComponent<SpawnManager>().AddThreshold();
            spawnManager.GetComponent<SpawnManager>().SetCounter(2950);
            other.gameObject.SetActive(false);
        }
    }
    public void AddInput(CustomClasses.Input input)
    {
        inputs.Add(input);
        //ProcessInput();
    }
    /*public void AddStates(string state)
    {
        states.Add(state);
    }
    public void ClearStates()
    {
        states.Clear();
    }*/
    public string GetPlayerState()
    {
        return state.ToString();
    }
}
