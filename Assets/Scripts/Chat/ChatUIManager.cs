using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChatUIManager : MonoBehaviour
{
    [SerializeField]
    private Toggle toggle_CHDM; // isOn == true: , isOn == false: DM
    [SerializeField]
    private GameObject obj_CH;
    [SerializeField]
    private GameObject obj_DM;


    [SerializeField]
    private GameObject spawner_channel;
    [SerializeField]
    private Button button_channelTarget; // prefab
    [SerializeField]
    private GameObject spawner_DM;
    [SerializeField]
    private Button button_DMTarget; // prefab
    [SerializeField]
    private TMP_InputField inputField;
    [SerializeField]
    private Button button_submit;
    [SerializeField]
    private GameObject spawner_message;
    [SerializeField]
    private TMP_Text text_messagePref; // prefab

    public void OnCalueChangedCHDM()
    {
        if(toggle_CHDM.isOn) // 
        {
            obj_CH.GetComponent<Image>().color += new Color(0f, 0f, 0f, 200);
            obj_DM.GetComponent<Image>().color += new Color(0f, 0f, 0f, -200);
        }
        else // DM
        {
            obj_DM.GetComponent<Image>().color += new Color(0f, 0f, 0f, 200);
            obj_CH.GetComponent<Image>().color += new Color(0f, 0f, 0f, -200);
        }
    }

    private void DisplayChannelTargets()
    {

        // var obj = Instantiate(button_channelTarget).GetComponentInChildren<TextMeshProUGUI>().text = ; // あとで変更
        // 親
        // スクリプト付与
        // スクリプトの変数初期化
        // onclick()設定
    }

    private void DisplayDMTargets()
    {

    }

    public void OnClickSubmit()
    {

    }

    public void OnClickCH()
    {

    }

    public void OnClickDM()
    {

    }

    public void OnClickChannelTarget()
    {

    }

    public void OnClickDMTarget()
    {

    }
}
