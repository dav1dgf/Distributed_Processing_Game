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
    private int playerId;
    private Vector2 playerPosition;
    private int playerHealth;
    private Vector2 enemyPosition;
    private int enemyHealth;
    public TMP_Text text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async void Start()
    {
        ConnectToServer();
        await ListenForServerMessages();
    }

    void ConnectToServer()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 65432);  // Asegúrate de que el puerto coincida con el servidor
            stream = client.GetStream();
            Debug.Log("Conectado al servidor.");
        }
        catch (SocketException e)
        {
            Debug.LogError("No se pudo conectar al servidor: " + e.Message);
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
        else if (msg.StartsWith("DATA"))
        {
            // El servidor envía la información de la posición y la vida del otro jugador
            var parts = msg.Split(' ');
            if (parts.Length == 4)
            {
                enemyPosition = new Vector2(float.Parse(parts[1]), float.Parse(parts[2]));
                enemyHealth = int.Parse(parts[3]);
                Debug.Log($"Posición del enemigo: {enemyPosition}, Salud del enemigo: {enemyHealth}");
            }
        }
        
        // Aquí podrías agregar más comandos para otras acciones como "MOVE", "JUMP", etc.
    }

    private void Update()
    {
        // Si el juego ha comenzado y estamos en nuestro turno, procesamos las entradas
        if (gameStarted)
        {
            if (playerId == 0) // Si eres el jugador 0
            {
                // Movimiento simple con teclas para pruebas
                float moveX = Input.GetAxis("Horizontal");
                float moveY = Input.GetAxis("Vertical");

                // Enviar la posición y la vida al servidor cada vez que se mueva
                SendPlayerInfoToServer(playerPosition.x, playerPosition.y, playerHealth);
            }
        }
    }

    private void SendPlayerInfoToServer(float x, float y, int health)
    {
        string msg = $"{x} {y} {health}";
        SendToServer(msg);
    }

    private void SendToServer(string msg)
    {
        if (stream != null && stream.CanWrite)
        {
            byte[] data = Encoding.ASCII.GetBytes(msg + "\n");
            stream.Write(data, 0, data.Length);
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
