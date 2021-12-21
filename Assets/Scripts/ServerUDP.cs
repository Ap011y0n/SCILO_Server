using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace CustomClasses
{
    #region classes
    [System.Serializable]
    public class WelcomeMessage
    {
        public string GUIDplayer1;
        public string GUIDplayer2;
        public string GUIDgun1;
        public string GUIDgun2;
        public List<Spawn> spawns = new List<Spawn>();
        public List<SceneObject> objects = new List<SceneObject>();

    }
    [System.Serializable]
    public class Message
    {
        public int ACK = -1;
        public List<string> messageTypes = new List<string>();
        public List<SceneObject> objects = new List<SceneObject>();
        public List<Input> inputs = new List<Input>();
        public List<Remove> removals = new List<Remove>();
        public List<Spawn> spawns = new List<Spawn>();

        public void addType(string type)
        {
            if (!messageTypes.Contains(type))
                messageTypes.Add(type);
        }
    }
    
    [System.Serializable]
    public class SceneObject
    {
        public string name;
        private GameObject gameObject;
        private int lastModified;
        private bool modified;
        public Vector3 position;
        public Quaternion rotation;
        public string guid;
        public GameObject getGO()
        {
            return gameObject;
        }
        public void setGO(GameObject go)
        {
            gameObject = go;
        }
        public int getModDate()
        {
            return lastModified;
        }
        public void setModDate(int mod)
        {
            lastModified = mod;
        }
        public bool CheckMod()
        {
            return modified;
        }
        public void setMod(bool mod)
        {
            modified = mod;
        }
    }
    [System.Serializable]
    public class Input
    {
        public string key;
        public string type;
    }
    [System.Serializable]
    public class Spawn
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        private GameObject Obj;
        public string guid;
        public void setGO(GameObject go)
        {
            Obj = go;
        }
        public GameObject getGO()
        {
            return Obj;
        }
    }
    [System.Serializable]
    public class Remove
    {
        public string name;
        private GameObject Obj;
        public string guid;

        public void setGO(GameObject go)
        {
            Obj = go;
        }
        public GameObject getGO()
        {
            return Obj;
        }
    }
    #endregion
    
}


public class ServerUDP : MonoBehaviour
{
    class Client
    {
        public Socket socket;
        public EndPoint remote;
        public bool active = false;
    }
    public List<CustomClasses.SceneObject> DynamicGameObjects = new List<CustomClasses.SceneObject>();
    public GameObject player1;
    public GameObject player2;

    Client client1 = new Client();
    Client client2 = new Client();

    Thread threadstart1 = null;
    Thread threadstart2 = null;
    Thread sendThread = null;
    Thread receiveThread1 = null;
    Thread receiveThread2 = null;
    int frameCounter = 0;


    private int port = 7777;
    private IPEndPoint ipep;

    public GameObject[] prefabs;
    //public Dictionary<string, GameObject> spawnable;
   
    [HideInInspector]
    public List<CustomClasses.Spawn> waitingforspawn = new List<CustomClasses.Spawn>();
    [HideInInspector]
    public List<CustomClasses.Remove> waitingforremoval = new List<CustomClasses.Remove>();
    [HideInInspector]
    public List<CustomClasses.SceneObject> Objs2Update = new List<CustomClasses.SceneObject>();

    public GameObject[] SceneGameObjects;

    IPEndPoint sender;
    EndPoint Remote;
   
    PlayerController playerController1;
    PlayerController playerController2;
    CustomClasses.Message message = new CustomClasses.Message();

    int ACK1 = -1;
    int ACK2 = -1;

    // Start is called before the first frame update
    void Start()
    {
       // GameObject[] allObjects = FindObjectsOfType<GameObject>();
       playerController1 = player1.GetComponent<PlayerController>();
       playerController2 = player2.GetComponent<PlayerController>();

        foreach (GameObject go in SceneGameObjects)
        { 
            if(go.name != "Variables Saver")
            {
                CustomClasses.SceneObject newSceneObject = new CustomClasses.SceneObject();
                newSceneObject.setGO(go);
                newSceneObject.name = go.name;
                newSceneObject.position = go.transform.position;
                newSceneObject.rotation = go.transform.rotation;
                newSceneObject.setModDate(System.DateTime.UtcNow.Second);
                newSceneObject.setMod(false);
                newSceneObject.guid = Guid.NewGuid().ToString();
                DynamicGameObjects.Add(newSceneObject);
            } 
        }
        ExecuteScript();
    }

