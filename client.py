
import socket

client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
client.connect(('127.0.0.1', 65432))

while True:
    msg = client.recv(1024).decode()
    print("[SERVIDOR]:", msg)
    if "your_turn:yes" in msg or "YOURTURN" in msg:
        cmd = input("Tu comando (MOVE LEFT / ATTACK 1 / etc): ")
        client.sendall(f"{cmd}\n".encode())
