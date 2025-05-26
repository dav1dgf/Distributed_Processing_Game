import socket
import threading
import time
import signal
import sys
import json 

players = {}
lock = threading.Lock()
turn_index = 0
turn_duration = 3  # segundos
MAX_PLAYERS = 2
start_barrier = threading.Barrier(2)  # Wait for 2 threads (players)
player_data = {0: {"position": (0, 0), "health": 100}, 1: {"position": (10, 10), "health": 100}}

server_socket = None  # Para cerrarlo desde el manejador de señal

def handle_client(conn, addr, player_id):
    global turn_index
    other_player_id = (player_id + 1) % MAX_PLAYERS

    while len(players) < MAX_PLAYERS:
        time.sleep(0.5)

    try:
        conn.sendall(json.dumps({"type": "ASSIGN", "id": player_id}).encode() + b"\n")
        print("Sending ID...")
        start_barrier.wait()
        time.sleep(3)
        print("Sending START...")
        conn.sendall(json.dumps({"type": "START"}).encode() + b"\n")


        while True:
            data = conn.recv(1024).decode().strip()
            if not data:
                break
            print(f"[Jugador {player_id}] → {data}")
            start_barrier.wait()
            try:
                msg_json = json.loads(data)
                if msg_json.get("type") == "DATA":
                    #If fails then replace here everything
                    x = float(msg_json.get("x", 0))
                    y = float(msg_json.get("y", 0))
                    health = float(msg_json.get("health", 0))

                    player_data[player_id]["position"] = (x, y)
                    player_data[other_player_id]["health"] = health

                    print(f"Jugador {player_id} actualizado: posición {x}, {y}, vida rival {health}")

                    other_conn = players.get(other_player_id)
                    if other_conn:
                        try:
                            rival_position = player_data[player_id]["position"]
                            own_health = player_data[other_player_id]["health"]
                            data_to_send = {
                                "type": "DATA",
                                "x": rival_position[0],
                                "y": rival_position[1],
                                "health": own_health
                            }
                            other_conn.sendall(json.dumps(data_to_send).encode() + b"\n")
                        except Exception as e:
                            print(f"Error enviando al jugador {other_player_id}: {e}")
                    start_barrier.wait()
                    #time.sleep(4)
                    print("Sending TURN...")
                    conn.sendall(json.dumps({"type": "TURN"}).encode() + b"\n")
                elif msg_json.get("type") == "DISCONNECT":
                    print(f"[Jugador {player_id}] solicitó desconexión.")
                    with lock:
                        players.pop(player_id, None)
                    break
                else:
                    print(f"Datos incorrectos recibidos: {data}")
            except json.JSONDecodeError:
                print(f"Datos incorrectos recibidos: {data}")
    except Exception as e:
        print(f"Error con el jugador {player_id}: {e}")
    finally:
        conn.close()

def shutdown_server(signum=None, frame=None):
    global server_socket
    print("\n[SERVIDOR] Apagando servidor...")
    if server_socket:
        server_socket.close()
    with lock:
        for pid, conn in players.items():
            try:
                conn.sendall("DISCONNECT\n".encode())
                conn.close()
            except:
                pass
        players.clear()
    sys.exit(0)

def start_server():
    global server_socket
    HOST = '172.20.10.2'
    PORT = 65432
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((HOST, PORT))
    server_socket.listen()
    print(f"[SERVIDOR] Esperando conexiones en {HOST}:{PORT}...")

    signal.signal(signal.SIGINT, shutdown_server)

    player_id = 0
    while player_id < MAX_PLAYERS:
        try:
            conn, addr = server_socket.accept()
            print(f"[CONECTADO] Jugador {player_id} desde {addr}")
            players[player_id] = conn
            thread = threading.Thread(target=handle_client, args=(conn, addr, player_id))
            thread.start()
            player_id += 1
        except OSError:
            # El socket fue cerrado manualmente (por Ctrl+C)
            break

if __name__ == '__main__':
    start_server()
