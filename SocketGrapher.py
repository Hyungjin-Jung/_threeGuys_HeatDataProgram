import time
import socket
import dash
from dash import html
from dash import dcc
from dash.dependencies import Input, Output
import pandas as pd
import plotly.subplots as sp
import plotly.graph_objs as go
from plotly.subplots import make_subplots
import threading

class CSSocketHandler:
    def __init__(self, server_ip, server_port):
        self.__server_ip = server_ip          # 서버 IP 지정
        self.__server_port = server_port      # 서버 포트 지정
        self.__client_socket = None
        self.__client_address = None

    def prepare_socket(self):
        server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server_socket.bind((self.__server_ip, self.__server_port))
        server_socket.listen(1)
        self.__client_socket, self.__client_address = server_socket.accept()

    def send_socket_string(self, send_string):
        self.__client_socket.send(send_string.encode('utf-8'))

    def received_socket_string(self):
        data = self.__client_socket.recv(1024)
        received_string = data.decode('utf-8')
        return received_string

    def check_received_string(self, check_string):
        strings = list(check_string.split(","))
        if strings[0] == "file_path":
            return "1", strings[1]
        elif strings[0] == "Temp":
            return "Temp", strings[1]
        return "Error: " + strings[0] + "이(가) 정의되지 않았습니다."


class FactoryGraphDrawer:
    def __init__(self, port_num, file_url, graph_num, line_num, second_y, graph_data_list, graph_name_list, line_color_list, yaxes_list, yaxes_name_list, graph_unit):
        self.__port_num = port_num                          # 포트 번호 지정      
        self.__df = pd.read_csv(file_url, encoding="utf8")    # csv 파일 불러오기
        self.__graph_num = graph_num                          # 그래프 갯수
        self.__line_num = line_num                            # 선 갯수
        self.__second_y = second_y                            # secondary_y 유무
        self.__graph_data_list = graph_data_list              # 그래프 데이터 지정
        self.__graph_name_list = graph_name_list              # 그래프 이름 지정
        self.__line_color_list = line_color_list              # 선 색깔 지정
        self.__yaxes_list = yaxes_list                        # 그래프 y축 지정
        self.__yaxes_name_list = yaxes_name_list              # 그래프 y축 이름 지정
        self.__graph_unit = graph_unit                        # 그래프 단위 지정
        self.__df["Time"] = pd.to_datetime(self.__df["Time"], format="%Y-%m-%d_%H:%M:%S")
        self.__app = dash.Dash(__name__)
        self.__app.layout = html.Div(
            [
                dcc.Graph(
                    id="live-update-graph",
                    config={'displayModeBar': False}
                ),
                dcc.Interval(
                    id="interval-component", interval=1 * 1000, n_intervals=0  # in milliseconds
                ),
            ],  
        )

        self.__app.callback(
            Output("live-update-graph", "figure"), [Input("interval-component", "n_intervals")]
        )(self.__update_graph_live)

    def __update_graph_live(self, n):
        # n을 이용하여 현재까지의 인덱스 범위를 지정
        start_index = 0 if n == 0 else n
        end_index = start_index + 20  # 20개씩 표시하고자 함

        data = self.__df[start_index:end_index:1]

        # subplots 생성
        fig = make_subplots(rows=3, cols=1, vertical_spacing=0.2,
                            specs=[[{"secondary_y": True}], [{"secondary_y": True}], [{"secondary_y": True}]])

        # 각 trace에 대한 정보를 전달하여 trace를 그리는 함수 호출
        for i in range (self.__graph_num):
            for j in range (self.__line_num[i]):
                if self.__second_y and j == self.__yaxes_list[i][0]:
                    self.__add_trace_to_figure(fig, data["Time"], data[self.__graph_data_list[i][j]], self.__graph_name_list[i][j], row=1 + i, col=1, secondary_y=False, line_color=self.__line_color_list[i][j])
                else:
                    self.__add_trace_to_figure(fig, data["Time"], data[self.__graph_data_list[i][j]], self.__graph_name_list[i][j], row=1 + i, col=1, secondary_y=True, line_color=self.__line_color_list[i][j])

        # 레이아웃 설정
        fig.update_layout(height=1150, margin={"l": 5, "r": 5, "b": 5, "t": 10}, showlegend=False)

        fig.update_xaxes(title_text="[시간]")
        for i in range(self.__graph_num):
            fig.update_yaxes(title_text=self.__yaxes_name_list[i][0] + " " + self.__graph_unit[i], row=i + 1, secondary_y=False)
            fig.update_yaxes(title_text=self.__yaxes_name_list[i][1] + " " + self.__graph_unit[i], row=i + 1, secondary_y=True)

        return fig

    def __add_trace_to_figure(self, fig, x, y, name, row, col, secondary_y, line_color):
        trace = go.Scatter(
            x=x,
            y=y,
            name=name,
            mode="lines+markers",
            type="scatter",
            line=dict(color=line_color),
        )
        fig.add_trace(trace, row=row, col=col, secondary_y=secondary_y)

    def run_app(self):
        self.__app.run_server(debug=False, use_reloader=False, port = self.__port_num)
    
