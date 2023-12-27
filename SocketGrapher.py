import time
import datetime
import socket
import dash
from dash import html
from dash import dcc
from dash.dependencies import Input, Output, State
import pandas as pd
import plotly.subplots as sp
import plotly.graph_objs as go
from plotly.subplots import make_subplots
import threading
import logging

# 불필요한 Werkzeug 로그 메시지 출력을 비활성화
logging.getLogger('werkzeug').setLevel(logging.ERROR)

# 현재 시간을 저장할 전역 변수 선언
now_time = None

# 클라이언트 소켓을 다루는 클래스
class CSSocketHandler:
    def __init__(self, server_ip, server_port):
        self.__server_ip = server_ip        # 서버 IP 지정
        self.__server_port = server_port    # 서버 포트 지정
        self.__client_socket = None
        self.__client_address = None

    # 서버 소켓을 설정하는 연결 대기하는 메소드
    def prepare_socket(self):
        server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server_socket.bind((self.__server_ip, self.__server_port))
        server_socket.listen(1)
        self.__client_socket, self.__client_address = server_socket.accept()

    # 문자열을 클라이언트 소켓으로 전송하는 메소드
    def send_socket_string(self, send_string):
        self.__client_socket.send(send_string.encode('utf-8'))

    # 클라이언트로부터 문자열을 수신하는 메소드
    def received_socket_string(self):
        data = self.__client_socket.recv(1024)
        received_string = data.decode('utf-8')
        return received_string

    # 수신된 문자열을 파싱하여 처리하는 메소드
    def check_received_string(self, check_string):
        strings = list(check_string.split(","))
        if strings[0] == "file_path":
            return "1", strings[1]
        elif strings[0] == "Temp":
            return "Temp", strings[1]
        return "Error: " + strings[0] + "이(가) 정의되지 않았습니다."