    // Update is called once per frame
    void Update()
    {
        frameCounter++;
        for (int i = 0; i < Objs2Update.Count; i++)
        {
           
                for(int j = 0; j < DynamicGameObjects.Count; j++)
                {
                    if(Objs2Update[i].guid == DynamicGameObjects[j].guid)
                    {
                        GameObject obj2update = DynamicGameObjects[j].getGO();
                        //obj2update.transform.position = Objs2Update[i].position;
                        obj2update.transform.rotation = Objs2Update[i].rotation;
                    }
                }
                

            
        }
    }

    public void ExecuteScript()
    {

        ipep = new IPEndPoint(IPAddress.Any, port);


        client1.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        client1.socket.Bind(ipep);

        ipep = new IPEndPoint(IPAddress.Any, port + 1);
        client2.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        client2.socket.Bind(ipep);


        sender = new IPEndPoint(IPAddress.Any, 0);
        Remote = (EndPoint)(sender);

        sendThread = new Thread(ThreadSend);
        threadstart1 = new Thread(ThreadStart1);
        threadstart2 = new Thread(ThreadStart2);

        receiveThread1 = new Thread(ThreadReceive1);
        receiveThread2 = new Thread(ThreadReceive2);

        threadstart1.Start();
        threadstart2.Start();
        receiveThread1.Start();
        receiveThread2.Start();
        sendThread.Start();
    }



