using System.Diagnostics.Tracing;
using Fusion;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using System.Collections.Generic;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public NetworkPrefabRef PlayerPrefab;
    GameObject playerContainer;

    public void PlayerJoined(PlayerRef player)
    {
        Debug.Log(Runner.SessionInfo.PlayerCount);
        if (player == Runner.LocalPlayer)
        {
            if(PlayerPrefab == default(NetworkPrefabRef)) // PlayerPrefabは何も設定されていない場合、nullにならない
            {
                PlayerPrefab = GameObject.Find("PlayerPrefabs").GetComponent<PlayerPrefabs>().playerPrefabs[0];
            }

            GetPlayerInfos(PlayFabSettings.staticPlayer.PlayFabId);
            if(Runner.SessionInfo.PlayerCount == 1 && Runner.IsSharedModeMasterClient)
            {
                var localPlayer = Runner.Spawn(prefabRef:PlayerPrefab, new Vector3(2.5f, -7.5f, 0), inputAuthority: this.Runner.LocalPlayer);
                PlayerData pd = localPlayer.GetComponent<PlayerData>();
                pd.IsHost = true;
                pd.PlayFabId = PlayFabSettings.staticPlayer.PlayFabId;
                // pd.IsOnline = true;
                localPlayer.name = "LocalPlayer";
                // localPlayer.transform.SetParent(GameObject.Find("Players").transform);
                GameObject cam = GameObject.Find("Main Camera");
                cam.transform.SetParent(localPlayer.transform);
                cam.transform.localPosition = new Vector3(0f, 0f, cam.transform.position.z);
            }
            else
            {
                var players = GameObject.Find("Players");
                if(PlayFabData.DictPlayerInfos.ContainsKey(PlayFabSettings.staticPlayer.PlayFabId))
                {
                    bool spawned = false;
                    for (int i = 0; i < players.transform.childCount; i++)
                    {
                        if(players.transform.GetChild(i).GetComponent<PlayerData>().PlayFabId.Equals(PlayFabSettings.staticPlayer.PlayFabId))
                        {
                            GameObject target = players.transform.GetChild(i).gameObject;
                            target.GetComponent<ChatSender>().RPC_RequestDespawn(target.GetComponent<NetworkObject>());
                            target.name = "target";
                            Debug.Log("taget " + target.GetComponent<PlayerData>().DisplayName);
                            SpawnLocalPlayer(target.transform.position);
                            spawned = true;
                            break;
                        }
                    }
                    if(spawned == false)
                    {
                        SpawnLocalPlayer(new Vector3(2.5f, -7.5f, 0));
                    }
                }
                else
                {
                    var localPlayer = Runner.Spawn(prefabRef:PlayerPrefab, new Vector3(2.5f, -7.5f, 0), inputAuthority: this.Runner.LocalPlayer);
                    PlayerData pd = localPlayer.GetComponent<PlayerData>();
                    pd.PlayFabId = PlayFabSettings.staticPlayer.PlayFabId;
                    // pd.IsOnline = true;
                    localPlayer.name = "LocalPlayer";
                    // localPlayer.transform.SetParent(GameObject.Find("Players").transform);
                    GameObject cam = GameObject.Find("Main Camera");
                    cam.transform.SetParent(localPlayer.transform);
                    cam.transform.localPosition = new Vector3(0f, 0f, cam.transform.position.z);
                }
            }
        }
        else
        {

        }
        Debug.Log("joined");
    }

    public void Spawn(Vector3 pos, string id)
    {
        if(playerContainer == null)
        {
            playerContainer = GameObject.Find("Players");
        }
        if(PlayerPrefab == default(NetworkPrefabRef)) // PlayerPrefabは何も設定されていない場合、nullにならない
        {
            PlayerPrefab = GameObject.Find("PlayerPrefabs").GetComponent<PlayerPrefabs>().playerPrefabs[0];
        }
        var player = Runner.Spawn(prefabRef:PlayerPrefab, pos, inputAuthority: null);
        // player.transform.SetParent(playerContainer.transform);
        PlayerData pd = player.GetComponent<PlayerData>();
        pd.PlayFabId = id;
        // pd.IsOnline = false;
        pd.DisplayName = PlayFabData.DictPlayerInfos[id].name;
        string texturePath = PlayFabData.DictPlayerInfos[id].texturePath;
        if(texturePath.Length < 17)
        {
            pd.texturePath = texturePath;
            pd.texturePath2 = "";
        }
        else
        {
            pd.texturePath = texturePath.Substring(0, 16);
            pd.texturePath2 = texturePath.Substring(16);
        }
        // Runner.Despawn(player.GetComponent<NetworkObject>());
    }

    private void SpawnLocalPlayer(Vector3 pos)
    {
        var localPlayer = Runner.Spawn(prefabRef:PlayerPrefab, pos, inputAuthority: this.Runner.LocalPlayer);
        PlayerData pd = localPlayer.GetComponent<PlayerData>();
        pd.PlayFabId = PlayFabSettings.staticPlayer.PlayFabId;
        // pd.IsOnline = true;
        localPlayer.name = "LocalPlayer";
        GameObject cam = GameObject.Find("Main Camera");
        cam.transform.SetParent(localPlayer.transform);
        cam.transform.localPosition = new Vector3(0f, 0f, cam.transform.position.z);
        Debug.Log(cam.transform.position + "  " + cam.transform.localPosition);
    }

    private void GetPlayerInfos(string playFabId)
    {
        var request = new GetSharedGroupDataRequest
        {
            SharedGroupId = PlayFabData.CurrentSharedGroupId
        };
        PlayFabClientAPI.GetSharedGroupData(request, 
        result => {
            if(result.Data.ContainsKey("Players")) // あとで変更
            {
                string jsonData = result.Data["Players"].Value;
                PlayFabData.DictPlayerInfos = JsonConvert.DeserializeObject<Dictionary<string, PlayerInfo>>(jsonData);

                if(Runner.SessionInfo.PlayerCount == 1 && Runner.IsSharedModeMasterClient)
                {
                    List<string> ids = new List<string>();
                    List<Vector3> positions = new List<Vector3>();
                    int count = 0;
                    foreach (var info in PlayFabData.DictPlayerInfos)
                    {
                        if(info.Key != playFabId)
                        {
                            ids.Add(info.Key);
                            positions.Add(new Vector3(-6.5f + (count * 2), -0.5f, 0f));
                            count++;
                        }
                    }
                    SpawnAllAI(ids, positions);
                }
            }
        }
        , error => Debug.Log("get失敗: " + error.GenerateErrorReport()));
    }

    public void SpawnAllAI(List<string> Ids, List<Vector3> pos)
    {
        if(Ids.Count != pos.Count || Ids.Count == 0 || pos.Count == 0)
        {
            return;
        }

        for(var i = 0; i < Ids.Count; i++)
        {
            if(!string.IsNullOrEmpty(Ids[i]))
            {
                Spawn(pos[i], Ids[i]);
            }
        }
    }
}
