using System;
using Fusion;
using Interface;
using UnityEngine;

namespace CustomConnectionHandler {
    [RequireComponent(typeof(BoxCollider))]
    public class ConnectionGate : MonoBehaviour {
        public ConnectionData ConnectionData;
        private float _lastTimeInside;


        private void OnTriggerStay(Collider other)
        {
            if (other.GetComponentInParent<NetworkObject>().HasInputAuthority)
            {
                _lastTimeInside = Time.time;
                InterfaceManager.Instance.GateUI.ShowGate(ConnectionData);
            }
        }

        private void LateUpdate()
        {
            if (Time.time - _lastTimeInside >= .5f)
            {
                InterfaceManager.Instance.GateUI.Defocus();
            }
        }
    }
}
