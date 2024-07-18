using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabData : MonoBehaviour
{
    public static bool Islogouted = false;

    public static string MyName;
    public static string MyGraduationYear;

    public static string SharedGroupAdminId = "B909DEE6B7D0C9E2"; // 全共有グループの管理者（Lab_AdminのPlayFabId & CustomId）
    public static string CurrentChannelId = "general";
    public static string CurrentMessageTarget; // CH: All, DM: id
    public static string AllYearLabSharedGroupId = "hasegawa_lab"; // 全年度
    public static string MyYearLabSharedGroupId; // 自分の年度
    public static string CurrentSharedGroupId; // 現在入室しているルームのグループ、今後選択する画面を作成
    public static Dictionary<string, ChannelData> CurrentRoomChannels = new Dictionary<string, ChannelData>();
    public static Dictionary<string, string> CurrentRoomPlayers = new Dictionary<string, string>();

    public static Dictionary<string, DMButton> DictDMScripts = new Dictionary<string, DMButton>();
}
