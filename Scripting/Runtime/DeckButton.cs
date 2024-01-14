
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using VRC.SDKBase;
using VRC.Udon;


namespace Lastation.TOD
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DeckButton : UdonSharpBehaviour
    {
        public URLLoader UIController;
        public TODDeckContainer assignedSet;
        public TextMeshProUGUI buttonText;

        private VRCPlayerApi _player;

        public void Start()
        {
            _player = Networking.LocalPlayer;
        }

        public void OnClick()
        {
            if (UIController._IsMasterLocked && !_player.isMaster) return;
            UIController.LoadSetDataContainer(assignedSet);
        }

        //sets the container this button is for on start()
        public void SetTODSetContainer(TODDeckContainer InputWorldData)
        {
            assignedSet = InputWorldData;
            buttonText.text = assignedSet.presetDeckName;
        }
    }
}
