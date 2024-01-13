using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Lastation.TOD
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TODSetContainer : UdonSharpBehaviour
    {
        public string setName; //set in inspector to gen the button name
        public VRCUrl VRCUrl; //address of the json file

        [HideInInspector] public SetButton SetButton;
    }
}
