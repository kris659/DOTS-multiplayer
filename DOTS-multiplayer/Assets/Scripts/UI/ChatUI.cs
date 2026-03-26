using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : UIWindow
{
    [Header("Messages")]
    [SerializeField] private GameObject _messagePrefab;
    [SerializeField] private Transform _messagesParent;
    [SerializeField] private Color _normalMessageColor;
    [SerializeField] private Color _privateMessageColor;

    [Header("Messages Sending")]
    [SerializeField] private TMP_Dropdown _messageDestinationDropdown;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _sendButton;


    private readonly List<GameObject> _messages = new();
    private List<int> _dropdownNetworkIds = new();


    private readonly int MAX_MESSAGES = 10;

    protected override void Awake()
    {
        base.Awake();
        _messagesParent.DestroyAllChildren(includeInactive: true);
        _sendButton.onClick.AddListener(SendMessage);
        PlayersData.PlayersDataChanged += UpdateMessageDestinationDropdown;
        Close();
    }

    private void SendMessage()
    {
        ChatManager.SendMessage(_inputField.text, _dropdownNetworkIds[_messageDestinationDropdown.value]);
        _inputField.text = string.Empty;
    }

    private void UpdateMessageDestinationDropdown()
    {
        List<string> options = new() { "All" };
        _dropdownNetworkIds = new() { 0 };

        foreach (var player in PlayersData.PlayerDataDictionary)
        {
            if (player.Key == PlayersData.LocalPlayer)
                continue;
            options.Add(player.Value.Name.ToString());
            _dropdownNetworkIds.Add(player.Value.NetworkId);
        }
        _messageDestinationDropdown.ClearOptions();
        _messageDestinationDropdown.AddOptions(options);
    }


    public void AddMessage(ReceiveMessageRequest message)
    {
        GameObject messageGO = Instantiate(_messagePrefab, _messagesParent);
        TMP_Text text = messageGO.GetComponentInChildren<TMP_Text>();

        string senderName = PlayersData.GetPlayerDataByNetworkId(message.SenderNetworkId).Name.ToString();

        if (message.ReceiverNetworkId == 0)
        {
            text.text = $"[{senderName}]: {message.Message}";
            text.color = _normalMessageColor;
        }
        else
        {
            string receiverName = PlayersData.GetPlayerDataByNetworkId(message.ReceiverNetworkId).Name.ToString();
            text.text = $"[{senderName} > {receiverName}]: {message.Message}";
            text.color = _privateMessageColor;
        }

        _messages.Add(messageGO);
        if (_messages.Count > MAX_MESSAGES)
        {
            Destroy(_messages[0]);
            _messages.RemoveAt(0);
        }
    }
}
