using System.Net.Sockets;
using System.Text;
using System;
using RPGCommon;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
public class NetworkManager : MonoBehaviour
{

    public static NetworkManager Instance { get; private set; } 
    
    const string SERVER_IP = "127.0.0.1";
    const int PORT = 7777;
    ServerSession _session = new ServerSession();

    public int myId = -1;
    public string nick = null;
    public int wincount = 0;
    TcpClient client;
    NetworkStream stream;

    public GameObject playerPrefab;
    //Dictionary<int, GameObject> userMap = new Dictionary<int, GameObject>();
    string packetBuffer = "";

    //temp
    public int EnemyId;
    public int MyRole;
   

    public JobType myJob;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        ConnectToServer();
    }

    void Update()
    {
        if (client == null || !client.Connected) return;

        try
        {
            if (stream.DataAvailable)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    _session.OnReceive(buffer, 0, bytesRead);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"수신 에러: {e.Message}");
        }
    }

    /*public void SpawnPlayer(int id)
    {
        if (userMap.ContainsKey(id)) return;
        
        GameObject newPlayer = Instantiate(playerPrefab, new Vector3(0,0,0), Quaternion.identity);

        userMap.Add(id, newPlayer);
        if(id == myId)
        {
            newPlayer.name = $"Player";
            Camera.main.transform.SetParent(newPlayer.transform);
            Camera.main.transform.localPosition = new Vector3(0, 10, -10);
            Camera.main.transform.localRotation = Quaternion.Euler(45, 0, 0);
        }
        else
        {
            newPlayer.name = $"Other Player : {id}";

        }
    }*/
    public void SetMyId(int id, string nickname, int winCount)
    {
        myId = id;
        nick = nickname;
        wincount = winCount;
    }

    public void UpdatewinCountRequest()
    {
        PacketWinCountUpdateRequest wcuR = new PacketWinCountUpdateRequest();
        Send(wcuR.Serialize());
    }

    public void UpdateWinCount(int Count)
    {
        wincount = Count;
    }
    void SendMovePacket(int id, int x, int y)
    {
        PacketMove packet = new PacketMove();
        packet.playerId = id;
        packet.x = x;
        packet.y = y;

        byte[] sendData = packet.Serialize();

        Send(sendData);
    }

    public void Send(byte[] data)
    {
        if(client != null && client.Connected)
        {
            stream.Write(data, 0, data.Length);
        }
    }

    /*void ProcessPacket(string msg)
    {
        if(msg.StartsWith("WELCOME:"))
        {
            string content = msg.Substring(8);
            myId = int.Parse(content);
            Debug.Log($"ID할당{myId}");
        }
        else if (msg.StartsWith("POS:"))
        {
            try
            {
                string content = msg.Substring(4);
                string[] parts = content.Split(',');

                int id = int.Parse(parts[0]);
                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);

                Vector3 targetPos = new Vector3(x, y, 0);

                if (userMap.ContainsKey(id))
                {
                    RemotePlayer rp = userMap[id].GetComponent<RemotePlayer>();
                    if (rp != null)
                    {
                        rp.SetTargetPosition(targetPos);
                    }
                }
                else
                {
                    GameObject newPlayer = Instantiate(playerPrefab, targetPos, Quaternion.identity);
                    newPlayer.name = $"Player_{id}";
                    userMap.Add(id, newPlayer);

                    if(id == myId)
                    {
                        Camera.main.transform.SetParent(newPlayer.transform);

                        Camera.main.transform.localPosition = new Vector3(0, 10, -10);
                        Camera.main.transform.localRotation = Quaternion.Euler(45, 0, 0);
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"패킷 해석 에러: {msg} / {e.Message}");
            }


        }
    }*/
    void SendData(string Message)
    {
        if (stream == null || !stream.CanWrite) return;

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(Message);
            stream.Write(data, 0, data.Length);
        }
        catch(Exception e)
        {
            Debug.Log($"전송에러 {e.Message}");
        }
    }

    public void HandleMatch()
    {
        LobbyManager.Instance.OnUI();
    }

    public void GotoLobby()
    {
        StartCoroutine(LoadingGameScene("Lobby"));
        //UpdatewinCountRequest();
    }

    IEnumerator LoadingGameScene(string Scene)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(Scene);

        op.allowSceneActivation = false;
        while (!op.isDone)
        {
            yield return null;

                if (op.progress >= 0.9f)
                {
                    op.allowSceneActivation = true;
                }
        }
    }
    private void ConnectToServer()
    {
        try
        {
            client = new TcpClient();
            client.Connect(SERVER_IP, PORT);

            Debug.Log($"[Client] 서버{SERVER_IP} : {PORT}에 접속");

            stream = client.GetStream();

        }
        catch (Exception e)
        {
            Debug.LogError($"[Client] 접속 실패 : {e.Message}");
        }
    }

    private void OnApplicationQuit()
    {
        if (stream != null) stream.Close();
        if (client != null) client.Close();
    }
}
