import socket
import threading
import sys

HOST = '127.0.0.1'
PORT = 65432
running = True  # Bandera para detener el hilo

# Lista de movimientos predefinidos (x, y, health)
movements = [
    (-2.0, -1.0, 100),
    (-1.5, -1.84, 90),
    (-1.3, -1.84, 80),
    (-1.3, -1, 70),
    (-1.6, -1.84, 60),
    (-1.5, -0.5, 50),
    (-1.4, -1.84, 40),
    (-1.3, -0.7, 30),
    (-1.2, -1.84, 20),
    (-1.1, -0.9, 10)
]

movement_index = 0  # índice del movimiento actual

def handle_server_messages(sock):
    global running, movement_index

    try:
        while running:
            data = sock.recv(1024).decode().strip()
            if not data:
                print("[CLIENTE FAKE] Conexión cerrada por el servidor.")
                break
            print(f"[Servidor]: {data}")

            if data.startswith("START") or data.startswith("TURN"):
                if movement_index < len(movements):
                    x, y, health = movements[movement_index]
                    mensaje = f"DATA {x:.1f} {y:.1f} {health}\n"
                    print(f"[CLIENTE FAKE] → Enviando: {mensaje.strip()}")
                    sock.sendall(mensaje.encode())
                    movement_index += 1
                else:
                    print("[CLIENTE FAKE] Movimientos agotados. Enviando DISCONNECT.")
                    sock.sendall("DISCONNECT\n".encode())
                    running = False
                    sock.close()
                    break

    except Exception as e:
        if running:
            print(f"[CLIENTE FAKE] Error: {e}")

def main():
    global running
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.connect((HOST, PORT))

    try:
        player_id = sock.recv(1024).decode().strip()
        print(f"[CLIENTE FAKE] Asignado como jugador {player_id}")
    except:
        print("No se pudo obtener el ID del jugador.")
        return

    thread = threading.Thread(target=handle_server_messages, args=(sock,))
    thread.start()

    try:
        while thread.is_alive():
            thread.join(1)
    except KeyboardInterrupt:
        print("\n[CLIENTE FAKE] Ctrl+C detectado. Cerrando conexión...")
        running = False
        try:
            sock.sendall("DISCONNECT\n".encode())
        except:
            pass
        sock.close()
        sys.exit(0)

if __name__ == '__main__':
    main()
