/****************************************************
    文件：GameRoot.cs
    作者：XieChangsheng
    邮箱:  1059334012@qq.com
    日期：2023/9/6 1:30:17
    功能：根节点 
*****************************************************/

using PEProtocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameRoot : MonoBehaviour {
    public static GameRoot Instance = null;
    public AudioSource audioSource;
    public AudioClip[] audioClip;
    public bool isLogin;
    public Text txtTips;
    private bool isTipsShow = false;
    private bool isMessageShow = false;
    private Queue<string> tipsQue = new Queue<string>();
    private Queue<Message> messageQue = new Queue<Message>();
    public GameObject messagePrefab;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(this);
        Init();
    }

    private void Init() {
        NetSvc.Instance.InitSvc();
        DataManager.Instance.Init();
    }

    public void AddTips(string tips) {
        lock (tipsQue) {
            tipsQue.Enqueue(tips);
        }
    }

    public void AddMessage(Message message) {
        lock (messageQue) {
            messageQue.Enqueue(message);
        }
    }

    private void SetTips(string tips) {
        txtTips.transform.gameObject.SetActive(true);
        txtTips.text = tips;

        AnimationClip clip = txtTips.GetComponent<Animation>().GetClip("TipsShowAni");
        txtTips.GetComponent<Animation>().Play();

        StartCoroutine(AniPlayDone(clip.length, () => {
            txtTips.transform.gameObject.SetActive(false);
            isTipsShow = false;
        }));
    }

    private IEnumerator AniPlayDone(float sec, Action cb) {
        yield return new WaitForSeconds(sec);
        if (cb != null) {
            cb();
        }
    }

    private void Update() {
        NetSvc.Instance.Update();
        if (tipsQue.Count > 0 && isTipsShow == false) {
            lock (tipsQue) {
                string tips = tipsQue.Dequeue();
                isTipsShow = true;
                SetTips(tips);
            }
        }
        if (messageQue.Count > 0 && isMessageShow == false) {
            lock (messageQue) {
                Message msg = messageQue.Dequeue();
                PlayerData playerData = msg.playerData;
                string content = msg.message;
                GameObject msgPre = Instantiate(messagePrefab) as GameObject;
                msgPre.transform.parent = transform.Find("TipsCanvas");
                msgPre.transform.Find("Button").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Avatars/{playerData.Photo}");
                msgPre.transform.Find("Circle/Text").GetComponent<Text>().text = DataManager.Instance.GetPlayerIndex(playerData).ToString();
                Text nameText = msgPre.transform.Find("Name").GetComponent<Text>();
                nameText.text = playerData.Name + ":" + content;
                Transform background = msgPre.transform.Find("Background");
                RectTransform backRect = background.GetComponent<RectTransform>();
                backRect.sizeDelta = new Vector2(GetRowTextWordWidth(nameText) + 50, backRect.sizeDelta.y);
            }
        }
    }

    private float GetRowTextWordWidth(Text text) {
        float width = 0;
        TextGenerator tg = text.cachedTextGeneratorForLayout;
        TextGenerationSettings settings = text.GetGenerationSettings(Vector2.zero);
        width = tg.GetPreferredWidth(text.text, settings) / text.pixelsPerUnit;
        return width;
    }

    public void PlayClip(int index) {
        audioSource.clip = audioClip[index];
        audioSource.Play();
    }
}