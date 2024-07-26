using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayFabData : MonoBehaviour
{
    public static bool Islogouted = false;

    public static string MyName;
    public static string MyGraduationYear;

    public static string SharedGroupAdminId = "B909DEE6B7D0C9E2"; // 全共有グループの管理者（Lab_AdminのPlayFabId & CustomId）
    public static string CurrentChannelId = "general"; // DM: "DM"
    public static string CurrentMessageTarget = "All"; // CH: "All", DM: id
    public static string AllYearLabSharedGroupId = "hasegawa_lab"; // 全年度
    public static string MyYearLabSharedGroupId; // 自分の年度
    public static string CurrentSharedGroupId; // 現在入室しているルームのグループ、今後選択する画面を作成
    public static Dictionary<string, ChannelData> CurrentRoomChannels = new Dictionary<string, ChannelData>();
    public static Dictionary<string, string> CurrentRoomPlayers = new Dictionary<string, string>(); // key: id, value: displayName
    public static Dictionary<string, PlayerData> CurrentRoomPlayersRefs = new Dictionary<string,PlayerData>();
    public static Dictionary<string, DMButton> DictDMScripts = new Dictionary<string, DMButton>();
    public static Dictionary<string, ChannelButton> DictChannelScripts = new Dictionary<string, ChannelButton>();

    public static Dictionary<string, int> DictReadMessageCount = new Dictionary<string, int>();
}
