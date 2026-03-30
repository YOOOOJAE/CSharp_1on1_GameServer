using UnityEngine;
using System.Collections.Generic;
using RPGCommon;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public GameObject Player1;
    public GameObject Player2;
    public GameObject AttackUI;

    public GameObject Win;
    public GameObject Lose;
    public GameObject Leave;
    public GameObject Rematch;

    public static GameManager Instance;

    Dictionary<int, GameObject> _users = new Dictionary<int, GameObject>();


    public bool GameStart = false;
    public bool GameEnded = false;
    public int myRole;
    public int enemyId;
    public int myId;

    public int MyStack = 0;
    public int MyMaxStack = 0;
    public UnityEngine.UI.Image[] BatteryStack = new UnityEngine.UI.Image[9];
    public UnityEngine.UI.Image[] HPImage = new UnityEngine.UI.Image[3];

    public GameObject Message;
    private void Awake()
    {
        Instance = this;
    }

    void UpdateStack(int count)
    {
        for(int i = 0; i< BatteryStack.Length; i++)
        {
            if(i < count)
            {
                BatteryStack[i].color = new Color(255, 255, 255, 1);
            }
            else
            {
                BatteryStack[i].color = new Color(255, 255, 255, 0.1f);
            }
        }
    }
    void UpdateHPUI(int hp)
    {
        Debug.Log("HPUI UPDATE");
        for(int i = 0; i < HPImage.Length; i++)
        {
            if (i < hp)
            {
                HPImage[i].color = new Color(255, 255, 255, 0.8f);
            }
            else
            {
                HPImage[i].color = new Color(255, 255, 255, 0f);
            }
        }
    }

    void Start()
    {
        myRole = NetworkManager.Instance.MyRole;
        enemyId = NetworkManager.Instance.EnemyId;
        myId = NetworkManager.Instance.myId;

        if (myRole == 1)
        {
            SetUpMyPlayer(Player1);
            SetUpEnemyPlayer(Player2, enemyId);

            _users.Add(myId, Player1);
            _users.Add(enemyId, Player2);
        }
        else
        {
            SetUpMyPlayer(Player2);
            SetUpEnemyPlayer(Player1, enemyId);
            _users.Add(myId, Player2);
            _users.Add(enemyId, Player1);
        }

        ReadyPacketSend();
    }
    public void GameEnd(int id, int reason)
    {
        GameEnded = true;

        if(id == myId)
        {
            ShowDefeat(reason);
        }
        else
        {
            ShowVictory(reason);
        }
    }

    public void ShowDefeat(int reason)
    {
        Lose.SetActive(true);
        Leave.SetActive(true);
        Rematch.SetActive(true);
    }

    public void ShowVictory(int reason)
    {
        Win.SetActive(true);
        Leave.SetActive(true);
        Rematch.SetActive(true);
    }
    

    public void LeaveGame(int id)
    {
        if (id == myId)
            StartCoroutine(LoadingLobbyScene());
        else
        {
            Debug.Log("상대방나감");
            Message.SetActive(true);
        }

            
    }
    public void HandlePlayerDie(int id)
    {
        if (_users.ContainsKey(id))
        {
            PlayerController playerController = _users[id].GetComponent<PlayerController>();
            playerController.PlayerDie();
        }
    }
    public void PlayerAttack(int id, float Angle, float speed)
    {
        if (_users.ContainsKey(id))
        {
            PlayerController playerController = _users[id].GetComponent<PlayerController>();
            playerController.AttackBullet(Angle, speed);
        }
    }
    public void PlayerHpUpdate(int id, int hp)
    {
        if (_users.ContainsKey(id))
        {
            if(id == myId)
            {
                UpdateHPUI(hp);
            }
            PlayerController playerController = _users[id].GetComponent<PlayerController>();
            playerController.HpUpdatePacket(id, hp);
        }
    }
    public void PlayerHit(int id)
    {
        if (_users.ContainsKey(id))
        {
            PlayerController playerController = _users[id].GetComponent<PlayerController>();
            playerController.Hit(id);
        }
    }
    void SetUpMyPlayer(GameObject Player)
    {
        if (Player.TryGetComponent<PlayerController>(out var controller))
        {
            controller.enabled = true;
            AttackUI.GetComponent<AttackCoolUI>().Initialize(controller);
        }
    }

    void SetUpEnemyPlayer(GameObject Player, int enmeyId)
    {
        PlayerController a = Player.GetComponent<PlayerController>();
        a.playerid = enemyId;
        a.enabled = false;
    }

    public void ReadyPacketSend()
    {
        PacketReady Readypacket = new PacketReady();
        Readypacket.playerId = myId;

        NetworkManager.Instance.Send(Readypacket.Serialize());
    }

    public void LeavepacketSend()
    {
        PacketLeaveGame LGPacket = new PacketLeaveGame();

        LGPacket.playerId = myId;

        NetworkManager.Instance.Send(LGPacket.Serialize());
    }

    void PlaerTransformReset()
    {
        if(myRole == 1)
        {
            transform.position = new Vector3(0, -4);
        }
        else
        {
            transform.position = new Vector3(0, 4);
        }

    }
    public void HandleGameStart()
    {
        if(GameEnded)
        {
            Win.SetActive(false);
            Lose.SetActive(false);
            Leave.SetActive(false);
            Rematch.SetActive(false);

            foreach (var entry in _users)
            {
                GameObject playerObj = entry.Value;
                PlayerController pc = playerObj.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.StatusReset();
                    pc.SetSprite();
                    PlaerTransformReset();
                }
            }
            GameEnded = false;
        }
        GameStart = true;

        UpdateStack(MyMaxStack);
    }

    public void HandleMovePacket(int playerId, int x, int y)
    {
        if(_users.ContainsKey(playerId))
        {
            GameObject go = _users[playerId];

            go.transform.position = new Vector3(x, y, 0);
        }

    }
    public void HandleMoveStackPacket(int stack)
    {
        UpdateStack(stack);
        MyStack = stack;
    }

    public void HandleStartPacket(int playerId, int x, int y, JobType job)
    {
        if (_users.ContainsKey(playerId))
        {
            Debug.Log("Handle Start Packet Arrived");
            GameObject go = _users[playerId];

            go.transform.position = new Vector3(x, y, 0);
            JobData Job = JobDataRepository.GetStat(job);
            PlayerController Pc = go.GetComponent<PlayerController>();
            Pc.Job = job;
            Pc.attackCool = Job.AttackSpeed;
            Pc.SetSprite();


            if(playerId == myId)
            {
                MyStack = Job.MoveStack;
                MyMaxStack = Job.MoveStack;
                UpdateStack(MyStack);
            }
        }

    }
    IEnumerator LoadingLobbyScene()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync("Lobby");

        op.allowSceneActivation = false;
        float timer = 0f;
        while (!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;

            if (op.progress < 0.9f)
            {

            }
            else
            {
                    op.allowSceneActivation = true;
            }
        }
    }
}
