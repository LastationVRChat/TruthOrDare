using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Lastation.TOD
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Truth : UdonSharpBehaviour
    {
        [SerializeField] private GameManager _gameManager;
        private VRCPlayerApi _player;

        public override void Interact()
        {
            Debug.LogError("Truth Start");
            _player = Networking.LocalPlayer;
            Networking.SetOwner(_player, gameObject);
            _gameManager.Truth();
            Debug.LogError("Truth End");
        }
    }
}
