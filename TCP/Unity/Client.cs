using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Net.Sockets;
using System.Text;

public class Client : MonoBehaviour
{
    public Button connectButton;
    public Button exitButton;
    public Button sendButton;

    public TMP_InputField inputMessage;
    public TMP_Text displayText;

    private TcpClient client;
    private NetworkStream stream;
    private const string ip = "127.0.0.1";
    private const int port = 8000;
    private const int BUFF_SIZE = 1024;
    private byte[] buffer = new byte[BUFF_SIZE];

    void Start()
    {
        connectButton.onClick.AddListener(OnClickConnectButton);
        exitButton.onClick.AddListener(OnClickExitButton);
        sendButton.onClick.AddListener(OnClickSendButton);
        inputMessage.onSubmit.AddListener(ononSubmitInputMessage);
    }

    void ononSubmitInputMessage(string input) // Enter Àü¼Û
    {
        OnClickSendButton();
        inputMessage.text = null;
        inputMessage.ActivateInputField(); // TextInput Focusing
    }

    void OnClickConnectButton()
    {
        try
        {
            client = new TcpClient();
            client.Connect(ip, port);
            stream = client.GetStream();
            displayText.text = $"Sever Connected!\n";
            inputMessage.ActivateInputField();
        }
        catch (SocketException se)
        {
            displayText.text += $"Connection Error\n{se.Message}";
        }
        catch (Exception ex)
        {
            displayText.text += $"Connection Error\n{ex.Message}";
        }
    }

    void OnClickExitButton()
    {
        if (client != null)
        {
            stream.Close();
            client.Close();
            client = null;
            displayText.text += "Disconnected Server";
        }
    }

    void OnClickSendButton()
    {
        if (client == null || !client.Connected)
        {
            displayText.text += "Not Connected!";
            return;
        }

        try
        {
            string message = inputMessage.text;
            if (string.IsNullOrEmpty(message)) return;

            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);

            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            displayText.text += $"[Server]\n{receivedMessage}\n";
            inputMessage.text = null;
            inputMessage.ActivateInputField();
        }
        catch (SocketException se)
        {
            displayText.text = $"Connection Error\n{se.Message}";
        }
        catch (Exception ex)
        {
            displayText.text = $"Connection Error\n{ex.Message}";
        }
    }
}
