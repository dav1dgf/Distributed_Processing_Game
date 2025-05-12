import socket
import threading
import time
import random

players = {}
lock = threading.Lock()
turn_index = 0
turn_duration = 3  # segundos
MAX_PLAYERS = 2

# Información ficticia de robots (ejemplo)
player_data = {0: {"position": (0, 0), "health": 100}, 1: {"position": (10, 10), "health": 100}}

def handle_client(conn, addr, player_id):
    global turn_index
    conn.sendall(f"{player_id}\n".encode())  # Enviar el ID del jugador (0 o 1) al cliente
    other_player_id = (player_id + 1) % MAX_PLAYERS

    # Esperar hasta que haya 2 jugadores conectados
    while len(players) < MAX_PLAYERS:
        time.sleep(0.5)

    # Informar a los clientes cuando el juego comienza
    conn.sendall(f"{player_id}".encode())

    # Iniciar el ciclo de actualizaciones periódicas
    last_update_time = time.time()

    while True:
        try:
            # Recibir datos del cliente (se espera 2 floats y un int: x, y, vida)
            data = conn.recv(1024).decode().strip()
            if not data:
                break
            print(f"[Jugador {player_id}] → {data}")

            # Si es el primer mensaje, esperar los 2 floats y el int (x, y, vida)
            if data:
                try:
                    # Se esperan tres valores (x, y) flotantes y vida (entero)
                    parts = data.split()
                    if len(parts) == 3:
                        x, y, health = float(parts[0]), float(parts[1]), int(parts[2])
                        # Actualizar la posición y vida del jugador
                        player_data[player_id]["position"] = (x, y)
                        player_data[other_player_id]["health"] = health
                        
                        print(f"Jugador {player_id} actualizado: posición {x}, {y}, vida rival {health}")

                        # Ahora enviar la información del otro jugador
                        other_position = player_data[other_player_id]["position"]
                        other_health = player_data[player_id]["health"]
                        
                        # Enviar al jugador actual la información del otro jugador
                        conn.sendall(f"DATA {other_position[0]} {other_position[1]} {other_health}".encode())
                    else:
                        print(f"Datos incorrectos recibidos: {data}")
                except ValueError as ve:
                    print(f"Error al convertir datos: {ve}")
                    conn.sendall(f"ERROR: Datos inválidos. Se esperaban 2 floats y un int.\n".encode())
                    continue  # Ignorar este ciclo y esperar nuevos datos
          
            

        except Exception as e:
            print(f"Error: {e}")
            break

    conn.close()


def start_server():
    HOST = '127.0.0.1'
    PORT = 65432
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.bind((HOST, PORT))
    server.listen()

    print(f"[SERVIDOR] Esperando conexiones en {HOST}:{PORT}...")
    player_id = 0

    while player_id < MAX_PLAYERS:
        conn, addr = server.accept()
        print(f"[CONECTADO] Jugador {player_id} desde {addr}")
        players[player_id] = conn
        thread = threading.Thread(target=handle_client, args=(conn, addr, player_id))
        thread.start()
        player_id += 1

start_server()
