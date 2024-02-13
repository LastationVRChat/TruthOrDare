using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UdonVR.DisBridge;
using UnityEngine.UI;
using VRC.Udon.Common.Interfaces;

namespace Lastation.TOD
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class DisbrigeTODPlug : UdonSharpBehaviour
    {
        public PluginManager manager;
        public Button supporterButton;
        public URLLoader urlLoader;

        private void Start()
        {
            manager.AddPlugin(gameObject);
        }

        public void _UVR_Init()
        {
            supporterButton.interactable = true;
        }

        public void NSFWEvent()
        {
            urlLoader.ToggleNSFW();
        }

        public void SupporterCheck()
        {
            if (manager.IsStaff(Networking.LocalPlayer))
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(NSFWEvent));
                return;
            }
            else if (manager.IsSupporter(Networking.LocalPlayer))
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(NSFWEvent));
                return;
            }
            else
            {
                urlLoader.ToggleNSFWUI();
                urlLoader.StatusCode("NotSupporter");
            }
        }
    }
}

