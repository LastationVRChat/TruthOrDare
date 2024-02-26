using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using Random = UnityEngine.Random;

namespace Lastation.TOD
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class GameManagerV2 : UdonSharpBehaviour
    {
        #region Variables & Data

        [SerializeField] private PlayerManager playerManager;

        [Space]

        [Header("Displays")]
        [SerializeField] public TextMeshProUGUI questionDisplayedText;
        [SerializeField] public TextMeshProUGUI playerDisplayedText;

        [Space]

        [Header("Game Settings")]
        [SerializeField] private string randomPlayer = "<player>";
        [SerializeField] private string localPlayer = "<local>";
        [SerializeField] private string range = "<range[";


        // Truth & Dares
        [HideInInspector] public DataList _truths = new DataList();
        [HideInInspector] public DataList _dares = new DataList();

        // Game Logic & Local Player
        private VRCPlayerApi _player;
        private int _id;

        //range Data
        private int startRange;
        private int endRange;
        private int value;
        private int startIndex;

        // Synced Data
        [UdonSynced] private string _question;
        [UdonSynced] public int _playerID = -1;

        #endregion Variables & Data

        #region Start
        void Start()
        {
            _player = Networking.LocalPlayer;
        }
        #endregion Start

        #region Truth
        public void Truth()
        {
            Networking.SetOwner(_player, gameObject);
            _playerID = _player.playerId;

            _id = Random.Range(0, _truths.Count);
            SetQuestion(1);
        }
        #endregion Truth

        #region Dare
        public void Dare()
        {
            Networking.SetOwner(_player, gameObject);
            _playerID = _player.playerId;

            _id = Random.Range(0, _dares.Count);
            SetQuestion(2);
        }
        #endregion Dare

        #region Set Question & Range Processing

        private void SetQuestion(int value)
        {
            switch (value)
            {
                case 1:
                    _question = _truths[_id].String;
                    if (_question.Contains(randomPlayer) && playerManager.PlayerCount >= 3) _question = _question.Replace(randomPlayer, playerManager.GetRandomPlayer());
                    else if (playerManager.PlayerCount < 3 && _question.Contains(randomPlayer))
                    {
                        Truth();
                        Debug.Log("Truth was forced by playermanager");
                    }
                    if (_question.Contains(localPlayer)) _question = _question.Replace(localPlayer, _player.displayName);
                    if (_question.Contains(range)) RangeProcessing();
                    break;
                case 2:
                    _question = _dares[_id].String;
                    if (_question.Contains(randomPlayer) && playerManager.PlayerCount >= 3) _question = _question.Replace(randomPlayer, playerManager.GetRandomPlayer());
                    else if (playerManager.PlayerCount < 3 && _question.Contains(randomPlayer))
                    {
                        Dare();
                        Debug.Log("Dare was forced by playermanager");
                    }
                    if (_question.Contains(localPlayer)) _question = _question.Replace(localPlayer, _player.displayName);
                    if (_question.Contains(range)) RangeProcessing();
                    break;
            }
            _UpdateQuestion();
        }

        public void RangeProcessing()
        {

            startRange = 0;
            endRange = 0;
            value = 0;
            startIndex = _question.IndexOf(range);

            if (startIndex != -1)
            {
                int _tmpIndex = startIndex + range.Length;

                while (_tmpIndex < _question.Length && Char.IsDigit(_question[_tmpIndex]))
                {
                    startRange = startRange * 10 + (_question[_tmpIndex] - '0');
                    _tmpIndex++;
                }

                _tmpIndex++;

                while (_tmpIndex < _question.Length && Char.IsDigit(_question[_tmpIndex]))
                {
                    endRange = endRange * 10 + (_question[_tmpIndex] - '0');
                    _tmpIndex++;
                }

                value = Random.Range(startRange, endRange + 1);

                int endIndex = _question.IndexOf("]>", startIndex);
                if (endIndex != -1)
                {
                    _question = _question.Remove(startIndex, endIndex - startIndex + 2);
                    _question = _question.Insert(startIndex, value.ToString());
                }
            }
        }
        #endregion Set Question & Range Processing

        #region Update Question and Serialization
        private void _UpdateQuestion()
        {
            playerDisplayedText.text = VRCPlayerApi.GetPlayerById(_playerID).displayName;
            questionDisplayedText.text = _question;
            RequestSerialization();
        }

        public override void OnDeserialization()
        {
            if (!Utilities.IsValid(VRCPlayerApi.GetPlayerById(_playerID))) return;
            playerDisplayedText.text = VRCPlayerApi.GetPlayerById(_playerID).displayName;
            questionDisplayedText.text = _question;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            RequestSerialization();
        }
        #endregion Update Question and Serialization
    }
}
