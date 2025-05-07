import socket
import threading

PORT = 12345
MAX_CLIENTS = 2
client_sockets = []
lock = threading.Lock()

def handle_client(sock, client_id):
    while True:
        try:
            data = sock.recv(1024)
            if not data:
                break

            with lock:
                # Send the message to the other client
                for i, other_sock in enumerate(client_sockets):
                    if other_sock != sock:
                        try:
                            other_sock.sendall(data)
                        except:
                            pass
        except:
            break

    print(f"Client {client_id} disconnected.")
    sock.close()

def main():
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind(('', PORT))
    server_socket.listen(MAX_CLIENTS)
    print(f"Server listening on port {PORT}")

    while len(client_sockets) < MAX_CLIENTS:
        client_sock, addr = server_socket.accept()
        with lock:
            client_sockets.append(client_sock)
        print(f"Player {len(client_sockets)} connected from {addr}")

    # Send START to all clients
    for sock in client_sockets:
        sock.sendall(b'START')

    # Create threads for each client
    for i, sock in enumerate(client_sockets):
        thread = threading.Thread(target=handle_client, args=(sock, i+1), daemon=True)
        thread.start()

    try:
        while True:
            pass  # Keep the server running
    except KeyboardInterrupt:
        print("Server shutting down.")
        for sock in client_sockets:
            sock.close()
        server_socket.close()

if __name__ == '__main__':
    main()
