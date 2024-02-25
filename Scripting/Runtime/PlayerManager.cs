using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using Random = UnityEngine.Random;

namespace Lastation.TOD
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerManager : UdonSharpBehaviour
    {
        [Header("Script References")]
        [SerializeField] private GameManagerV2 gameManager;

        [Header("Button References")]
        [SerializeField] private Button joinButton;
        [SerializeField] private Button leaveButton;

        [Header("Player Display")]
        public GameObject[] templates;
        private TextMeshProUGUI[] _templateNames;

        [UdonSynced] private int[] _joinedPlayerIDs = new int[] { -1 };

        private VRCPlayerApi _LocalPlayer;
        private bool _isRateLimited;
        private int[] _nonLocalPlayers = new int[0];

        private void Start()
        {
            _LocalPlayer = Networking.LocalPlayer;
            _templateNames = new TextMeshProUGUI[templates.Length];
            for (int i = 0; i < templates.Length; i++)
            {
                _templateNames[i] = templates[i].GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        public int PlayerCount => _joinedPlayerIDs.Length;

        public void RateLimit()
        {
            _isRateLimited = false;
            joinButton.interactable = true;
            leaveButton.interactable = true;
        }

        public string GetRandomPlayer()
        {
            if (_nonLocalPlayers.Length == 0) return null;
            int randomIndex = Random.Range(0, _nonLocalPlayers.Length);
            return VRCPlayerApi.GetPlayerById(_nonLocalPlayers[randomIndex]).displayName;
        }

        public void ButtonFlipper()
        {
            joinButton.gameObject.SetActive(!joinButton.gameObject.activeSelf);
            leaveButton.gameObject.SetActive(!joinButton.gameObject.activeSelf);
        }

        public void Join()
        {
            if (!Utilities.IsValid(_LocalPlayer) || _isRateLimited) return;
            Networking.SetOwner(_LocalPlayer, gameObject);

            _isRateLimited = true;
            ButtonFlipper();
            leaveButton.interactable = false;
            SendCustomEventDelayedSeconds(nameof(RateLimit), 5);

            AddPlayer(_LocalPlayer.playerId);
        }

        public void Leave()
        {
            if (!Utilities.IsValid(_LocalPlayer) || _isRateLimited) return;
            Networking.SetOwner(_LocalPlayer, gameObject);

            _isRateLimited = true;
            ButtonFlipper();
            joinButton.interactable = false;
            SendCustomEventDelayedSeconds(nameof(RateLimit), 5);

            RemovePlayer(_LocalPlayer.playerId);
        }

        private void UpdateNonLocalPlayers()
        {
            int count = 0;
            for (int i = 0; i < _joinedPlayerIDs.Length; i++)
            {
                if (_joinedPlayerIDs[i] != _LocalPlayer.playerId)
                {
                    count++;
                }
            }
            _nonLocalPlayers = new int[count];
            count = 0;
            for (int i = 0; i < _joinedPlayerIDs.Length; i++)
            {
                if (_joinedPlayerIDs[i] != _LocalPlayer.playerId)
                {
                    _nonLocalPlayers[count] = _joinedPlayerIDs[i];
                    count++;
                }
            }
        }

        private void UpdateDisplay()
        {
            for (int i = 0; i < templates.Length; i++)
            {
                if (i < _joinedPlayerIDs.Length && _joinedPlayerIDs[i] != -1)
                {
                    string playerName = VRCPlayerApi.GetPlayerById(_joinedPlayerIDs[i]).displayName;
                    if (!string.IsNullOrEmpty(playerName))
                    {
                        _templateNames[i].text = playerName;
                        templates[i].SetActive(true);
                    }
                }
                else
                {
                    templates[i].SetActive(false);
                }
            }
        }

        public override void OnDeserialization()
        {
            UpdateNonLocalPlayers();
            UpdateDisplay();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            RequestSerialization();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (Array.IndexOf(_joinedPlayerIDs, player.playerId) != -1)
            {
                int index = Array.IndexOf(_joinedPlayerIDs, player.playerId);
                int[] newArray = new int[_joinedPlayerIDs.Length - 1];
                Array.Copy(_joinedPlayerIDs, 0, newArray, 0, index);
                Array.Copy(_joinedPlayerIDs, index + 1, newArray, index, _joinedPlayerIDs.Length - index - 1);
                _joinedPlayerIDs = newArray;

                UpdateNonLocalPlayers();
                UpdateDisplay();
                RequestSerialization();
            }
        }

        public void AddPlayer(int playerId)
        {
            if (Array.IndexOf(_joinedPlayerIDs, playerId) == -1)
            {
                int[] newArray = new int[_joinedPlayerIDs.Length + 1];
                Array.Copy(_joinedPlayerIDs, newArray, _joinedPlayerIDs.Length);
                newArray[_joinedPlayerIDs.Length] = playerId;
                _joinedPlayerIDs = newArray;

                UpdateNonLocalPlayers();
                UpdateDisplay();
                RequestSerialization();
            }
        }

        public void RemovePlayer(int playerId)
        {
            if (Array.IndexOf(_joinedPlayerIDs, playerId) != -1)
            {
                int index = Array.IndexOf(_joinedPlayerIDs, playerId);
                int[] newArray = new int[_joinedPlayerIDs.Length - 1];
                Array.Copy(_joinedPlayerIDs, 0, newArray, 0, index);
                Array.Copy(_joinedPlayerIDs, index + 1, newArray, index, _joinedPlayerIDs.Length - index - 1);
                _joinedPlayerIDs = newArray;

                UpdateNonLocalPlayers();
                UpdateDisplay();
                RequestSerialization();
            }
        }
    }
}
