using RPGCommon;
using UnityEngine;
using System.Text;

public class ServerSession : PacketSession
{
    protected override void OnPacketReceived(ushort protocolId, byte[] body)
    {
        PacketType type = (PacketType)protocolId;

        Debug.Log($"패킷 수신! ID: {type}, 크기: {body.Length}");

        switch(type)
        {
            case PacketType.Chat:
                PacketChat chatPacket = new PacketChat();
                chatPacket.Deserialize(body);

                string displayMsg = $"{chatPacket.chatMsg}";
                GameObject.FindObjectOfType<ChatManager>().OnRecvChat(displayMsg);
                break;

            case PacketType.Move:
                PacketMove movePacket = new PacketMove();
                movePacket.Deserialize(body);

                if(GameManager.Instance != null)
                {
                    GameManager.Instance.HandleMovePacket(movePacket.playerId, movePacket.x, movePacket.y);
                }
                break;
            case PacketType.MoveStack:
                PacketMoveStack MoveStack = new PacketMoveStack();
                MoveStack.Deserialize(body);

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.HandleMoveStackPacket(MoveStack.stack);
                }
                break;
            case PacketType.SetPosition:
                PacketSetPosition SPPacket = new PacketSetPosition();
                SPPacket.Deserialize(body);

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.HandleMovePacket(SPPacket.playerId, SPPacket.x, SPPacket.y);
                }
                break;
            case PacketType.PlayerStartSetting:
                PacketPlayerStartSetting SSPacket = new PacketPlayerStartSetting();
                SSPacket.Deserialize(body);

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.HandleStartPacket(SSPacket.playerId, SSPacket.x, SSPacket.y, SSPacket.Job);
                }
                break;
            case PacketType.Login:
                PacketLogin loginPacket = new PacketLogin();
                loginPacket.Deserialize(body);
                if(loginPacket.success == 1)
                {
                    NetworkManager.Instance.SetMyId(loginPacket.playerId, loginPacket.nickname, loginPacket.winCount);
                    NetworkManager.Instance.GotoLobby();
                }
                else
                {
                    if (LoginRequestManager.Instance != null) LoginRequestManager.Instance.FaildMessage(loginPacket.success);
                }
                break;
            case PacketType.RegisterResult:
                PacketRegisterResult rrp = new PacketRegisterResult();
                rrp.Deserialize(body);

                if(RegisterManager.Instance != null)
                {
                    RegisterManager.Instance.RegisterResult(rrp.success, rrp.message);
                }

                break;
            case PacketType.FastMatchingSuccess:
                PacketFastMatchingSuccess FMSPacket = new PacketFastMatchingSuccess();
                FMSPacket.Deserialize(body);
                NetworkManager.Instance.EnemyId = FMSPacket.enemyId;
                NetworkManager.Instance.MyRole = FMSPacket.myRole;
                NetworkManager.Instance.HandleMatch();
                break;
            case PacketType.PlayerAttack:
                PacketPlayerAttack paPacket = new PacketPlayerAttack();
                paPacket.Deserialize(body);

                if(GameManager.Instance != null)
                {
                    GameManager.Instance.PlayerAttack(paPacket.playerId, paPacket.angle, paPacket.speed);
                }
                break;
            /*case PacketType.PlayerHit: // 서버에서 처리방식으로 바꿧기 때문에 주석처리.
                PacketPlayerHit phPacket = new PacketPlayerHit();
                phPacket.Deserialize(body);

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PlayerHit(phPacket.targetId);
                }
                break;*/
            case PacketType.PlayerHpUpdate:
                PacketPlayerHpUpdate phuPacket = new PacketPlayerHpUpdate();
                phuPacket.Deserialize(body);

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PlayerHpUpdate(phuPacket.playerId, phuPacket.currentHp);
                }
                break;
            case PacketType.PlayerDie:
                PacketPlayerDie pdPacket = new PacketPlayerDie();
                pdPacket.Deserialize(body);

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.HandlePlayerDie(pdPacket.playerId);
                }
                break;
            case PacketType.GameEnd:
                PacketGameEnd GEPacket = new PacketGameEnd();
                GEPacket.Deserialize(body);

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameEnd(GEPacket.loserId, GEPacket.reason);
                }
                break;
            case PacketType.LeaveGame:
                PacketLeaveGame LGPacket = new PacketLeaveGame();

                LGPacket.Deserialize(body);
                if(GameManager.Instance != null)
                {
                    GameManager.Instance.LeaveGame(LGPacket.playerId);
                }
                break;
            case PacketType.Start:
                PacketStart StartPacket = new PacketStart();
                StartPacket.Deserialize(body);

                if(GameManager.Instance != null)
                {
                    GameManager.Instance.HandleGameStart();
                }
                break;
            case PacketType.WinCountUpdate:
                PacketWinCountUpdate wcuP = new PacketWinCountUpdate();
                wcuP.Deserialize(body);

                Debug.Log($"{wcuP.playerId}{wcuP.winCount}");
                if(NetworkManager.Instance != null)
                {
                    NetworkManager.Instance.UpdateWinCount(wcuP.winCount);
                }
                break;
        }

    }
}