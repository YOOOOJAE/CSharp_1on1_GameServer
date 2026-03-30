using RPGCommon;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginRequestManager : MonoBehaviour
{

    public static LoginRequestManager Instance;
    
    public InputField ID;
    public InputField Password;


    public GameObject LoginFailedMessage;
    public TextMeshProUGUI failedmessagetext;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void FaildMessage(int success)
    {
        string message;
        switch (success)
        {
            case 2:
                message = "아이디가 존재하지 않습니다.";
                break;
            case 3:
                message = "비밀번호가 잘못되었습니다.";
                break;
            case 4:
                message = "이미 로그인중인 아이디입니다.";
                break;
            default:
                message = "알수없는오류";
                break;
        }

        failedmessagetext.text = message;
        LoginFailedMessage.SetActive(true);
    }

    public void OffFaildMessage()
    {
        failedmessagetext.text = " ";
        LoginFailedMessage.SetActive(false);
    }
    public void Request()
    {
        string id = ID.text;
        string password = Password.text;

        PacketLoginRequest req = new PacketLoginRequest();

        req.userId = id;
        req.password = password;

        NetworkManager.Instance.Send(req.Serialize());

    }


}