class FactoryGraphDrawer:
    # 그래프 그리기에 필요한 값을 저장할 필드 선언
    __num_graph_variable = []
    __num_lines_per_graph_variable = []
    __secondary_y_per_graph_variable = []
    __line_column_per_graph_variable = []
    __line_name_per_graph_variable = []
    __line_colors_per_graph_variable = []
    __yaxes_basic_per_graph_variable = []
    __yaxes_basic_titles_per_graph_variable = []
    __yaxes_basic_units_per_graph_variable = []
    
    def __init__(self, df):
        self.__df = df      # 데이터프레임 지정
        self.__app = dash.Dash(__name__)
        
        # Dash 앱 레이아웃 정의
        self.__app.layout = html.Div([       
            dcc.Tabs(id='tabs', value=0, children=[
                dcc.Tab(label='1구역', value=0),
                dcc.Tab(label='2구역', value=1),
                dcc.Tab(label='3구역', value=2),
                dcc.Tab(label='4구역', value=3),
                ]),
            dcc.Graph(id="live-update-graph"),
            dcc.Interval(id="interval-component", interval=1 * 1000, n_intervals=0),
            ], style={'fontFamily': 'Noto Sans', 'fontWeight': '700'})

        # 콜백 메소드 등록
        self.__app.callback(
            Output("live-update-graph", "figure"), [Input('interval-component', 'n_intervals')], [State('tabs', 'value')]
            )(self.__update_graph_live)
    
    # 생성할 그래프 설정값을 저장하는 메소드
    def graph_set(self, num_graph, num_lines_per_graph, secondary_y_per_graph, line_column_per_graph, line_name_per_graph, line_colors_per_graph, 
                  yaxes_basic_per_graph, yaxes_basic_titles_per_graph, yaxes_basic_units_per_graph):    
        self.__num_graph_variable.append(num_graph)                                         # 그래프 개수
        self.__num_lines_per_graph_variable.append(num_lines_per_graph)                     # 각 그래프의 선 갯수
        self.__secondary_y_per_graph_variable.append(secondary_y_per_graph)                 # 각 그래프의 secondary_y 유무
        self.__line_column_per_graph_variable.append(line_column_per_graph)                 # 각 그래프의 선 데이터
        self.__line_name_per_graph_variable.append(line_name_per_graph)                     # 각 그래프의 선 이름
        self.__line_colors_per_graph_variable.append(line_colors_per_graph)                 # 각 그래프의 선 색깔
        self.__yaxes_basic_per_graph_variable.append(yaxes_basic_per_graph)                 # 각 그래프의 y축
        self.__yaxes_basic_titles_per_graph_variable.append(yaxes_basic_titles_per_graph)   # 각 그래프의 y축 이름
        self.__yaxes_basic_units_per_graph_variable.append(yaxes_basic_units_per_graph)     # 각 그래프의 y축 단위

    # 그래프를 실시간으로 업데이트하는 메소드
    def __update_graph_live(self, n, button_value):
        global now_time
        
        # 그래프에 표시할 데이터 선택
        start_index = self.__df.index[self.__df["Time"] == now_time][0]
        end_index = start_index + 100               # 표시 개수
        data = self.__df[start_index:end_index:1]

        # subplots 생성
        fig = make_subplots(rows=self.__num_graph_variable[button_value], cols=1, specs=[[{"secondary_y": True}] for _ in range(self.__num_graph_variable[button_value])])
        
        # 각 trace에 대한 정보를 전달하여 trace를 그리는 함수 호출
        for i in range (self.__num_graph_variable[button_value]):
            for j in range (self.__num_lines_per_graph_variable[button_value][i]):
                legendgroup = f"group_{i}_{j}"  # 그룹에 대한 고유한 legendgroup 값 생성
                if self.__secondary_y_per_graph_variable[button_value] and j == self.__yaxes_basic_per_graph_variable[button_value][i][0]:
                    self.__add_trace_to_figure(fig, data["Time"], data[self.__line_column_per_graph_variable[button_value][i][j]], self.__line_name_per_graph_variable[button_value][i][j], row=1 + i, col=1, secondary_y=False, line_color=self.__line_colors_per_graph_variable[button_value][i][j], legendgroup=legendgroup)
                else:
                    self.__add_trace_to_figure(fig, data["Time"], data[self.__line_column_per_graph_variable[button_value][i][j]], self.__line_name_per_graph_variable[button_value][i][j], row=1 + i, col=1, secondary_y=True, line_color=self.__line_colors_per_graph_variable[button_value][i][j], legendgroup=legendgroup)

        # 레이아웃 설정
        layout_height = 800
        layout_margin = {"l": 5, "r": 5, "b": 5, "t": 10}
        fig.update_layout(height=layout_height, margin=layout_margin)

        fig.update_xaxes(ticks="outside", tickwidth=2, tickfont_size=12, minor_ticks="outside")
        fig.update_yaxes(ticks="outside", tickwidth=2, tickfont_size=12, minor_ticks="outside")

        for i in range(self.__num_graph_variable[button_value]):
            fig.update_yaxes(title_text=self.__yaxes_basic_titles_per_graph_variable[button_value][i][0] + " " + self.__yaxes_basic_units_per_graph_variable[button_value][i], 
                             title_font_family="Noto Sans", title_font_size=16, row=i + 1, secondary_y=False)
            fig.update_yaxes(title_text=self.__yaxes_basic_titles_per_graph_variable[button_value][i][1] + " " + self.__yaxes_basic_units_per_graph_variable[button_value][i], 
                             title_font_family="Noto Sans", title_font_size=16, row=i + 1, secondary_y=True)

        fig.update_layout(
            legend_font_size=16,
            legend=dict(
                x=1,
                y=0.5,
                traceorder='normal',
                bgcolor='rgba(230,236,245,1)',
                bordercolor='rgba(0,0,0,0.3)',
                borderwidth=1))
        
        fig.update_layout(hovermode="x")
        fig.update_xaxes(showspikes=True, spikecolor="black", spikesnap="cursor", spikemode="across")
        
        fig.update_layout(modebar_remove=['zoom', 'pan', 'zoomIn', 'zoomOut', 'autoScale', 'resetScale'])

        return fig

    # 그래프에 trace를 추가하는 메소드
    def __add_trace_to_figure(self, fig, x, y, name, row, col, secondary_y, line_color, legendgroup):
        trace = go.Scatter(
            x=x,
            y=y,
            name=name,
            mode="lines",
            type="scatter",
            line=dict(color=line_color),
            legendgroup=legendgroup
        )
        fig.add_trace(trace, row=row, col=col, secondary_y=secondary_y)

    # Dash 앱 실행하는 메소드
    def run_app(self):
        self.__app.run_server(debug=False, use_reloader=False)
 
