"""
TCP Server Script

This script implements a simple multithreaded TCP server.

If you use this code, please cite:

Author: Mahya Mirbagheri
GitHub Repository: https://github.com/Marry-20/UWP-2wayTCP
"""
import socket
import threading
import signal
from select import select
import sys

# Global flag to control server termination
terminate = False  

def signal_handler(signum, frame):           
    """Handles incoming signals (e.g., Ctrl+C) to gracefully terminate the server."""
    global terminate                         
    terminate = True 

def tcp_worker(sock, addr):
    """
    Handles communication with a connected client.
    
    Args:
        sock (socket.socket): The client's socket connection.
        addr (tuple): The client's IP address and port.
    """
    print(f'New client connection from {addr[0]}:{addr[1]}')

    while True:
        data = sock.recv(1024)
        if not data or data.decode('utf-8') == 'disconnect':
            break
        else:
            # Respond to client
            msg = 'Hello From Server'
            sock.send(msg.encode('utf-8'))

    sock.close()
    print(f'Client connection {addr[0]}:{addr[1]} closed')

def start_server(host='', port=8000):
    """
    Starts the TCP server.

    Args:
        host (str): The server's IP address (empty string binds to all available interfaces).
        port (int): The port on which the server listens.
    """
    # Setup signal handler to allow graceful termination
    signal.signal(signal.SIGINT, signal_handler)

    # Create and configure server socket
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    server_socket.bind((host, port))
    server_socket.listen(1)

    print('TCP Server Running on port', port)
    print('Waiting for clients...')

    try:
        while not terminate:
            ready, _, _ = select([server_socket], [], [], 1)
            if ready:
                client_cxn, addr = server_socket.accept()
                thread = threading.Thread(target=tcp_worker, args=(client_cxn, addr))
                thread.start()
    except Exception as e:
        print(f"An error occurred: {e}")
    finally:
        print('TCP Server Shutdown')
        server_socket.close()

if __name__ == "__main__":
    start_server()
