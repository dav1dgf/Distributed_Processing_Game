import socket
import threading
import time
import signal
import sys
import json 
import os

# Stores connected players' sockets indexed by their player ID
players = {}

# Lock to ensure thread-safe access to shared resources
lock = threading.Lock()

# Keeps track of whose turn it is (0 or 1)
turn_index = 0

# Time duration for each turn (in seconds)
turn_duration = 3  

# Maximum number of players allowed
MAX_PLAYERS = 2

# Synchronization barrier to make both players wait until each is ready
start_barrier = threading.Barrier(2)

# Initial player data: position (x, y) and health
# Just initialise
player_data = {
    0: {"position": (0, 0), "health": 100},
    1: {"position": (0, 0), "health": 100}
}

# Reference to the server socket (needed for graceful shutdown)
server_socket = None

# Handles communication with a single client
def handle_client(conn, addr, player_id):
    global turn_index
    other_player_id = (player_id + 1) % MAX_PLAYERS  # ID of the opponent

    # Wait until both players are connected
    while len(players) < MAX_PLAYERS:
        time.sleep(0.5)

    try:
        # Send assigned player ID
        conn.sendall(json.dumps({"type": "ASSIGN", "id": player_id}).encode() + b"\n")
        print("Sending ID...")

        # Wait for both players to be ready
        start_barrier.wait()
        time.sleep(3)

        # Notify client that the game is starting
        print("Sending START...")
        conn.sendall(json.dumps({"type": "START"}).encode() + b"\n")

        # Main loop to receive and process data from this client
        while True:
            data = conn.recv(1024).decode().strip()
            if not data:
                break  # Client disconnected

            print(f"[Player {player_id}] → {data}")
            start_barrier.wait()  # Sync before processing

            try:
                msg_json = json.loads(data)

                # Handle player data update
                if msg_json.get("type") == "DATA":
                    # Get updated position and health from client
                    x = float(msg_json.get("x", 0))
                    y = float(msg_json.get("y", 0))
                    
                    health = float(msg_json.get("health", 0))
                    #Check for comply in position and health, if x < -20, x>20 | y< -3 y > 3| health <0 discard msg
                    if x < -20 or x > 20 or y < -3 or y > 3 or health < 0:
                        print(f"Received invalid data from player {player_id}: position=({x},{y}), health={health}. Discarding message.")
                        continue  # This discards the data by skipping the rest of the loop iteration
                    # Update this player's position and the opponent's health
                    player_data[player_id]["position"] = (x, y)
                    player_data[other_player_id]["health"] = health

                    print(f"Player {player_id} updated: position {x}, {y}, enemy health {health}")

                    # Send the updated info to the other player
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
                            print(f"Error sending to player {other_player_id}: {e}")

                    # Sync both players again and send TURN signal
                    start_barrier.wait()
                    time.sleep(5)
                    print("Sending TURN...")
                    conn.sendall(json.dumps({"type": "TURN"}).encode() + b"\n")

                # Handle player winner request
                elif msg_json.get("type") == "WINNER":
                    winner = msg_json.get("winner")
                    print(f"[Player {player_id}] reported WINNER: {winner}")

                    # Notify the other player who won
                    other_conn = players.get(other_player_id)
                    if other_conn:
                        try:
                            winner_msg = {
                                "type": "WINNER",
                                "winner": winner
                            }
                            other_conn.sendall(json.dumps(winner_msg).encode() + b"\n")
                        except Exception as e:
                            print(f"Failed to notify player {other_player_id} of WINNER: {e}")
                    with lock:
                        players.pop(player_id, None)
                    shutdown_server()
                    break

                else:
                    print(f"Invalid data received: {data}")
            except json.JSONDecodeError:
                print(f"Invalid JSON received: {data}")

    except Exception as e:
        print(f"Error with player {player_id}: {e}")
        shutdown_server()
    finally:
        conn.close()

def handle_sigint(signum, frame):
    print("\n[SERVER] SIGINT received (Ctrl+C), closing server...\n")
    shutdown_server()

# Gracefully shut down the server and notify clients
def shutdown_server(signum=None, frame=None):
    global server_socket
    print("\n[SERVER] Shutting down...")
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
    os._exit(0)

# Start the game server
def start_server():
    global server_socket
    HOST = '172.20.10.2'  # Change to your local IP or 0.0.0.0 for all interfaces
    PORT = 65432
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((HOST, PORT))
    server_socket.listen()
    print(f"[SERVER] Waiting for connections at {HOST}:{PORT}...")

    # Handle Ctrl+C to cleanly stop the server
    signal.signal(signal.SIGINT, handle_sigint)

    player_id = 0
    while player_id < MAX_PLAYERS:
        try:
            conn, addr = server_socket.accept()
            print(f"[CONNECTED] Player {player_id} from {addr}")
            players[player_id] = conn
            thread = threading.Thread(target=handle_client, args=(conn, addr, player_id))
            thread.start()
            player_id += 1
        except OSError:
            # Socket was closed manually (e.g. via Ctrl+C)
            break

# Entry point
if __name__ == '__main__':
    start_server()
