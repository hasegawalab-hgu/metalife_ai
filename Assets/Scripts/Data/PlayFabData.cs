using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabData : MonoBehaviour
{
    public static string SharedGroupAdminId = "B909DEE6B7D0C9E2"; // 全共有グループの管理者（Lab_AdminのPlayFabId & CustomId）
    public static string CurrentChannelId;
    public static string CurrentMessageTarget; // CH: All, DM: id
    public static string AllYearLabSharedGroupId = "hasegawa_lab"; // 全年度
    public static string MyYearLabSharedGroupId; // 自分の年度
    public static string CurrentSharedGroupId; // 現在入室しているルームのグループ、今後選択する画面を作成
    public static List<ChannelData> CurrentRoomChannels = new List<ChannelData>();
    public static List<PlayerData> CurrentRoomPlayers = new List<PlayerData>();
}
