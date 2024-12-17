using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;


public class RuleBaseAI : MonoBehaviour 
{
    public enum State
    {
        Idle,
        Chat,
        Sit,
        Follow,
        Question,
        Bored
    }

    private PlayerData pd;
    private PlayerMovement pm;

    public State state = State.Idle;

    // 各Stateの選択確率（0.0〜1.0の範囲で設定）
    private float[] stateProbabilities = { 0.1f, 0.00f, 0.4f, 0.0f, 0.00f, 0.5f }; // Idle, Chat, Sit, Follow, Question, Bored


    private float deltaTime = 0f;
    private const float interval = 5f;

    void Start()
    {
        pd = GetComponent<PlayerData>();
        pm = GetComponent<PlayerMovement>();
        if(!pd.IsAI && !PlayFabData.CurrentRoomPlayersRefs[PlayFabSettings.staticPlayer.PlayFabId].IsHost)
        {
            return;
        }
    }

    void Update()
    {
        if(!pd.IsAI && !PlayFabData.CurrentRoomPlayersRefs[PlayFabSettings.staticPlayer.PlayFabId].IsHost)
        {
            return;
        }
        deltaTime += Time.deltaTime;
        deltaTime += (float)(Random.value - 0.5f) * 0.1f;

        if(deltaTime >= interval)
        {
            if(pd.Q_nextInputs.Count == 0)
            {
                if(state == State.Sit || state == State.Bored || state == State.Follow || state == State.Question)
                {
                    float value = Random.value;
                    float standardValue = 0.5f;
                    if(state == State.Sit)
                    {
                        standardValue = 0.7f;
                    }
                    if(value < standardValue)
                    {
                        ChangeState(state);
                    }
                    else
                    {
                        ChangeState(GetRandomStateByProbability());
                    }
                }
                else
                {
                    ChangeState(GetRandomStateByProbability());
                }

                switch(state)
                {
                    case State.Idle:
                        OnIdleState();
                        break;
                    case State.Chat:
                        break;
                    case State.Sit:
                        OnSit();
                        break;
                    case State.Follow:
                        // OnFollow();
                        break;
                    case State.Question:
                        break;
                    case State.Bored:
                        OnBored();
                        break;
                    default:
                        OnIdleState();
                        break;
                }
                deltaTime = 0f;
            }
        }
    }

    private void ChangeState(State newState)
    {
        state = newState;
    }

    // 確率に基づいてランダムな状態を取得
    private State GetRandomStateByProbability()
    {
        float randomValue = Random.value; // 0.0〜1.0のランダム値
        float cumulativeProbability = 0f;

        for (int i = 0; i < stateProbabilities.Length; i++)
        {
            cumulativeProbability += stateProbabilities[i];
            if (randomValue < cumulativeProbability)
            {
                return (State)i;
            }
        }

        return State.Idle; // デフォルトでIdleを返す
    }

    private void OnIdleState()
    {
        int value = Random.Range(3, 31);
        for (int i = 0; i < value; i++)
        {
            pd.Q_nextInputs.Enqueue(-1);
        }
    }

    private void OnSit()
    {
        Vector3 targetPos = PlayFabData.ChairPos[transform.GetSiblingIndex()];
        pm.MoveToTargetPos(transform.position, targetPos, (int)MyNetworkInput.InputType.RIGHT);
    }

    private void OnFollow()
    {
        string nearTarget = "";
        float minDist = float.PositiveInfinity;
        foreach(var target in PlayFabData.DictDistance)
        {
            float dist = (target.Value - transform.position).magnitude;
            if(target.Key != pd.PlayFabId && dist < minDist)
            {
                nearTarget = target.Key;
            }
        }

        pm.MoveToTargetPos(transform.position, PlayFabData.CurrentRoomPlayersRefs[nearTarget].transform.position, -1);
    }

    private void OnBored()
    {
        int x = Random.Range(-6, 6);
        int y = Random.Range(-6, 6);
        Vector3 targetPos = transform.position + new Vector3(x, y, 0f);
        pm.MoveToTargetPos(transform.position, targetPos, -1);
    }
}
