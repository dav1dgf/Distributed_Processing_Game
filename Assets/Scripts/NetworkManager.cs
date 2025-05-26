using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using TMPro;

public class NetworkManager : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Queue<string> incomingMessages = new Queue<string>();
    private bool gameStarted = false;

    // Para almacenar la información de los jugadores
    public int playerId;
    private Vector2 playerPosition;
    private int playerHealth;
    private Vector2 enemyPosition;
    public TMP_Text text;
    [SerializeField] private TurnManager turnManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async void Start()
    {
        await TryConnectToServerRepeatedly();
        await ListenForServerMessages();
    }

    private async Task TryConnectToServerRepeatedly()
    {
        while (client == null || !client.Connected)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync("172.20.10.2", 65432);
                stream = client.GetStream();
                Debug.Log("Conectado al servidor.");
                text.text = "Connected, waiting for player...";
            }
            catch (SocketException e)
            {
               // Debug.LogWarning("No se pudo conectar al servidor. Reintentando en 0.5s...");
                await Task.Delay(500);
            }
        }
    }

    private async Task ListenForServerMessages()
    {
        byte[] buffer = new byte[1024];
        StringBuilder messageBuffer = new StringBuilder();

        while (true)
        {
            int length;
            try
            {
                length = await stream.ReadAsync(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
                Debug.LogError("Red caida: " + e);
                break;
            }

            if (length == 0) continue;

            messageBuffer.Append(Encoding.ASCII.GetString(buffer, 0, length));

            while (true)
            {
                string allMessages = messageBuffer.ToString();
                int newlineIndex = allMessages.IndexOf('\n');
                if (newlineIndex == -1) break;

                string msg = allMessages.Substring(0, newlineIndex).Trim();
                messageBuffer.Remove(0, newlineIndex + 1);

                HandleServerMessage(msg);
            }
        }
    }

    private void HandleServerMessage(string msg)
    {
        Debug.Log("Servidor dijo: " + msg);
        NetworkMessage baseMsg = null;
        try
        {
            baseMsg = JsonUtility.FromJson<NetworkMessage>(msg);
        }
        catch
        {
            Debug.LogWarning("Mensaje no reconocido: " + msg);
            return;
        }
        if (baseMsg.type == "ASSIGN")
            {
                AssignMessage assignMessage = JsonUtility.FromJson<AssignMessage>(msg);
                gameStarted = true;
                // El servidor envía el ID del jugador
                playerId = assignMessage.id;
                Debug.Log($"Eres el Jugador {playerId}");
                text.text = $"Eres el Jugador {playerId}";
            }
            else if (baseMsg.type == "START")
            {
                turnManager.StartGame();
                Debug.Log($"Server start");
                text.text = "";

            }
            else if (baseMsg.type == "TURN")
            {
                turnManager.EndTurn();
                // text.text = "";

            }
            else if (baseMsg.type == "DATA")
            {
                DataMessage dataMsg = JsonUtility.FromJson<DataMessage>(msg);
                enemyPosition = new Vector2(dataMsg.x, dataMsg.y);
                float myHealth = Mathf.RoundToInt(dataMsg.health);
                Debug.Log($"Posición del enemigo: {enemyPosition}, Salud del enemigo: {myHealth}");
                turnManager.StartTurn(enemyPosition, myHealth);
            }
        

    }

    //JSON

    public void SendPlayerInfoToServer(float x, float y, float health)
    {
        DataMessage dataMsg = new DataMessage(x, y, health);
        string msg = JsonUtility.ToJson(dataMsg);
        SendToServer(msg);
    }

    public void EndGame()
    {
        Debug.Log("Desconectando del servidor...");

        try
        {
            // Enviar un mensaje de desconexión al servidor (opcional)
            DisconnectMessage dataMsg = new DisconnectMessage();
            string msg = JsonUtility.ToJson(dataMsg);
            SendToServer(msg);

            // Cerrar stream y conexión
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }

            if (client != null)
            {
                client.Close();
                client = null;
            }

            Debug.Log("Conexión cerrada.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error al cerrar la conexión: " + e.Message);
        }
    }


    private void SendToServer(string msg)
    {
        if (stream != null && stream.CanWrite)
        {
            byte[] data = Encoding.ASCII.GetBytes(msg + "\n");
            stream.Write(data, 0, data.Length);
            Debug.Log($"Enviado a server: {msg}");
        }

    }

    private void OnApplicationQuit()
    {
        // Cerrar la conexión cuando la aplicación se cierre
        if (client != null && client.Connected)
        {
            client.Close();
        }
    }
}

[Serializable]
public class NetworkMessage
{
    public string type; // e.g. "DATA", "TURN", "START", ASSIGN.
}

[Serializable]
public class DataMessage : NetworkMessage
{
    public float x;
    public float y;
    public float health;

    public DataMessage(float x, float y, float health)
    {
        this.type = "DATA";
        this.x = x;
        this.y = y;
        this.health = health;
    }
}

[Serializable]
public class AssignMessage : NetworkMessage
{
    public int id;

    public AssignMessage(int id)
    {
        this.type = "ASSIGN";
        this.id = id;
    }
}

[Serializable]
public class DisconnectMessage : NetworkMessage
{
    public DisconnectMessage()
    {
        this.type = "DISCONNECT";

    }
}