if __name__ == "__main__":
    dataset_url = "https://drive.google.com/uc?id=1ct6IrhLEwsJbNlFQpgSFGVQvx-Ff5kma&export=download"
    df = pd.read_csv(dataset_url, encoding="utf8")
    df["Time"] = pd.to_datetime(df["Time"], format="%Y-%m-%d_%H:%M:%S")     # 
    now_time = df["Time"][0]                                                # 첫번째 시간값으로 설정
    
    # 현재 시간을 주기적으로 업데이트하는 쓰레드
    def update_time():
        global now_time
        while True:
            print(f'Now Time: {now_time}')
            now_time = now_time + datetime.timedelta(seconds=1)
            while now_time not in df['Time'].values:
                now_time = now_time + datetime.timedelta(seconds=1)
            time.sleep(1)

    """
    def prepare_thread():
        print(f"\n클라이언트와의 연결 시도 중...")
        csh.prepare_socket()
        print(f"\n클라이언트 연결 성공!")
        while True:
            try:
                received_string = csh.received_socket_string()
                if received_string:
                    print(f"\n수신: {received_string}")
            except ConnectionResetError:
                print(f"\n클라이언트와의 연결이 끊겼습니다. 다시 연결 시도 중...")
                csh.prepare_socket()
                print(f"다시 연결되었습니다.")
    """
    
    graph_instance = FactoryGraphDrawer(df)
    
    num_graph = 3
    num_lines_per_graph = [2, 5, 3]
    secondary_y_per_graph = [True, True, True]
    line_column_per_graph = [["GN07N_MAIN_POWER", "GN07N_SUB_POWER"],
                       ["GN07N_TEMP", "GN07N_HIGH_TEMP", "GN07N_MID_TEMP", "GN07N_LOW_TEMP", "GN07N_OVER_TEMP"],
                       ["GN07N_GAS_NRG", "GN07N_GAS_AMM", "GN07N_GAS_CDO"]]
    line_name_per_graph = [["메인전력", "보조전력"],
                       ["온도", "상층온도", "중층온도", "하층온도", "과열온도"],
                        ["질소가스", "암모니아가스", "Co2가스"]]
    line_colors_per_graph = [["darkorange", "darkgoldenrod"],
                        ["red", "darkred", "darkorange", "orangered", "firebrick"],
                        ["forestgreen", "limegreen", "darkgreen"]]
    yaxes_basic_per_graph = [[0, 1],
                  [0, 1],
                  [1, 0]]
    yaxes_basic_titles_per_graph = [["메인전력", "보조전력"],
                       ["온도", "기타온도"],
                       ["암모니아가스", "질소, Co2가스"]]
    yaxes_basic_units_per_graph = ["[w]", "[°C]", "[㎥]"]
    
    graph_instance.graph_set(num_graph, num_lines_per_graph, secondary_y_per_graph, line_column_per_graph, line_name_per_graph, line_colors_per_graph, 
                             yaxes_basic_per_graph, yaxes_basic_titles_per_graph, yaxes_basic_units_per_graph)

    num_graph = 3
    num_lines_per_graph = [2, 4, 3]
    secondary_y_per_graph = [True, True, True]
    line_column_per_graph = [["GN05N_MAIN_POWER", "GN05M_MAIN_POWER"],
                             ["GN05M_TEMP", "GN05M_HIGH_TEMP", "GN05M_LOW_TEMP", "GN05M_OVER_TEMP"],
                             ["GN05M_GAS_NRG", "GN05M_GAS_AMM", "GN05M_GAS_CDO"]]
    line_name_per_graph = [["메인전력", "메인전력"],
                           ["온도", "상층온도", "하층온도", "과열온도"],
                           ["질소가스", "암모니아가스", "Co2가스"]]
    line_colors_per_graph = [["darkorange", "darkgoldenrod"],
                             ["red", "darkred", "orangered", "firebrick"],
                             ["forestgreen", "limegreen", "darkgreen"]]
    yaxes_basic_per_graph = [[0, 1],
                             [0, 1],
                             [1, 0]]
    yaxes_basic_titles_per_graph = [["메인전력", "메인전력"],
                                    ["온도", "기타온도"],
                                    ["암모니아가스", "질소, Co2가스"]]
    yaxes_basic_units_per_graph = ["[w]", "[°C]", "[㎥]"]

    graph_instance.graph_set(num_graph, num_lines_per_graph, secondary_y_per_graph, line_column_per_graph, line_name_per_graph, line_colors_per_graph, 
                             yaxes_basic_per_graph, yaxes_basic_titles_per_graph, yaxes_basic_units_per_graph)

    num_graph = 3
    num_lines_per_graph = [2, 5, 3]
    secondary_y_per_graph = [True, True, True]
    line_column_per_graph = [["GN04N_MAIN_POWER", "GN04M_MAIN_POWER"],
                             ["GN04M_TEMP", "GN04M_HIGH_TEMP", "GN04M_MID_TEMP", "GN04M_LOW_TEMP", "GN04M_OVER_TEMP"],
                             ["GN04M_GAS_NRG", "GN04M_GAS_AMM", "GN04M_GAS_CDO"]]
    line_name_per_graph = [["메인전력", "메인전력"],
                           ["온도", "상층온도", "중층온도", "하층온도", "과열온도"],
                           ["질소가스", "암모니아가스", "Co2가스"]]
    line_colors_per_graph = [["darkorange", "darkgoldenrod"],
                             ["red", "darkred", "darkorange", "orangered", "firebrick"],
                             ["forestgreen", "limegreen", "darkgreen"]]
    yaxes_basic_per_graph = [[0, 1],
                             [0, 1],
                             [1, 0]]
    yaxes_basic_titles_per_graph = [["메인전력", "메인전력"],
                                    ["온도", "기타온도"],
                                    ["암모니아가스", "질소, Co2가스"]]
    yaxes_basic_units_per_graph = ["[w]", "[°C]", "[㎥]"]
    
    graph_instance.graph_set(num_graph, num_lines_per_graph, secondary_y_per_graph, line_column_per_graph, line_name_per_graph, line_colors_per_graph, 
                             yaxes_basic_per_graph, yaxes_basic_titles_per_graph, yaxes_basic_units_per_graph)
    
    num_graph = 3
    num_lines_per_graph = [2, 5, 3]
    secondary_y_per_graph = [True, True, True]
    line_column_per_graph = [["GN03N_MAIN_POWER", "GN02N_MAIN_POWER"],
                             ["GN02N_TEMP", "GN02N_HIGH_TEMP", "GN02N_MID_TEMP", "GN02N_LOW_TEMP", "GN02N_OVER_TEMP"],
                             ["GN02N_GAS_NRG", "GN02N_GAS_AMM", "GN02N_GAS_CDO"]]
    line_name_per_graph = [["메인전력", "메인전력"],
                           ["온도", "상층온도", "중층온도", "하층온도", "과열온도"],
                           ["질소가스", "암모니아가스", "Co2가스"]]
    line_colors_per_graph = [["darkorange", "darkgoldenrod"],
                             ["red", "darkred", "darkorange", "orangered", "firebrick"],
                             ["forestgreen", "limegreen", "darkgreen"]]
    yaxes_basic_per_graph = [[0, 1],
                             [0, 1],
                             [1, 0]]
    yaxes_basic_titles_per_graph = [["메인전력", "메인전력"],
                                    ["온도", "기타온도"],
                                    ["암모니아가스", "질소, Co2가스"]]
    yaxes_basic_units_per_graph = ["[w]", "[°C]", "[㎥]"]
    
    graph_instance.graph_set(num_graph, num_lines_per_graph, secondary_y_per_graph, line_column_per_graph, line_name_per_graph, line_colors_per_graph, 
                             yaxes_basic_per_graph, yaxes_basic_titles_per_graph, yaxes_basic_units_per_graph)
    
    graph_thread = threading.Thread(target=graph_instance.run_app)
    update_time_thread = threading.Thread(target=update_time)
    
    graph_thread.start()
    update_time_thread.start()

    graph_thread.join()
    update_time_thread.join()