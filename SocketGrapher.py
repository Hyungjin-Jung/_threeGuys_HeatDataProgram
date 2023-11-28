import threading
import time
import socket

class CSSocketHandler:
    def __init__(self):
        self.server_ip = '127.0.0.1'        # 서버 주소 지정
        self.server_port = 56789            # 포트 번호 지정
        self.client_socket = None
        self.client_address = None

    def prepare_socket(self):
        server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server_socket.bind((self.server_ip, self.server_port))
        server_socket.listen(1)

        self.client_socket, self.client_address = server_socket.accept()

    def send_socket_string(self, send_string):
        self.client_socket.send(send_string.encode('utf-8'))

    def received_socket_string(self):
        data = self.client_socket.recv(1024)
        received_string = data.decode('utf-8')
        return received_string

    def check_received_string(self, check_string):
        strings = list(check_string.split(" "))
        if strings[0] == "1":
            return "1"
        elif strings[0] == "2":
            return "2"
        return "Error: " + strings[0] + "이(가) 정의되지 않았습니다."

if __name__ == "__main__":
    csh = CSSocketHandler()
    print("\n클라이언트와의 연결 시도 중...")
    csh.prepare_socket()
    print("\n클라이언트 연결 성공!")
    is_connected = True

    def send_thread():
        while True:
            try:
                if is_connected:
                    send_string = input()
                    csh.send_socket_string(send_string)
            except ConnectionResetError:
                print("\n클라이언트와의 연결이 끊겼습니다. 다시 연결 시도 중...")

    def receive_thread():
        while True:
            try:
                received_string = csh.received_socket_string()
                if received_string:
                    print(f"\n수신: {received_string}")
            except ConnectionResetError:
                is_connected = False
                print("\n클라이언트와의 연결이 끊겼습니다. 다시 연결 시도 중...")
                csh.prepare_socket()
                is_connected = True
                print("다시 연결되었습니다.")


    send_thread = threading.Thread(target=send_thread)
    receive_thread = threading.Thread(target=receive_thread)

    send_thread.start()
    receive_thread.start()

    try:
        send_thread.join()
        receive_thread.join()
    except KeyboardInterrupt:
        print(f"서버가 종료되었습니다.")
        is_connected = False  # 사용자가 프로그램을 종료하면 연결 종료