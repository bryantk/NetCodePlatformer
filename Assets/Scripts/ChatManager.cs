using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public TMPro.TMP_InputField ChatBox;
    public TMPro.TMP_Text ChatWindow;

    private List<string> MessageList;
    private int TotalMessagesReceived = 0;

    public void Awake()
    {
        MessageList = new List<string>() { "", "", "", "", "" };
        UpdateChatWindow();
    }
    /// <summary>
    /// Message typed into chat window
    /// </summary>
    public void ChatMessageEntered()
    {
        // Send chat message to server/clients
        ChatMessageReceived(ChatBox.text, Servicer.Instance.Netcode.ConnectionID, true);
        // Reset message text to empty
        ChatBox.text = "";
        ChatBox.Select();
    }

    /// <summary>
    /// Receive chat message from server
    /// </summary>
    public void ChatMessageReceived(string message, int senderID, bool broadcast = false)
    {
        string color = Servicer.Instance.Netcode.ConnectionID == senderID ? "blue" : "red";
        string playerName = Servicer.Instance.Netcode.ConnectionID == senderID ? "<Self>" : "<Other>";
        string finalMessage = "<color=" + color + ">" + playerName + message + "</color>";

        // Set Message in MessageList
        if (TotalMessagesReceived < 5)
        {
            int arrayPosition = 4 - TotalMessagesReceived;
            for (int i= arrayPosition + 1; i < 5; ++i)
            {
                MessageList[i - 1] = MessageList[i];
            }
            MessageList[4] = finalMessage;
        }
        else
        {
            MessageList.RemoveAt(0);
            MessageList.Add(finalMessage);
        }
        TotalMessagesReceived++;

        // Update chat window
        UpdateChatWindow();

        // Send message to server if it's from self
        if (broadcast)
        {
            Servicer.Instance.Netcode.SendData(
                (short)NetcodeMsgType.ChatMessage,
                message);
        }
    }

    private void UpdateChatWindow()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (string chatMessage in MessageList)
            sb.Append(chatMessage + "\n");
        ChatWindow.text = sb.ToString();
    }
}
