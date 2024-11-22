using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerInfo
{
    public string id;
    public string name;
    public string texturePath;
}

public class PlayerStateInfo
{
    public string id;
    public string name;
    public Vector3 pos;
    public int dir;
    public bool isAI;
    public bool isChatting;
    public string content;
    public bool isInputting;

    public PlayerStateInfo(string id, string name, Vector3 pos, int dir, bool isAI, bool isChatting, string content, bool isInputting)
    {
        this.id = id;
        this.name = name;
        this.pos = pos;
        this.dir = dir;
        this.isAI = isAI;
        this.isChatting = isChatting;
        this.content = content;
        this.isInputting = isInputting;
    }
}

public class PlayFabData : MonoBehaviour
{
    public static bool Islogouted = false;

    public static string MyName;
    public static string MyGraduationYear;
    public static Texture2D MyTexture = null;
    public static string MyTexturePath = "";
    public static bool NewCreated = false;

    public static string SharedGroupAdminId = "B909DEE6B7D0C9E2"; // 全共有グループの管理者（Lab_AdminのPlayFabId & CustomId）
    public static string CurrentChannelId = "general"; // DM: "DM"
    public static string CurrentMessageTarget = "All"; // CH: "All", DM: id
    public static string AllYearLabSharedGroupId = "hasegawa_lab"; // 全年度
    public static string MyYearLabSharedGroupId; // 自分の年度
    public static string CurrentSharedGroupId; // 現在入室しているルームのグループ、今後選択する画面を作成
    public static string CurrentAI;
    public static Dictionary<string, ChannelData> CurrentRoomChannels = new Dictionary<string, ChannelData>();
    // public static Dictionary<string, string> CurrentRoomPlayers = new Dictionary<string, string>(); // key: id, value: displayName
    public static Dictionary<string, PlayerData> CurrentRoomPlayersRefs = new Dictionary<string,PlayerData>();
    public static Dictionary<string, DMButton> DictDMScripts = new Dictionary<string, DMButton>();
    public static Dictionary<string, ChannelButton> DictChannelScripts = new Dictionary<string, ChannelButton>();
    public static Dictionary<string, GameObject> DictSpawnerSimpleMessage = new Dictionary<string, GameObject>();

    public static Dictionary<string, int> DictReadMessageCount = new Dictionary<string, int>();
    public static Dictionary<string, PlayerInfo> DictPlayerInfos = new Dictionary<string, PlayerInfo>();
    public static Dictionary<string, Vector3> DictDistance = new Dictionary<string, Vector3>();
    public static Dictionary<string, PlayerStateInfo> DictPlayerStateInfos = new Dictionary<string, PlayerStateInfo>();
    public static List<Dictionary<string, PlayerStateInfo>> ListDictPlayerStateInfo = new List<Dictionary<string,PlayerStateInfo>>();
    public static int PlayerStateInfoLength {private set; get; } = 10;
    public static Dictionary<string, List<MessageData>> DictAllMessageDatas = new Dictionary<string, List<MessageData>>();

    public static void Initialize()
    {
        Debug.Log("initialize");
        MyName = "";
        MyGraduationYear = "";
        MyTexture = null;
        MyTexturePath = "";
        CurrentAI = "";
        CurrentRoomChannels = new Dictionary<string, ChannelData>();
        // CurrentRoomPlayers = new Dictionary<string, string>(); 
        CurrentRoomPlayersRefs = new Dictionary<string,PlayerData>();
        DictDMScripts = new Dictionary<string, DMButton>();
        DictChannelScripts = new Dictionary<string, ChannelButton>();
        DictReadMessageCount = new Dictionary<string, int>();
        DictPlayerInfos = new Dictionary<string, PlayerInfo>();
        DictPlayerStateInfos = new Dictionary<string, PlayerStateInfo>();
        DictAllMessageDatas = new Dictionary<string, List<MessageData>>();
        ListDictPlayerStateInfo = new List<Dictionary<string, PlayerStateInfo>>();
    }
}