    void ThreadStart1()
    {
        while(true)
        {
            if (!client1.active)
            {
                try
                {
                    Debug.Log("Waiting for a client");
                    byte[] msg = new Byte[256];
                    int recv = client1.socket.ReceiveFrom(msg, ref Remote);
                    client1.active = true;
                    client1.remote = Remote;
                    Debug.Log("Message received: " + Encoding.ASCII.GetString(msg));

                    CustomClasses.WelcomeMessage welcome = new CustomClasses.WelcomeMessage();
                    welcome.GUIDplayer1 = DynamicGameObjects[0].guid;
                    welcome.GUIDplayer2 = DynamicGameObjects[1].guid;
                    welcome.GUIDgun1 = DynamicGameObjects[2].guid;
                    welcome.GUIDgun2 = DynamicGameObjects[3].guid;

                    for (int i = 0; i < DynamicGameObjects.Count; i++)
                    {
                        CustomClasses.Spawn newSpawn = new CustomClasses.Spawn();

                        newSpawn.name = "1";
                        newSpawn.setGO(DynamicGameObjects[i].getGO());
                        newSpawn.position = DynamicGameObjects[i].position;
                        newSpawn.rotation = Quaternion.identity;
                        newSpawn.guid = DynamicGameObjects[i].guid;
                        welcome.spawns.Add(newSpawn);
                    }
                    welcome.objects = DynamicGameObjects;
                    MemoryStream stream = new MemoryStream();
                    stream = serializeWelcome(welcome);
                    client1.socket.SendTo(stream.ToArray(), SocketFlags.None, Remote);
                }
                catch (SystemException e)
                {
                    Debug.Log("Couldn't send or receive message");
                    Debug.Log(e.ToString());

                }
            }
        }
      
       
    }
    void ThreadStart2()
    {
        while (true)
        {
            if (!client2.active)
            {
                try
                {
                    Debug.Log("Waiting for a client");
                    byte[] msg = new Byte[256];
                    int recv = client2.socket.ReceiveFrom(msg, ref Remote);
                    client2.active = true;
                    client2.remote = Remote;
                    Debug.Log("Message received: " + Encoding.ASCII.GetString(msg));

                    CustomClasses.WelcomeMessage welcome = new CustomClasses.WelcomeMessage();
                    welcome.GUIDplayer1 = DynamicGameObjects[0].guid;
                    welcome.GUIDplayer2 = DynamicGameObjects[1].guid;
                    welcome.GUIDgun1 = DynamicGameObjects[2].guid;
                    welcome.GUIDgun2 = DynamicGameObjects[3].guid;
                    for (int i = 0; i < DynamicGameObjects.Count; i++)
                    {
                        CustomClasses.Spawn newSpawn = new CustomClasses.Spawn();

                        newSpawn.name = "1";
                        newSpawn.setGO(DynamicGameObjects[i].getGO());
                        newSpawn.position = DynamicGameObjects[i].position;
                        newSpawn.rotation = Quaternion.identity;
                        newSpawn.guid = DynamicGameObjects[i].guid;
                        welcome.spawns.Add(newSpawn);
                    }
                    welcome.objects = DynamicGameObjects;
                    MemoryStream stream = new MemoryStream();
                    stream = serializeWelcome(welcome);
                    client2.socket.SendTo(stream.ToArray(), SocketFlags.None, Remote);
                }
                catch (SystemException e)
                {
                    Debug.Log("Couldn't send or receive message");
                    Debug.Log(e.ToString());

                }
            }
        }
    }
    void ThreadSend()
    {
        while(true)
        {
            if (frameCounter >= 5)
            {
                List<CustomClasses.SceneObject> UpdatedGameObjects = new List<CustomClasses.SceneObject>(); 
                foreach (CustomClasses.SceneObject go in DynamicGameObjects)
                {
                    if (go.CheckMod())
                    {
                        UpdatedGameObjects.Add(go);
                        go.setMod(false);
                    }
                }

                if (waitingforspawn.Count > 0)
                {
                    Debug.Log(waitingforspawn);


                    message.spawns = waitingforspawn;
                    message.addType("spawn");
                }
                if (waitingforremoval.Count > 0)
                {

                    Debug.Log("Remove: " + waitingforremoval);

                    for (int i = 0; i < waitingforremoval.Count; i++)
                    {
                        for (int j = 0; j < DynamicGameObjects.Count; j++)
                        {
                            if (DynamicGameObjects[j].guid == waitingforremoval[i].guid)
                            {
                                DynamicGameObjects.RemoveAt(j);
                                j--;
                            }

                        }

                    }

                    message.removals = waitingforremoval;
                    message.addType("remove");
                }
                if (UpdatedGameObjects.Count > 0)
                {
                    message.objects = UpdatedGameObjects;
                    message.addType("movement");
                }

               
              
                    if (client1.active)
                    {
                        try
                        {
                        if (ACK1 != -1)
                        {
                            message.addType("acknowledgement");
                            message.ACK = ACK1;
                            ACK1 = -1;
                        }
                        MemoryStream stream = new MemoryStream();
                        stream = serializeJson(message);
                        client1.socket.SendTo(stream.ToArray(), SocketFlags.None, client1.remote);
                        message.messageTypes.Remove("acknowledgement");
                        }
                        catch (SystemException e)
                        {
                            Debug.Log("Couldn't send message");
                            Debug.Log(e.ToString());
                            client1.active = false;
                        }
                    }
                    if (client2.active)
                    {
                        try
                        {
                        if (ACK2 != -1)
                        {
                            message.addType("acknowledgement");
                            message.ACK = ACK2;
                            ACK2 = -1;
                        }
                        MemoryStream stream = new MemoryStream();
                        stream = serializeJson(message);
                        client2.socket.SendTo(stream.ToArray(), SocketFlags.None, client2.remote);
                        message.messageTypes.Remove("acknowledgement");

                        }
                        catch (SystemException e)
                        {
                            Debug.Log("Couldn't send message");
                            Debug.Log(e.ToString());
                            client2.active = false;
                        }
                    }
                    message.objects.Clear();
                    message.spawns.Clear();
                    message.inputs.Clear();
                    message.removals.Clear();
                    message.messageTypes.Clear();

                    waitingforspawn.Clear();
                    waitingforremoval.Clear();
                

                frameCounter = 0;
            }
        }
       
    }
    void ThreadReceive1()
    { 
        while (true)
        {
            if (client1.active)
                try
                {
                    byte[] msg = new Byte[2000];
                    int recv = client1.socket.ReceiveFrom(msg, ref Remote);
                    //Debug.Log(Encoding.ASCII.GetString(msg));
                    MemoryStream stream = new MemoryStream(msg);
                    CustomClasses.Message m = deserializeJson(stream);
                    if(message.messageTypes.Contains("acknowledgement"))
                    {
                       
                        ACK1 = m.ACK;
                     //   Debug.Log("ACK = " + m.ACK);
                    }
                   

                    foreach (CustomClasses.Input input in m.inputs)
                    {
                        playerController1.AddInput(input);
                    }
                    if (m.messageTypes.Contains("movement"))
                    {
                        foreach (CustomClasses.SceneObject obj in m.objects)
                        {
                            Objs2Update.Add(obj);
                        }
                    }

                }
                catch (SystemException e)
                {
                    Debug.Log("Couldn't receive message");
                    Debug.Log(e.ToString());
                    client1.active = false;
                }

        }

    }
    void ThreadReceive2()
    {
        while (true)
        {
            if(client2.active)
            try
            {
                byte[] msg = new Byte[2000];
                int recv = client2.socket.ReceiveFrom(msg, ref client2.remote);
                //Debug.Log(Encoding.ASCII.GetString(msg));
                MemoryStream stream = new MemoryStream(msg);
                CustomClasses.Message m = deserializeJson(stream);
                    if (message.messageTypes.Contains("acknowledgement"))
                    {
                        message.addType("acknowledgement");
                        ACK2 = m.ACK;
                      //  Debug.Log("ACK = " + m.ACK);
                    }

                    foreach (CustomClasses.Input input in m.inputs)
                {
                    playerController2.AddInput(input);
                }
                    if (m.messageTypes.Contains("movement"))
                    {
                        foreach (CustomClasses.SceneObject obj in m.objects)
                        {
                            Objs2Update.Add(obj);
                        }
                    }
                }
            catch (SystemException e)
            {
                Debug.Log("Couldn't receive message");
                Debug.Log(e.ToString());
                    client2.active = false;

                }

        }

    }

