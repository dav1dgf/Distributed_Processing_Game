import socket
import threading

HOST = '127.0.0.1'
PORT = 65432

def receive_messages(sock):
    while True:
        try:
            data = sock.recv(1024).decode()
            if not data:
                print("Conexión cerrada por el servidor.")
                break
            print(f"[Servidor]: {data}")
        except:
            print("Error al recibir datos del servidor.")
            break

def main():
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.connect((HOST, PORT))
    
    # Recibir ID de jugador
    try:
        player_id = sock.recv(1024).decode().strip()
        print(f"[CLIENTE] Asignado como jugador {player_id}")
    except:
        print("No se pudo obtener el ID del jugador.")
        return

    # Hilo para recibir mensajes del servidor
    receiver_thread = threading.Thread(target=receive_messages, args=(sock,), daemon=True)
    receiver_thread.start()

    # Bucle principal: enviar mensajes manualmente
    try:
        while True:
            msg = input("> ")
            if msg.lower() == "quit":
                break
            sock.sendall(msg.encode())
    except KeyboardInterrupt:
        print("\n[CLIENTE] Finalizando conexión.")
    finally:
        sock.close()

if __name__ == '__main__':
    main()
