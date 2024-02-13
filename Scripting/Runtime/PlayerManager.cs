using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using Random = UnityEngine.Random;

namespace Lastation.TOD
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerManager : UdonSharpBehaviour
    {
        #region Variables & Data
        [Header("Script References")]
        [SerializeField] private GameManagerV2 gameManager;

        [Header("Button References")]
        [SerializeField] private Button joinButton;
        [SerializeField] private Button leaveButton;

        [Header("Player Display")]
        public GameObject[] templates;
        private TextMeshProUGUI[] _templateNames;

        // Synced Data
        [UdonSynced] private int[] _joinedPlayerIDs;

        // Local Data
        private VRCPlayerApi _localPlayer;
        private bool _isRateLimited;
        private int[] _nonLocalPlayers;
        #endregion Variables & Data

        #region Start, RateLimit, GetRandomPlayer, and GetPlayerCount
        void Start()
        {
            #region Cache References
            _templateNames = new TextMeshProUGUI[templates.Length];
            for (int i = 0; i < templates.Length; i++)
            {
                _templateNames[i] = templates[i].GetComponentInChildren<TextMeshProUGUI>();
            }
            #endregion Cache References

            _joinedPlayerIDs = new[] { -1 };
            _localPlayer = Networking.LocalPlayer;
        }

        public int PlayerCount //number of players opted in
        {
            get
            {
                return _nonLocalPlayers.Length;
            }
        }

        public void RateLimit()
        {
            _isRateLimited = false;
            joinButton.interactable = true;
            leaveButton.interactable = true;
        }

        public string GetRandomPlayer()
        {
            int randomIndex = Random.Range(0, _nonLocalPlayers.Length);

            return VRCPlayerApi.GetPlayerById(_nonLocalPlayers[randomIndex]).displayName;
        }
        #endregion Start, RateLimit, GetRandomPlayer, and GetPlayerCount

        #region Join & Leave

        public void ButtonFlipper()
        {
            joinButton.gameObject.SetActive(!joinButton.gameObject.activeSelf);
            leaveButton.gameObject.SetActive(!joinButton.gameObject.activeSelf);
        }

        public void Join()
        {
            if (!Utilities.IsValid(_localPlayer) || _isRateLimited) return;
            Networking.SetOwner(_localPlayer, gameObject);

            _isRateLimited = true;
            ButtonFlipper();
            leaveButton.interactable = false;
            SendCustomEventDelayedSeconds(nameof(RateLimit), 5);

            Add(_localPlayer.playerId);
        }

        public void Leave()
        {
            if (!Utilities.IsValid(_localPlayer) || _isRateLimited) return;
            Networking.SetOwner(_localPlayer, gameObject);

            _isRateLimited = true;
            ButtonFlipper();
            joinButton.interactable = false;
            SendCustomEventDelayedSeconds(nameof(RateLimit), 5);

            Remove(_localPlayer.playerId);
        }
        #endregion Join & Leave

        #region Update nonLocalPlayers
        public void _UpdateList()
        {
            if (_joinedPlayerIDs.Length == 1) return;

            int[] _temp = new int[_joinedPlayerIDs.Length - 1];

            int j = 0;

            for (int i = 0; i < _joinedPlayerIDs.Length; i++)
            {
                if (_joinedPlayerIDs[i] == _localPlayer.playerId) continue;
                _temp[j++] = _joinedPlayerIDs[i];
            }

            _nonLocalPlayers = _temp;
        }
        #endregion Update nonLocalPlayers

        #region Update Joined Player Display
        public void _UpdateDisplay()
        {
            if (_joinedPlayerIDs[0] == -1)
            {
                foreach (GameObject Template in templates)
                {
                    Template.SetActive(false);
                }
                return;
            }

            foreach (GameObject Template in templates)
            {
                Template.SetActive(false);
            }

            for (int i = 0; i < _joinedPlayerIDs.Length; i++)
            {
                string playerName = _localPlayer.displayName;
                if (string.IsNullOrEmpty(playerName)) continue;

                _templateNames[i].text = playerName;
                templates[i].SetActive(true);
            }
        }
        #endregion Update Joined Player Display

        #region Deserialization & PlayerJoined
        public override void OnDeserialization()
        {
                _UpdateList();
                _UpdateDisplay();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            RequestSerialization();
        }
        #endregion Deserialization & PlayerJoined

        #region Add & Remove
        public void Add(int id)
        {
            if (_joinedPlayerIDs.Length == 1 && _joinedPlayerIDs[0] == -1)
            {
                _joinedPlayerIDs[0] = id;
                RequestSerialization();
                _UpdateList();
                _UpdateDisplay();
            }
            else
            {
                int[] _temp = new int[_joinedPlayerIDs.Length + 1];

                Array.Copy(_joinedPlayerIDs, _temp, _joinedPlayerIDs.Length);

                _temp[_temp.Length - 1] = id;

                _joinedPlayerIDs = _temp;
                RequestSerialization();
                _UpdateList();
                _UpdateDisplay();
            }
        }

        public void Remove(int id)
        {
            if (_joinedPlayerIDs.Length == 1 && _joinedPlayerIDs[0] != -1)
            {
                _joinedPlayerIDs[0] = -1;
                RequestSerialization();
                _UpdateList();
                _UpdateDisplay();
            }
            else
            {
                int[] _temp = new int[_joinedPlayerIDs.Length - 1];
                int g = 0;

                for (int i = 0; i < _joinedPlayerIDs.Length; i++)
                {

                    if (_joinedPlayerIDs[i] == id) continue;
                    _temp[g++] = _joinedPlayerIDs[i];
                }

                _joinedPlayerIDs = _temp;
                RequestSerialization();
                _UpdateList();
                _UpdateDisplay();
            }
        }

        #endregion
    }
}
