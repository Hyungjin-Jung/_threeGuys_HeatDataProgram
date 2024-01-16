import socket

# 원하는 색깔로 텍스트를 출력하는 함수
def colored_print(color, text):
    colors = {
        'reset': '\033[0m',
        'light_red': '\033[91m',
        'light_green': '\033[92m',
        'light_yellow': '\033[93m',
    }

    # 해당하는 색상이 없으면 색상 없이 출력
    if color not in colors:     
        print(text)
        return

    colored_text = f"{colors[color]}{text}{colors['reset']}"
    print(colored_text)

class SocketHandler:
    __server_socket = None      # 서버 소켓 정보를 담는 필드
    __client_sockets = dict()   # 모든 클라이언트 소켓 정보를 담는 딕셔너리
    __clinet_number = 0

    def __init__(self, port_num, max_clients, encode, decode, buffer_size):
        self.__encode = encode              # 인코딩 형식 지정
        self.__decode = decode              # 디코딩 형식 지정
        self.__max_clinets = max_clients    # 최대 클라이언트 수 지정
        self.__buffer_size = buffer_size    # 버퍼 사이즈 지정

        self.__server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.__server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.__server_socket.bind(("0.0.0.0", port_num))
        self.__server_socket.listen(max_clients)

    # 클라이언트와 연결을 시도하는 메소드
    def sockets_prepare(self):
        client_socket, client_address = self.__server_socket.accept()
        
        if self.__clinet_number < self.__max_clinets:
            self.__clinet_number += 1
            self.__client_sockets[client_socket] = client_address
            colored_print("light_green", f"{self.__client_sockets[client_socket][0]}:{self.__client_sockets[client_socket][1]} 클라이언트 연결!")
        
        else:
            client_socket.close()
            colored_print("light_red", f"{client_address[0]}:{client_address[1]} 최대 클라이언트 제한으로 클라이언트 연결 차단!")

    # 클라이언트와의 연결이 끊겼을 경우 클라이언트 목록에서 클라이언트를 삭제하는 메소드
    def __sockets_remove(self, clients_sockets_remove):
        for client_socket in clients_sockets_remove:
            self.__client_sockets.pop(client_socket, None)
            self.__clinet_number -= 1

    # 모든 클라이언트에게 문자를 수신하는 메소드
    def sockets_send(self, send_string):
        clients_sockets_remove = list()

        for client_socket in self.__client_sockets.keys():
            try:
                client_socket.send(send_string.encode(self.__encode))

            # 클라이언트와 연결이 끊겼을 경우
            except (ConnectionResetError, BrokenPipeError) as e:
                colored_print("light_yellow", f"{self.__client_sockets[client_socket][0]}:{self.__client_sockets[client_socket][1]} 클라이언트 연결 끊김!\n{e}")
                clients_sockets_remove.append(client_socket)

        # 클라이언트 목록에서 끊어진 클라이언트 삭제
        self.__sockets_remove(clients_sockets_remove)

    # 모든 클라이언트로부터 문자를 송신하는 메소드
    def sockets_received(self):
        clients_sockets_remove = list()
        received_data_dict = dict()

        for client_socket, client_address in self.__client_sockets.items():
            data = client_socket.recv(self.__buffer_size)
            if data:
                client_address = f"{client_address[0]}:{client_address[1]}"
                received_string = data.decode(self.__decode)
                received_data_dict[client_address] = received_string

                return received_data_dict

            # 클라이언트와 연결이 끊겼을 경우
            else:
                colored_print("light_yellow", f"{self.__client_sockets[client_socket][0]}:{self.__client_sockets[client_socket][1]} 클라이언트 연결 끊김!")
                clients_sockets_remove.append(client_socket)

        # 클라이언트 목록에서 끊어진 클라이언트 삭제
        self.__sockets_remove(clients_sockets_remove)