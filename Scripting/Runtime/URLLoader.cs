using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace Lastation.TOD
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class URLLoader : UdonSharpBehaviour
    {
        #region Variables & Data
        [SerializeField] public TODDeckContainer[] _setContainers;

        [Space]

        [Header("Game Instance")]
        public GameManagerV2 gameManager;

        [Space]

        [Header("Button Instancing")]
        [SerializeField] private Transform _buttonParent;
        [SerializeField] private GameObject _buttonPrefab;
        private Button[] _presetButtons;

        [Space]

        [Header("URL Input")]
        [SerializeField] private VRCUrl _defaultURL;
        [SerializeField] private VRCUrlInputField _urlInputField;

        [Space]

        [Header("Master Lock")]
        [SerializeField] private Toggle _masterLockToggle;
        [UdonSynced] public bool _IsMasterLocked = true;

        [Space]

        [Header("Loaded Set Info")]
        [SerializeField] private TextMeshProUGUI _deckName;
        [SerializeField] private TextMeshProUGUI _deckBy;
        [SerializeField] private TextMeshProUGUI _truthCount;
        [SerializeField] private TextMeshProUGUI _playerTruthCount;
        [SerializeField] private TextMeshProUGUI _dareCount;
        [SerializeField] private TextMeshProUGUI _playerDareCount;

        [Header("StatusCode Handling")]
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private AudioSource _uIAudioSource;
        [SerializeField] private AudioClip _errorClip;

        //Internal & Synced Variables
        private VRCPlayerApi _player;
        [UdonSynced] private VRCUrl _loadedURL;

        #endregion Variables & Data

        #region Start, Master & Serialization
        void Start()
        {
            _player = Networking.LocalPlayer;
            GenerateButtons();
            #region Button Caching
            _presetButtons = new Button[_setContainers.Length];
            for (int i = 0; i < _setContainers.Length; i++)
            {
                _presetButtons[i] = _setContainers[i].SetButton.GetComponent<Button>();
            }
            #endregion Button Caching
            #region Default URL Loader
            if (_loadedURL == null && _defaultURL != null)
            {
                _loadedURL = _defaultURL;
                RequestSerialization();
                LoadURL(_loadedURL);
            }
            #endregion Default URL Loader
        }

        public override void OnDeserialization()
        {   
            _masterLockToggle.SetIsOnWithoutNotify(_IsMasterLocked);
            LoadURL(_loadedURL);
        }

        public void MasterSwitch()
        {
            if (_IsMasterLocked && !_player.isMaster)
            {
                _masterLockToggle.SetIsOnWithoutNotify(_IsMasterLocked);
                StatusCode("MasterLocked");
                return;
            }
            _IsMasterLocked = !_IsMasterLocked;
            _masterLockToggle.SetIsOnWithoutNotify(_IsMasterLocked);
            RequestSerialization();
        }

        #endregion Start, Master & Serialization

        #region Button Generation
        public void GenerateButtons()
        {
            foreach (TODDeckContainer containerInstance in _setContainers)
            {
                if (containerInstance.SetButton == null)
                {
                    GameObject button = Instantiate(_buttonPrefab, _buttonParent);
                    button.GetComponent<DeckButton>().SetTODSetContainer(containerInstance);
                    containerInstance.SetButton = button.GetComponent<DeckButton>();
                    button.SetActive(true);
                    if (containerInstance.isNSFW) button.SetActive(false);
                }
            }
        }
        #endregion Button Generation

        #region URL Loading
        public void LoadURL(VRCUrl url)
        {
            VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);
        }

        public void LoadSetDataContainer(TODDeckContainer containerInstance)
        {
            Networking.SetOwner(_player, gameObject);
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(EnableRateLimit));
            _loadedURL = containerInstance.presetDeckURL;
            LoadURL(_loadedURL);
            RequestSerialization();
        }

        public void RequestURL()
        {
            if (_IsMasterLocked && !_player.isMaster)
            {
                StatusCode("MasterLocked");
                return;
            }
            Networking.SetOwner(_player, gameObject);
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(EnableRateLimit));
            _loadedURL = _urlInputField.GetUrl();
            LoadURL(_loadedURL);
            RequestSerialization();
        }
        #endregion URL Loading

        #region String Load Events
        public override void OnStringLoadSuccess(IVRCStringDownload WebRequest)
        {
            string json = WebRequest.Result;
            Debug.Log($"Successfully downloaded json {json}");

            if (VRCJson.TryDeserializeFromJson(json, out DataToken result))
            {
                //Currently a dictionaty with 6 items
                result.DataDictionary.TryGetValue("DeckName", out DataToken deckName);
                result.DataDictionary.TryGetValue("DeckBy", out DataToken deckBy);
                result.DataDictionary.TryGetValue("Truths", out DataToken truths);
                result.DataDictionary.TryGetValue("Player_Truths", out DataToken pTruths);
                result.DataDictionary.TryGetValue("Dares", out DataToken dares);
                result.DataDictionary.TryGetValue("Player_Dares", out DataToken pDares);


                _deckName.text = deckName.String;
                _deckBy.text = deckBy.String;
                _truthCount.text = truths.DataList.Count.ToString();
                _playerTruthCount.text = pTruths.DataList.Count.ToString();
                _dareCount.text = dares.DataList.Count.ToString();
                _playerDareCount.text = pDares.DataList.Count.ToString();

                //all tokens below are datalists of x items
                gameManager._truths = truths.DataList;
                gameManager._pTruths = pTruths.DataList;
                gameManager._dares = dares.DataList;
                gameManager._pDares = pDares.DataList;

                gameManager.playerDisplayedText.text = deckName.String;
                gameManager.questionDisplayedText.text = "By " + deckBy.String;

                StatusCode("Loaded");

                SendCustomEventDelayedSeconds(nameof(DisableRateLimit), 10);
            }

        }

        public override void OnStringLoadError(IVRCStringDownload WebRequest)
        {
            StatusCode("LoadError");
            gameManager.playerDisplayedText.text = "StatusCode " + WebRequest.ErrorCode.ToString();
            gameManager.questionDisplayedText.text = WebRequest.Error;
            SendCustomEventDelayedSeconds(nameof(DisableRateLimit), 10);
        }
        #endregion String Load Events

        #region Rate Limiting
        public void EnableRateLimit()
        {
            _urlInputField.interactable = false;
            for (int i = 0; i < _presetButtons.Length; i++)
            {
                _presetButtons[i].interactable = false;
            }
        }

        public void DisableRateLimit()
        {
            _urlInputField.interactable = true;
            for (int i = 0; i < _presetButtons.Length; i++)
            {
                _presetButtons[i].interactable = true;
            }
        }

        #endregion Rate Limiting

        #region Error Handling
        public void StatusCode(string status)
        {
            switch (status)
            {
                case "MasterLocked":
                    _statusText.text = "[MasterLocked] Only instance master can currently do this.";
                    _statusText.color = Color.red;
                    _uIAudioSource.PlayOneShot(_errorClip);
                    break;
                case "LoadError":
                    _statusText.text = "Failed to Load JSON!";
                    _statusText.color = Color.red;
                    _uIAudioSource.PlayOneShot(_errorClip);
                    break;
                case "Loaded":
                    _statusText.text = "Deck Loaded!";
                    _statusText.color = Color.green;
                    break;
                case "NotSupporter":
                    _statusText.text = "You are not a Supporter";
                    _statusText.color = Color.red;
                    _uIAudioSource.PlayOneShot(_errorClip);
                    break;
                case "NSFWUnlock":
                    _statusText.text = "NSFW Decks Unlocked!";
                    _statusText.color = Color.green;
                    break;
            }
        }
        #endregion Error Handling
    }

}
