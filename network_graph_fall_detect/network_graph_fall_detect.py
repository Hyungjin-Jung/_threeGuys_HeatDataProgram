import time
import threading
import pandas as pd

import socket_handler
import realtime_graph_drawer
import opencv_fall_detection

if __name__ == "__main__":
    def socket_prepare():
        print(f"\033[92m소켓 서버 열림!\033[0m")
        while True:
            try:
                socket_instace.sockets_prepare()
                time.sleep(0.01)
            except Exception as e:
                print(f"\033[91m{e}\033[0m")

    def socket_send():
        while True:
            try:
                opencv_result = opencv_instace.detect_pose()
                print(opencv_result)
                send_string = opencv_result
                socket_instace.sockets_send(send_string)
                time.sleep(5)
            except Exception as e:
                print(f"\033[91m{e}\033[0m")

    def socket_receive():
        while True:
            try:
                received_data_dict = socket_instace.sockets_received()
                if received_data_dict:
                    for received_data in received_data_dict.keys():
                        print(f'\033[92m{received_data} 수신: {received_data_dict[received_data]}\033[0m')
                time.sleep(0.01)
            except Exception as e:
                print(f"\033[91m{e}\033[0m")

    port_num = 56792
    max_clients = 1
    encode = 'utf-8'
    decode = 'utf-8'
    buffer_size = 1024
    socket_instace = socket_handler.SocketHandler(port_num, max_clients, encode, decode, buffer_size)

    dataset_url = "https://drive.google.com/uc?id=1ct6IrhLEwsJbNlFQpgSFGVQvx-Ff5kma&export=download"
    print(f"\033[92m데이터셋 읽는 중...\033[0m")
    df = pd.read_csv(dataset_url, encoding="utf8")
    print(f"\033[92m데이터셋 읽기 성공!\033[0m")
    df["Time"] = pd.to_datetime(df["Time"], format="%Y-%m-%d_%H:%M:%S")     # 데이터 프레임의 시간을 시간형으로 변경

    graph_instance = realtime_graph_drawer.RealtimeGraphDrawer()
    graph_instance.dataframe_set(df)

    opencv_instace = opencv_fall_detection.PoseDetection()

    socket_prepare_thread = threading.Thread(target=socket_prepare)
    socket_send_thread = threading.Thread(target=socket_send)
    update_time_thread = threading.Thread(target=graph_instance.update_time, args=(2000, ))
    graph_thread = threading.Thread(target=graph_instance.run_app)
    
    socket_prepare_thread.start()
    socket_send_thread.start()
    update_time_thread.start()
    graph_thread.start()

    socket_prepare_thread.join()
    socket_send_thread.join
    update_time_thread.join()
    graph_thread.join()
    