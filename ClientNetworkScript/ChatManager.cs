using UnityEngine;
using UnityEngine.UI;
using RPGCommon;
using System.Net.NetworkInformation;
using TMPro;
public class ChatManager : MonoBehaviour
{
    public InputField inputField;
    public GameObject chatLogText;
    public Transform Content;
    public ScrollRect myScrollRect;
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            if(inputField.text.Length > 0)
            {
                sendChat();
            }
        }
    }

    public void sendChat()
    {
        string msg = inputField.text;

        PacketChat packet = new PacketChat();
        packet.playerId = NetworkManager.Instance.myId;
        packet.chatMsg = msg;

        NetworkManager.Instance.Send(packet.Serialize());

        inputField.text = "";
        inputField.ActivateInputField();
    }

    public void OnRecvChat(string msg)
    {
        GameObject chat = Instantiate(chatLogText, Content);
        chat.GetComponentInChildren<TextMeshProUGUI>().text = msg;

        if (Content.childCount > 50)
        {
            Destroy(Content.GetChild(0).gameObject);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.GetComponent<RectTransform>());


        myScrollRect.verticalNormalizedPosition = 0f;
    }

}
