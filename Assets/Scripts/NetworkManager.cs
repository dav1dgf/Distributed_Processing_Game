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
    private int enemyHealth;
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
                await client.ConnectAsync("127.0.0.1", 65432);
                stream = client.GetStream();
                Debug.Log("Conectado al servidor.");
                text.text = "Connected, waiting 4 player...";
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

            string msg = Encoding.ASCII.GetString(buffer, 0, length).Trim();
            HandleServerMessage(msg);
        }
    }

    private void HandleServerMessage(string msg)
    {
        Debug.Log("Servidor dijo: " + msg);


        if (msg == "0" || msg == "1")
        {
            gameStarted = true;
            // El servidor envía el ID del jugador
            playerId = int.Parse(msg);
            Debug.Log($"Eres el Jugador {playerId}");
            text.text = $"Eres el Jugador {playerId}";
        }
        else if (msg == "START")
        {
            turnManager.StartGame();
            text.text = "";
            
        }
        else if (msg == "TURN")
        {
            turnManager.EndTurn();
            text.text = "";

        }
        else if (msg.StartsWith("DATA"))
        {
            var parts = msg.Split(' ');
            if (parts.Length == 4)
            {
                if (float.TryParse(parts[1], out float x) &&
                    float.TryParse(parts[2], out float y) &&
                    float.TryParse(parts[3].Replace(",", "."), out float health))
                {
                    enemyPosition = new Vector2(x, y);
                    enemyHealth = Mathf.RoundToInt(health);
                    Debug.Log($"Posición del enemigo: {enemyPosition}, Salud del enemigo: {enemyHealth}");
                }
                else
                {
                    Debug.LogWarning("Error al parsear los datos del enemigo.");
                }
            }
        }
    }



        public void SendPlayerInfoToServer(float x, float y, float health)
    {
        string msg = $"DATA {x} {y} {health}";

        SendToServer(msg);
    }

    public void EndGame()
    {
        Debug.Log("Desconectando del servidor...");

        try
        {
            // Enviar un mensaje de desconexión al servidor (opcional)
            SendToServer("DISCONNECT");

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
