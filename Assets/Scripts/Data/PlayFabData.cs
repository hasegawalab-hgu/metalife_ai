using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabData : MonoBehaviour
{
    public static string SharedGroupAdminId = "B909DEE6B7D0C9E2"; // 共有グループの管理者（Lab_AdminのPlayFabId & CustomId）
    public static Dictionary<string, string> MyYearLabAllPlayers; // key: playfabId, value: isOnline
    public static Dictionary<string, string> AllYearLabAllPlayers; // key: playfabId, value: isOnline
    public static List<ChannelData> MyYearLabChannels;
    public static List<ChannelData> AllYearLabChannels;
    public static string CurrentChannelId;
    public static string CurrentMessageTarget; // CH: All, DM: id
    public static string AllYearLabSharedGroupId = "hasegawa_lab"; // 全年度
    public static string MyYearLabSharedGroupId; // 自分の年度
    public static string CurrentSharedGroupId = MyYearLabSharedGroupId; // 現在入室しているルームのグループ
}
