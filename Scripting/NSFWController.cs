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
    public class NSFWController : UdonSharpBehaviour
    {
        #region Variables
        [Header("Script References")]
        [SerializeField] private PluginManager manager;
        [SerializeField] private URLLoader urlLoader;
        [Header("UI References")]
        [SerializeField] private GameObject _mainUi;
        [SerializeField] private GameObject _nSFWUi;
        [Header("Button References")]
        [SerializeField] private Button _nSFWButton;
        [SerializeField] private Button supporterButton;
        #endregion Variables

        #region Disbridge Checks
        private void Start()
        {
            manager.AddPlugin(gameObject);
        }

        public void _UVR_Init()
        {
            supporterButton.interactable = true;
        }

        public void SupporterCheck()
        {
            if (manager.IsStaff(Networking.LocalPlayer))
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ToggleNSFW));
                return;
            }
            else if (manager.IsSupporter(Networking.LocalPlayer))
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ToggleNSFW));
                return;
            }
            else
            {
                ToggleNSFWUI();
                urlLoader.StatusCode("NotSupporter");
            }
        }
        #endregion Disbridge Checks

        #region Lastation.Auth Checks
        public void AuthCheck()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ToggleNSFW));
        }   
        #endregion Lastation.Auth Checks

        #region UI Handling
        public void ToggleNSFWUI()
        {
            _nSFWUi.SetActive(!_nSFWUi.activeSelf);
            _mainUi.SetActive(!_mainUi.activeSelf);
        }

        public void ToggleNSFW()
        {
            foreach (TODDeckContainer containerInstance in urlLoader._setContainers)
            {
                if (containerInstance.isNSFW)
                {
                    containerInstance.SetButton.gameObject.SetActive(true);
                }
            }
            ToggleNSFWUI();
            _nSFWButton.interactable = false;
            urlLoader.StatusCode("NSFWUnlock");
        }
        #endregion UI Handling
    }
}