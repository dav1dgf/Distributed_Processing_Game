import socket
import threading
import time
import random

players = {}
lock = threading.Lock()
turn_index = 0
turn_duration = 3  # segundos
MAX_PLAYERS = 2
start_barrier = threading.Barrier(2)  # Wait for 2 threads (players)
# Información ficticia de robots (ejemplo)
player_data = {0: {"position": (0, 0), "health": 100}, 1: {"position": (10, 10), "health": 100}}

def handle_client(conn, addr, player_id):
    global turn_index
    #conn.sendall(f"{player_id}\n".encode())
    other_player_id = (player_id + 1) % MAX_PLAYERS

    # Esperar hasta que haya 2 jugadores conectados
    while len(players) < MAX_PLAYERS:
        time.sleep(0.5)

    conn.sendall(f"{player_id}".encode())
    start_barrier.wait()
    time.sleep(1)
    print("Sending START...")
    conn.sendall("START\n".encode())

    while True:
        try:
            data = conn.recv(1024).decode().strip()
            if not data:
                break
            print(f"[Jugador {player_id}] → {data}")
            start_barrier.wait()
            parts = data.split()
            if len(parts) == 4 and parts[0] == "DATA":
                #Lock not really needed
                
                x = float(parts[1].replace(',', '.'))
                y = float(parts[2].replace(',', '.'))
                health = float(parts[3])

                player_data[player_id]["position"] = (x, y)
                player_data[other_player_id]["health"] = health

                print(f"Jugador {player_id} actualizado: posición {x}, {y}, vida rival {health}")

                other_conn = players.get(other_player_id)
                if other_conn:
                    try:
                        rival_position = player_data[player_id]["position"]
                        own_health = player_data[other_player_id]["health"]
                        other_conn.sendall(f"DATA {rival_position[0]} {rival_position[1]} {own_health}\n".encode())
                        
                        
                    except Exception as e:
                        print(f"Error enviando al jugador {other_player_id}: {e}")
                start_barrier.wait()#wait till both sent message
                time.sleep(4)
                print("Sending TURN...")
                conn.sendall("TURN\n".encode())
            elif data == "DISCONNECT":
                print(f"[Jugador {player_id}] solicitó desconexión.")
                with lock:
                    players.pop(player_id, None)
                break

            else:
                print(f"Datos incorrectos recibidos: {data}")
        except Exception as e:
            print(f"Error con el jugador {player_id}: {e}")
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
