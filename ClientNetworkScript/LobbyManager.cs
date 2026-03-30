using RPGCommon;
using System.Collections;
using System.Threading;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{

    public GameObject MatchingImageUI;

    public GameObject MatchButton;
    public GameObject MatchCancleButton;
    public GameObject timerUI;


    public bool isMatch = false;
    
    public Slider progressBar;

    public static LobbyManager Instance;

    
    

    private void Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    public void MatchTimerStart()
    {
        timerUI.SetActive(true);
    }
    public void MatchTimerStop()
    {
        timerUI.SetActive(false);
    }

    public void SetMatchingState(bool isMatching)
    {
        MatchButton.SetActive(!isMatching);
        MatchCancleButton.SetActive(isMatching);

        if (timerUI != null)
            timerUI.SetActive(isMatching);

        if(isMatching)
        {
            SendMatchRequest();
            MatchTimerStart();
        }
        else
        {
            SendMatchCancel();
            MatchTimerStop();
        }
    }
    public void OnUI()
    {
        MatchingImageUI.SetActive(true);
        progressBar.gameObject.SetActive(true);
        OffUi();
    }

    public void OffUi()
    {
        //MatchingUI.SetActive(false);
        //MatchingUI.SetActive(false);

        StartCoroutine(LoadingGameScene());
    }
    
    IEnumerator  LoadingGameScene()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync("GameScene");

        op.allowSceneActivation = false;
        float timer = 0f;
        while(!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;

            if(op.progress < 0.9f)
            {
                progressBar.value = Mathf.Lerp(progressBar.value, op.progress, timer);
                if (progressBar.value >= op.progress)
                {
                    timer = 0f;
                }
            }
            else
            {
                if(progressBar != null)
                {
                    progressBar.value = Mathf.Lerp(progressBar.value, 1f, timer);
                    progressBar.gameObject.SetActive(false);
                }

                if (progressBar.value >= 0.99f)
                {
                    op.allowSceneActivation = true;
                }
            }
        }

    }
    public void SetMyJob(int job)
    {
        switch (job)
        {
            case 0:
                NetworkManager.Instance.myJob = JobType.Normal;
                break;
            case 1:
                NetworkManager.Instance.myJob = JobType.Stamina;
                break;
            case 2:
                NetworkManager.Instance.myJob = JobType.Speed;
                break;
            default:
                NetworkManager.Instance.myJob = JobType.Normal;
                break;
        }

        SendMyJob(NetworkManager.Instance.myJob);
    }

    public void SendMyJob(JobType job)
    {
        PacketPlayerSelectJob PSJPacket = new PacketPlayerSelectJob();
        PSJPacket.Job = job;
        PSJPacket.playerId = NetworkManager.Instance.myId;
        NetworkManager.Instance.Send(PSJPacket.Serialize());
    }

    public void SendMatchRequest()
    {
        PacketFastMatchingRequest FMRPacket = new PacketFastMatchingRequest();

        byte[] packet = FMRPacket.Serialize();

        NetworkManager.Instance.Send(packet);
        Debug.Log("서버에 매치요청");
    }

    public void SendMatchCancel()
    {
        PacketFastMatchingCancel FMRPacket = new PacketFastMatchingCancel();

        byte[] packet = FMRPacket.Serialize();

        NetworkManager.Instance.Send(packet);
        Debug.Log("서버에 매치요청캔슬");
    }
}