if __name__ == "__main__":
    is_connected = False
    file_url = "https://drive.google.com/uc?id=1ct6IrhLEwsJbNlFQpgSFGVQvx-Ff5kma&export=download"
    csh = CSSocketHandler("127.0.0.1", 56789)
    exit_signal = False
    
    def prepare_thread():
        global is_connected  
        print(f"\n클라이언트와의 연결 시도 중...")
        csh.prepare_socket()
        is_connected = True
        print(f"\n클라이언트 연결 성공!")
        restart_dash_thread()
        while True:
            try:
                received_string = csh.received_socket_string()
                if received_string:
                    print(f"\n수신: {received_string}")
            except ConnectionResetError:
                is_connected = False
                print(f"\n클라이언트와의 연결이 끊겼습니다. 다시 연결 시도 중...")
                csh.prepare_socket()
                is_connected = True
                print(f"다시 연결되었습니다.")
                restart_dash_thread()  # 연결 재시도 후 Dash 애플리케이션 다시 시작
    
    def restart_dash_thread():
        global area1_graph_thread, area2_graph_thread, area3_graph_thread, area4_graph_thread
        if area1_graph_thread.is_alive():
            area1_graph_thread.join()
        if area2_graph_thread.is_alive():
            area2_graph_thread.join()
        if area3_graph_thread.is_alive():
            area3_graph_thread.join()
        if area4_graph_thread.is_alive():
            area4_graph_thread.join()
        
        area1_graph_thread = threading.Thread(target=area1_graph.run_app)
        area2_graph_thread = threading.Thread(target=area2_graph.run_app)
        area3_graph_thread = threading.Thread(target=area3_graph.run_app)
        area4_graph_thread = threading.Thread(target=area4_graph.run_app)
        
        area1_graph_thread.start()
        area2_graph_thread.start()
        area3_graph_thread.start()
        area4_graph_thread.start()
    
    port_num = 56790
    graph_num = 3
    line_num = [2, 5, 3]
    second_y = [True, True, True]
    graph_data_list = [["GN07N_MAIN_POWER", "GN07N_SUB_POWER"],
                       ["GN07N_TEMP", "GN07N_HIGH_TEMP", "GN07N_MID_TEMP", "GN07N_LOW_TEMP", "GN07N_OVER_TEMP"],
                       ["GN07N_GAS_NRG", "GN07N_GAS_AMM", "GN07N_GAS_CDO"]]
    graph_name_list = [["메인전력", "보조전력"],
                       ["온도", "상층온도", "중층온도", "하층온도", "과열온도"],
                        ["질소가스", "암모니아가스", "Co2가스"]]
    line_color_list = [["darkorange", "darkgoldenrod"],
                        ["red", "darkred", "darkorange", "orangered", "firebrick"],
                        ["forestgreen", "limegreen", "darkgreen"]]
    yaxes_list = [[0, 1],
                  [0, 1],
                  [1, 0]]
    yaxes_name_list = [["메인전력", "보조전력"],
                       ["온도", "기타온도"],
                       ["암모니아가스", "질소, Co2가스"]]
    graph_unit = ["[w]", "[°C]", "[㎥]"]
    
    area1_graph = FactoryGraphDrawer(port_num, file_url, graph_num, line_num, second_y, graph_data_list, 
                               graph_name_list, line_color_list, yaxes_list, yaxes_name_list, graph_unit)

    port_num = 56791
    graph_num = 3
    line_num = [2, 4, 3]
    second_y = [True, True, True]
    graph_data_list = [["GN05N_MAIN_POWER", "GN05M_MAIN_POWER"],
                       ["GN05M_TEMP", "GN05M_HIGH_TEMP", "GN05M_LOW_TEMP", "GN05M_OVER_TEMP"],
                       ["GN05M_GAS_NRG", "GN05M_GAS_AMM", "GN05M_GAS_CDO"]]
    graph_name_list = [["메인전력", "메인전력"],
                       ["온도", "상층온도", "하층온도", "과열온도"],
                        ["질소가스", "암모니아가스", "Co2가스"]]
    line_color_list = [["darkorange", "darkgoldenrod"],
                        ["red", "darkred", "orangered", "firebrick"],
                        ["forestgreen", "limegreen", "darkgreen"]]
    yaxes_list = [[0, 1],
                  [0, 1],
                  [1, 0]]
    yaxes_name_list = [["메인전력", "메인전력"],
                       ["온도", "기타온도"],
                       ["암모니아가스", "질소, Co2가스"]]
    graph_unit = ["[w]", "[°C]", "[㎥]"]
    
    area2_graph  = FactoryGraphDrawer(port_num, file_url, graph_num, line_num, second_y, graph_data_list, 
                               graph_name_list, line_color_list, yaxes_list, yaxes_name_list, graph_unit)

    port_num = 56792
    graph_num = 3
    line_num = [2, 5, 3]
    second_y = [True, True, True]
    graph_data_list = [["GN04N_MAIN_POWER", "GN04M_MAIN_POWER"],
                       ["GN04M_TEMP", "GN04M_HIGH_TEMP", "GN04M_MID_TEMP", "GN04M_LOW_TEMP", "GN04M_OVER_TEMP"],
                       ["GN04M_GAS_NRG", "GN04M_GAS_AMM", "GN04M_GAS_CDO"]]
    graph_name_list = [["메인전력", "메인전력"],
                       ["온도", "상층온도", "중층온도", "하층온도", "과열온도"],
                        ["질소가스", "암모니아가스", "Co2가스"]]
    line_color_list = [["darkorange", "darkgoldenrod"],
                        ["red", "darkred", "darkorange", "orangered", "firebrick"],
                        ["forestgreen", "limegreen", "darkgreen"]]
    yaxes_list = [[0, 1],
                  [0, 1],
                  [1, 0]]
    yaxes_name_list = [["메인전력", "메인전력"],
                       ["온도", "기타온도"],
                       ["암모니아가스", "질소, Co2가스"]]
    graph_unit = ["[w]", "[°C]", "[㎥]"]
    
    area3_graph  = FactoryGraphDrawer(port_num, file_url, graph_num, line_num, second_y, graph_data_list, 
                               graph_name_list, line_color_list, yaxes_list, yaxes_name_list, graph_unit)
    
    port_num = 56793
    graph_num = 3
    line_num = [2, 5, 3]
    second_y = [True, True, True]
    graph_data_list = [["GN03N_MAIN_POWER", "GN02N_MAIN_POWER"],
                       ["GN02N_TEMP", "GN02N_HIGH_TEMP", "GN02N_MID_TEMP", "GN02N_LOW_TEMP", "GN02N_OVER_TEMP"],
                       ["GN02N_GAS_NRG", "GN02N_GAS_AMM", "GN02N_GAS_CDO"]]
    graph_name_list = [["메인전력", "메인전력"],
                       ["온도", "상층온도", "중층온도", "하층온도", "과열온도"],
                        ["질소가스", "암모니아가스", "Co2가스"]]
    line_color_list = [["darkorange", "darkgoldenrod"],
                        ["red", "darkred", "darkorange", "orangered", "firebrick"],
                        ["forestgreen", "limegreen", "darkgreen"]]
    yaxes_list = [[0, 1],
                  [0, 1],
                  [1, 0]]
    yaxes_name_list = [["메인전력", "메인전력"],
                       ["온도", "기타온도"],
                       ["암모니아가스", "질소, Co2가스"]]
    graph_unit = ["[w]", "[°C]", "[㎥]"]
    
    area4_graph = FactoryGraphDrawer(port_num, file_url, graph_num, line_num, second_y, graph_data_list, 
                               graph_name_list, line_color_list, yaxes_list, yaxes_name_list, graph_unit)
    
    prepare_thread1 = threading.Thread(target=prepare_thread)
    area1_graph_thread = threading.Thread(target=area1_graph.run_app)
    area2_graph_thread = threading.Thread(target=area2_graph.run_app)
    area3_graph_thread = threading.Thread(target=area3_graph.run_app)
    area4_graph_thread = threading.Thread(target=area4_graph.run_app)
    
    prepare_thread1.start()
    area1_graph_thread.start()
    area2_graph_thread.start()
    area3_graph_thread.start()
    area4_graph_thread.start()

    try:
        prepare_thread1.join()
        area1_graph_thread.join()
        area2_graph_thread.join()
        area3_graph_thread.join()
        area4_graph_thread.join()
        
    except KeyboardInterrupt:
        print(f"서버가 종료되었습니다.")
        is_connected = False