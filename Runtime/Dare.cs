using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Lastation.TOD
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Dare : UdonSharpBehaviour
    {
        [SerializeField] private GameManager _gameManager;
        private VRCPlayerApi _player;

        public override void Interact()
        {
            Debug.LogError("Dare Start");
            _player = Networking.LocalPlayer;
            Networking.SetOwner(_player, gameObject);
            _gameManager.Dare();
            Debug.LogError("Dare End");
        }
    }
}