    public void EndConnection()
    {
        Debug.Log("Stopping UDP Server connection");
        try
        {
            // newSocket.Shutdown(SocketShutdown.Both);
            client1.socket.Close();
            client2.socket.Close();
        }
        catch (SystemException e)
        {
            Debug.Log("Socket already closed");
            Debug.Log(e.ToString());

        }
        if (threadstart1 != null)
            threadstart1.Abort();
        if (threadstart2 != null)
            threadstart2.Abort();
        if (receiveThread1 != null)
            receiveThread1.Abort();
        if (receiveThread2 != null)
            receiveThread2.Abort();
        if (sendThread != null)
            sendThread.Abort();

    }

    void OnApplicationQuit()
    {
        try
        {
            client1.socket.Shutdown(SocketShutdown.Both);
            client1.socket.Close();
            client2.socket.Shutdown(SocketShutdown.Both);
            client2.socket.Close();
        }
        catch (SystemException e)
        {
            Debug.Log("Socket already closed");
            Debug.Log(e.ToString());

        }

        if (threadstart1 != null)
            threadstart1.Abort();
        if (threadstart2 != null)
            threadstart2.Abort();
        if (receiveThread1 != null)
            receiveThread1.Abort();
        if (receiveThread2 != null)
            receiveThread2.Abort();
        if (sendThread != null)
            sendThread.Abort();
    }

    MemoryStream serializeJson(CustomClasses.Message message)
    {
       
        string json = JsonUtility.ToJson(message);
       // Debug.Log(json);
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(json);
        return stream;
    }

    CustomClasses.Message deserializeJson(MemoryStream stream)
    {
        var m = new CustomClasses.Message();
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);
        string json = reader.ReadString();
      //  Debug.Log(json);
        m = JsonUtility.FromJson<CustomClasses.Message>(json);
        return m;
    }


    MemoryStream serializeWelcome(CustomClasses.WelcomeMessage message)
    {

        string json = JsonUtility.ToJson(message);
        Debug.Log(json);
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(json);
        return stream;
    }
}



//bool exit = false;
//System.DateTime myTime = System.DateTime.UtcNow;
//while (!exit)
//{
//    if ((System.DateTime.UtcNow - myTime).Seconds > 5f)
//    {
//        exit = true;
//    }
//}