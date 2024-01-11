using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Lastation.TOD
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class TODInteract : UdonSharpBehaviour
    {
        public UdonBehaviour _gameManager;
        public string eventName;
        public override void Interact()
        {
            Debug.LogError("Button Start");
            _gameManager.SendCustomEvent(eventName);
            Debug.LogError("Button End");
        }
    }
}
