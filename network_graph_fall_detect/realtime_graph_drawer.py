import os
import time
import datetime
import json
import dash
from dash import html, dcc
from dash.dependencies import Input, Output, State
import plotly.graph_objs as go
from plotly.subplots import make_subplots

class RealtimeGraphDrawer:
    # 현재 시간을 저장할 변수 선언
    __now_time = None
    __graph_config = None

    __now_states = list()
    __now_trace_list = list()
    __states_variable = dict()
    __trace_list_variable = dict()

    def __init__(self):
        # Dash 앱 인스턴스 생성
        self.__app = dash.Dash(__name__)

        # 그레프 설정 파일 불러오기
        file_name = r"graph_config.json"
        graph_config_path = os.path.abspath(file_name)
        with open(graph_config_path, 'r', encoding='utf8') as file:
            self.__graph_config = json.load(file)

        for graph in self.__graph_config.keys():
            self.__states_variable[graph] = dict()
            self.__trace_list_variable[graph] = dict()

            for figure in self.__graph_config[graph].keys():
                num_traces = 0
                for subplot in self.__graph_config[graph][figure]['subplot'].keys():
                    num_traces += len(self.__graph_config[graph][figure]['subplot'][subplot]["trace"])

                self.__states_variable[graph][figure] = list([True] * num_traces)
                self.__trace_list_variable[graph][figure] = list(range(num_traces))

        # Dash 앱 레이아웃 정의
        tabs_children = []
        for idx, graph in enumerate(self.__graph_config.keys()):
            tab_label = f'{idx + 1}구역'
            tab_value = graph
            tabs_children.append(dcc.Tab(label=tab_label, value=tab_value))

        self.__app.layout = html.Div(
            [
                dcc.Tabs(
                    id='tabs',
                    value="graph1",
                    children=tabs_children
                ),
                dcc.RangeSlider(
                    min=1,
                    max=12,
                    step=0.01,
                    marks={i: '{}'.format(2 ** i) for i in range(1, 13)} | {12: 'All'},
                    id='index_ranges',
                    value=[7],
                    dots=False,
                    updatemode='drag'
                ),
                html.Div([
                    dcc.Graph(
                    id='figure1',
                    )
                ], style={'width': '48%', 'display': 'inline-block'}),
                html.Div([
                    dcc.Graph(
                    id='figure2',
                    )
                ], style={'width': '48%', 'display': 'inline-block', 'float': 'right'}),
                html.Div([
                    dcc.Graph(
                    id='figure3',
                    )
                ], style={'width': '48%', 'display': 'inline-block'}),
                html.Div([
                    dcc.Graph(
                    id='figure4',
                    )
                ], style={'width': '48%', 'display': 'inline-block', 'float': 'right'}),
                html.Div([
                    dcc.Graph(
                    id='figure5',
                    )
                ], style={'width': '48%', 'display': 'inline-block'}),
                html.Div([
                    dcc.Graph(
                    id='figure6',
                    )
                ], style={'width': '48%', 'display': 'inline-block', 'float': 'right'}),
                dcc.Interval(id="interval-component", interval=1 * 1000, n_intervals=0),
                html.Button("Stop/Start", id="stop-start-button", n_clicks=0),
            ],
            style={'fontFamily': 'Noto Sans', 'fontWeight': '700'}
        )

        # 콜백 메소드 등록
        self.__app.callback(
            Output("figure1", "figure"),
            [Input('interval-component', 'n_intervals')],
            [State('figure1', 'id'), State('tabs', 'value'), State('index_ranges', 'value'), State('figure1', 'restyleData')]
        )(self.__update_graph_live)

        self.__app.callback(
            Output("figure2", "figure"),
            [Input('interval-component', 'n_intervals')],
            [State('figure2', 'id'), State('tabs', 'value'), State('index_ranges', 'value'), State('figure2', 'restyleData')]
        )(self.__update_graph_live)

        self.__app.callback(
            Output("figure3", "figure"),
            [Input('interval-component', 'n_intervals')],
            [State('figure3', 'id'), State('tabs', 'value'), State('index_ranges', 'value'), State('figure3', 'restyleData')]
        )(self.__update_graph_live)

        self.__app.callback(
            Output("figure4", "figure"),
            [Input('interval-component', 'n_intervals')],
            [State('figure4', 'id'), State('tabs', 'value'), State('index_ranges', 'value'), State('figure4', 'restyleData')]
        )(self.__update_graph_live)

        self.__app.callback(
            Output("figure5", "figure"),
            [Input('interval-component', 'n_intervals')],
            [State('figure5', 'id'), State('tabs', 'value'), State('index_ranges', 'value'), State('figure5', 'restyleData')]
        )(self.__update_graph_live)

        self.__app.callback(
            Output("figure6", "figure"),
            [Input('interval-component', 'n_intervals')],
            [State('figure6', 'id'), State('tabs', 'value'), State('index_ranges', 'value'), State('figure6', 'restyleData')]
        )(self.__update_graph_live)

        self.__app.callback(
            Output("interval-component", "disabled"),
            [Input("stop-start-button", "n_clicks")],
            [State("interval-component", "disabled")]
        )(self.__toggle_interval)

    def dataframe_set(self, df):
        self.__df = df

    def graph_config_file_set(self, graph_config_path):
        with open(graph_config_path, 'r', encoding='utf8') as file:
            self.__graph_config = json.load(file)

    # 시간을 주기적으로 업데이트하는 쓰레드
    def update_time(self, start_index):
        # 시작 시간 지정
        self.__now_time = self.__df["Time"][start_index]

        # 시간 갱신
        while True:
            self.__now_time = self.__now_time + datetime.timedelta(seconds=1)
            while self.__now_time not in self.__df['Time'].values:
                self.__now_time = self.__now_time + datetime.timedelta(seconds=1)
            time.sleep(1)

    # 그래프를 실시간으로 업데이트하는 메소드
    def __update_graph_live(self, n_intervals, figure, current_tab, index_ranges, restyleData):  

        end_index = self.__df.index[self.__df["Time"] == self.__now_time][0]
        if index_ranges[0] == 12:
            start_index = 0
        else:
            index_range = int(2 ** index_ranges[0])
            start_index = end_index - index_range
            if start_index < 0:
                start_index = 0
        data = self.__df[start_index:end_index:1]

        # 서브플롯 생성
        rows = list()
        cols = list()
        specs = list()

        for subplot in self.__graph_config[current_tab][figure]['subplot'].keys():
            rows.append(self.__graph_config[current_tab][figure]['subplot'][subplot]['row'])
            cols.append(self.__graph_config[current_tab][figure]['subplot'][subplot]['col'])

        rows = max(rows)    
        cols = max(cols)

        for _ in range (rows):
            spec = list()
            for _ in range(cols):
                spec.append({"secondary_y": True})
            specs.append(spec)

        fig = make_subplots(
            rows=rows, 
            cols=cols, 
            specs=specs
        )

        # 각 trace에 대한 정보를 전달하여 trace를 그리는 함수 호출
        for subplot in self.__graph_config[current_tab][figure]["subplot"].keys():
            for trace in self.__graph_config[current_tab][figure]["subplot"][subplot]["trace"].keys():

                row = self.__graph_config[current_tab][figure]['subplot'][subplot]['row']
                col = self.__graph_config[current_tab][figure]['subplot'][subplot]['col']
                secondary_y = self.__graph_config[current_tab][figure]["subplot"][subplot]["trace"][trace]["secondary_y"]

                trace = go.Scatter(
                    x=data["Time"],
                    y=data[self.__graph_config[current_tab][figure]["subplot"][subplot]["trace"][trace]["column"]],
                    name=self.__graph_config[current_tab][figure]["subplot"][subplot]["trace"][trace]["name"],
                    mode=self.__graph_config[current_tab][figure]["subplot"][subplot]["trace"][trace]["mode"],
                    type=self.__graph_config[current_tab][figure]["subplot"][subplot]["trace"][trace]["type"],
                    line=dict(color=self.__graph_config[current_tab][figure]["subplot"][subplot]["trace"][trace]["color"])
                )
                fig.add_trace(trace, row=row, col=col, secondary_y=secondary_y)

        # 레이아웃 설정
        fig.update_layout(
            title=self.__graph_config[current_tab][figure]['title'],
            height=self.__graph_config[current_tab][figure]['height'],
            margin=self.__graph_config[current_tab][figure]['margin'],
            legend_font_size=self.__graph_config[current_tab][figure]['legend_font_size'],
            hovermode=self.__graph_config[current_tab][figure]['hovermode'],
            xaxis=self.__graph_config[current_tab][figure]['xaxis'],
            legend=self.__graph_config[current_tab][figure]['legend'],
            modebar_add=['drawline',
                'drawopenpath',
                'drawclosedpath',
                'drawcircle',
                'drawrect',
                'eraseshape'
            ]
        )

        # x축 및 y축 설정
        for subplot in self.__graph_config[current_tab][figure]['subplot'].keys():
            fig.update_xaxes(self.__graph_config[current_tab][figure]['subplot'][subplot]['xaxis_config'])
            fig.update_yaxes(self.__graph_config[current_tab][figure]['subplot'][subplot]['yaxis_config'])
            for i in range(2):
                title_text = self.__graph_config[current_tab][figure]['subplot'][subplot]['yaxes_title']
                secondary_y = False
                if i != 0:
                    for trace in self.__graph_config[current_tab][figure]["subplot"][subplot]["trace"]:
                        if self.__graph_config[current_tab][figure]["subplot"][subplot]["trace"][trace]["secondary_y"]:
                            secondary_y = True
                            title_text = self.__graph_config[current_tab][figure]['subplot'][subplot]['secondary_y_yaxes_title']
                            break

                fig.update_yaxes(
                title_text=title_text,
                title_font_family="Noto Sans",
                title_font_size=16,
                row = self.__graph_config[current_tab][figure]['subplot'][subplot]['row'],
                col = self.__graph_config[current_tab][figure]['subplot'][subplot]['col'],
                secondary_y=secondary_y
                )
                
        # 범례 ON / OFF 기능
        if restyleData is not None:
            info, trace_list = restyleData
            states = info['visible']
            
            # 현재 상태와 트레이스 리스트가 변경되었을 때만 업데이트
            if (self.__now_states != states) or (self.__now_trace_list != trace_list):
                self.__now_states = states
                self.__now_trace_list = trace_list
                for i, j in zip(trace_list, range(len(trace_list))):
                    self.__states_variable[current_tab][figure][i] = states[j]

        current_fig = fig.to_dict()

        # 트레이스의 가시성 업데이트
        for state, trace in zip(self.__states_variable[current_tab][figure], self.__trace_list_variable[current_tab][figure]):
            current_fig['data'][trace].update({'visible': state})

        return current_fig

    def __toggle_interval(self, n_clicks, interval_disabled):
        # Toggle the interval (stop/start) based on button clicks
        return not interval_disabled

    # Dash 앱 실행하는 메소드
    def run_app(self):
        self.__app.run_server(host='0.0.0.0', debug=False, use_reloader=False)