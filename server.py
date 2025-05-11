
import socket
import threading
import time

players = {}
lock = threading.Lock()
turn_index = 0
turn_duration = 3  # segundos
MAX_PLAYERS = 2

def handle_client(conn, addr, player_id):
    global turn_index
    conn.sendall(f"WELCOME {player_id}\n".encode())
    while len(players) < MAX_PLAYERS:
        time.sleep(0.5)

    conn.sendall(f"START map1 your_turn:{{'yes' if player_id == turn_index else 'no'}}\n".encode())

    while True:
        try:
            data = conn.recv(1024).decode().strip()
            if not data:
                break
            print(f"[Jugador {player_id}] → {data}")

            if player_id == turn_index:
                if data.startswith("MOVE") or data.startswith("JUMP") or data.startswith("CLIMB"):
                    response = f"UPDATE move registered: {data}\n"
                    conn.sendall(response.encode())
                elif data.startswith("ATTACK"):
                    response = f"HIT {{(player_id+1)%2}} vida_actual:90\n"
                    conn.sendall(response.encode())
                    with lock:
                        turn_index = (turn_index + 1) % MAX_PLAYERS
                    conn.sendall("ENDTURN\n".encode())
                    time.sleep(0.5)
                    players[turn_index].sendall("YOURTURN 3\n".encode())
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
