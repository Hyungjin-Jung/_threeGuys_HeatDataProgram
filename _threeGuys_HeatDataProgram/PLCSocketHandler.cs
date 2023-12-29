using XGCommLib;

namespace PLCSocketHandler
{
    public enum XGCOMM_PRE_DEFINES : uint
    {
        MAX_RW_BIT_SIZE = 64,                   // 최대 읽기/쓰기 비트 크기
        MAX_RW_BYTE_SIZE = 1000,                // 최대 읽기/쓰기 바이트 크기
        MAX_RW_WORD_SIZE = 500,                 // 최대 읽기/쓰기 워드 크기
        DEF_PLC_SERVER_TIME_OUT = 15000,        // 기본 PLC 서버 타임아웃 (15초)
        DEF_PLC_KEEP_ALIVE_TIME = 10000         // 기본 PLC Keep Alive 시간 (10초)
    }

    public class DEF_DATA_TYPE
    {
        public const char DATA_TYPE_BIT = 'X';  // 데이터 타입: 비트
        public const char DATA_TYPE_BYTE = 'B'; // 데이터 타입: 바이트
        public const char DATA_TYPE_WORD = 'W'; // 데이터 타입: 워드
    }

    public enum XGCOMM_FUNC_RESULT : uint
    {
        RT_XGCOMM_SUCCESS = 0,                  // 함수가 수행 성공

        RT_XGCOMM_CAN_NOT_FIND_DLL = 1,         // XGCommLib.dll 파일을 찾을 수 없음, 윈도우 system32폴더의 regsvr32.exe를 이용해 등록필요
        RT_XGCOMM_FAILED_CONNECT = 2,           // PLC와 통신 접속 실패
        RT_XGCOMM_FAILED_KEEPALIVE = 3,         // PLC와 통신 접속 상태 유지 실패

        RT_XGCOMM_INVALID_COMM_DRIVER = 5,      // Comm Driver가 유효하지 않음, Connect함수를 호출하지않았거나 Disconnect를 호출한 상태
        RT_XGCOMM_INVALID_POINT = 6,	        // 함수의 인자로 전달한 배열 포인트가 NULL일 때   

        RT_XGCOMM_FAILED_RESULT = 10,           // XGCommLib.dll의 함수 실행이 실패했을 때
        RT_XGCOMM_FAILED_READ = 11,             // XGCommLib.dll의 ReadRandomDevice 함수의 반환값이 0으로 실패했을 때
        RT_XGCOMM_FAILED_WRITE = 12,            // XGCommLib.dll의 WriteRandomDevice 함수의 반환값이 0으로 실패했을 때

        RT_XGCOMM_ABOVE_MAX_BIT_SIZE = 20,      // Bit 함수의 Bit Size가 32를 초과했을 때(ReadDataBit, WriteDataBit)
        RT_XGCOMM_ABOVE_MAX_BYTE_SIZE = 21,     // Byte 함수의 Byte Size가 260를 초과했을 때(ReadDataByte, WriteDataByte)
        RT_XGCOMM_ABOVE_MAX_WORD_SIZE = 22,     // Word 함수의 Word Size가 130를 초과했을 때(ReadDataWord, WriteDataWord)
        RT_XGCOMM_BLOW_MIN_SIZE = 23,           // Size가 1보다 작을 때

        RT_XGCOMM_FAILED_GET_TIMEOUT = 25,	    // 타임아웃읽기 실패
        RT_XGCOMM_FAILED_SET_TIMEOUT = 26,	    // 타임아웃설정 실패
    }

    class PLCSocketManager
    {
        private CommObject20 m_CommDriver = null;
        private Int32 m_nLastCommTime = 0;
        private string m_PLCIPAddress;
        private long m_PLCPortNumber;

        // 연결 함수
        public uint Connect(string PLCIPAddress, long PLCPortNumber)
        {
            // 주소 또는 포트번호가 변경된 경우
            if ((m_PLCIPAddress != PLCIPAddress) || (m_PLCPortNumber != PLCPortNumber))
            {
                if (this.m_CommDriver != null)
                {
                    this.m_CommDriver.RemoveAll();
                    this.m_CommDriver.Disconnect();
                    this.m_CommDriver = null;
                }

                string strConnection = string.Format("{0}:{1}", PLCIPAddress, PLCPortNumber);
                CommObjectFactory20 factory = new CommObjectFactory20();
                this.m_CommDriver = factory.GetMLDPCommObject20(strConnection);
            }
            else
            {
                // 연결이 끊긴 경우
                if (this.m_CommDriver == null)
                {
                    string strConnection = string.Format("{0}:{1}", PLCIPAddress, PLCPortNumber);
                    CommObjectFactory20 factory = new CommObjectFactory20();
                    this.m_CommDriver = factory.GetMLDPCommObject20(strConnection);
                }
                else
                {
                    m_CommDriver.Disconnect();
                }
            }

            // PLC 연결
            if (0 == m_CommDriver.Connect(""))
            {
                return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_FAILED_CONNECT;
            }

            m_PLCIPAddress = PLCIPAddress;
            m_PLCPortNumber = PLCPortNumber;

            m_nLastCommTime = Environment.TickCount;
            return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS;
        }

        // 연결 해제 함수
        public uint Disconnect()
        {
            if (this.m_CommDriver != null)
            {
                this.m_CommDriver.Disconnect();

                return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS;
            }
            return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_INVALID_COMM_DRIVER;
        }

        // Keep Alive 업데이트 함수
        public uint UpdateKeepAlive()
        {
            uint dwTimeSpen;
            uint dwReturn;

            if (this.m_CommDriver == null)
            {
                return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_INVALID_COMM_DRIVER;
            }

            dwTimeSpen = (uint)TICKS_DIFF(m_nLastCommTime, Environment.TickCount);

            if (dwTimeSpen > (uint)XGCOMM_PRE_DEFINES.DEF_PLC_KEEP_ALIVE_TIME)
            {
                dwReturn = ReadData('F', 'X', 0, 1, null);
                if (dwReturn != (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS)
                {
                    if (dwTimeSpen > (uint)XGCOMM_PRE_DEFINES.DEF_PLC_SERVER_TIME_OUT)
                    {
                        return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_FAILED_KEEPALIVE;
                    }
                }
                else
                {
                    m_nLastCommTime = Environment.TickCount;
                }
            }

            return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS;
        }

        // 데이터 읽기 함수
        public uint ReadData(char PLCMemoryLocation, char PLCMemoryAccessSize, long PLCMemoryByteOffset, long PLCMemoryBitOffset, UInt16[] pbyRead)
        {
            // Comm Driver가 유효하지 않은 경우
            if (this.m_CommDriver == null)
            {
                return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_INVALID_COMM_DRIVER;
            }

            // 읽기 비트 크기 초과
            if (PLCMemoryBitOffset > (uint)XGCOMM_PRE_DEFINES.MAX_RW_BIT_SIZE)
            {
                return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_ABOVE_MAX_BIT_SIZE;
            }

            // 읽기 비트 크기 부적절
            if (PLCMemoryBitOffset < 1)
            {
                return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_BLOW_MIN_SIZE;
            }

            CommObjectFactory20 factory = new CommObjectFactory20();
            XGCommLib.DeviceInfo oDevice = factory.CreateDevice();
            this.m_CommDriver.RemoveAll();

            // 데이터 타입에 따라 디바이스 정보 설정
            switch (PLCMemoryAccessSize)
            {
                case DEF_DATA_TYPE.DATA_TYPE_BIT:
                    for (int i = 0; i < PLCMemoryBitOffset; i++)
                    {
                        oDevice.ucDeviceType = (byte)PLCMemoryLocation;
                        oDevice.ucDataType = (byte)'X';
                        oDevice.lOffset = (Int32)((PLCMemoryByteOffset + i) / 8);
                        oDevice.lSize = (Int32)((PLCMemoryByteOffset + i) % 8);

                        this.m_CommDriver.AddDeviceInfo(oDevice);
                    }
                    break;

                case DEF_DATA_TYPE.DATA_TYPE_BYTE:
                    oDevice.ucDeviceType = (byte)PLCMemoryLocation;
                    oDevice.ucDataType = (byte)'B';
                    oDevice.lOffset = (Int32)PLCMemoryByteOffset;
                    oDevice.lSize = (Int32)PLCMemoryBitOffset;

                    this.m_CommDriver.AddDeviceInfo(oDevice);
                    break;

                case DEF_DATA_TYPE.DATA_TYPE_WORD:
                    oDevice.ucDeviceType = (byte)PLCMemoryLocation;
                    oDevice.ucDataType = (byte)'B';
                    PLCMemoryByteOffset *= 2;
                    PLCMemoryBitOffset *= 2;
                    oDevice.lOffset = (Int32)(PLCMemoryByteOffset);
                    oDevice.lSize = (Int32)(PLCMemoryBitOffset);

                    this.m_CommDriver.AddDeviceInfo(oDevice);
                    break;
            }

            byte[] bufRead = new byte[PLCMemoryBitOffset];
            long lRetValue = this.m_CommDriver.ReadRandomDevice(bufRead);

            // 읽기 실패 시 재시도
            if (0 == lRetValue)
            {
                uint dwReteurn = Connect(m_PLCIPAddress, m_PLCPortNumber);
                if (dwReteurn == (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS)
                {
                    lRetValue = this.m_CommDriver.ReadRandomDevice(bufRead);
                    if (0 == lRetValue)
                    {
                        return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_FAILED_READ;
                    }
                }
                else
                {
                    return dwReteurn;
                }
            }

            // 데이터 복사
            if (pbyRead != null && PLCMemoryAccessSize != 'W')
            {
                bufRead.CopyTo(pbyRead, 0);
            }
            if (pbyRead != null && PLCMemoryAccessSize == 'W')
            {
                System.Buffer.BlockCopy(bufRead, 0, pbyRead, 0, (Int32)PLCMemoryBitOffset);
            }

            m_nLastCommTime = Environment.TickCount;

            return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS;
        }

        // 데이터 쓰기 함수
        public uint WriteData(char PLCMemoryLocation, char PLCMemoryAccessSize, long PLCMemoryByteOffset, long PLCMemoryBitOffset, string WriteData)
        {
            // Comm Driver가 유효하지 않은 경우
            if (this.m_CommDriver == null)
            {
                return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_INVALID_COMM_DRIVER;
            }

            // 쓰기 비트 크기 초과
            if (PLCMemoryBitOffset > (uint)XGCOMM_PRE_DEFINES.MAX_RW_BIT_SIZE)
            {
                return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_ABOVE_MAX_BIT_SIZE;
            }

            // 쓰기 비트 크기 부적절
            if (PLCMemoryBitOffset < 1)
            {
                return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_BLOW_MIN_SIZE;
            }

            // 쓰기 데이터가 없는 경우
            if (WriteData == null)
            {
                return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_INVALID_POINT;
            }

            // PLCWriteTextBox의 문자열을 바이트 또는 UInt16로 변환
            byte byEditData = Convert.ToByte(WriteData);
            byte[] byteArrayWriteData = new byte[PLCMemoryBitOffset];
            UInt16 uEditData = Convert.ToUInt16(WriteData);
            UInt16[] ushortArrayWriteData = new UInt16[PLCMemoryBitOffset];

            // 데이터 배열에 데이터 할당
            for (int i = 0; i < PLCMemoryBitOffset; i++)
            {
                if (PLCMemoryAccessSize == 'W')
                {
                    ushortArrayWriteData[i] = uEditData;
                }
                else
                {
                    byteArrayWriteData[i] = (byte)byEditData;
                }
            }

            // 만약 PLCMemoryAccessSize가 'W'인 경우 16비트 데이터를 8비트로 변환
            if (PLCMemoryAccessSize == 'W')
            {
                byteArrayWriteData = ushortArrayWriteData.SelectMany(BitConverter.GetBytes).ToArray();
            }

            CommObjectFactory20 factory = new CommObjectFactory20();
            this.m_CommDriver.RemoveAll();
            XGCommLib.DeviceInfo oDevice;

            // 데이터 타입에 따라 디바이스 정보 설정
            switch (PLCMemoryAccessSize)
            {
                case DEF_DATA_TYPE.DATA_TYPE_BIT:
                    for (int i = 0; i < PLCMemoryBitOffset; i++)
                    {
                        oDevice = factory.CreateDevice();

                        oDevice.ucDeviceType = (byte)PLCMemoryLocation;
                        oDevice.ucDataType = (byte)'X';
                        oDevice.lOffset = (Int32)((PLCMemoryByteOffset + i) / 8);
                        oDevice.lSize = (Int32)((PLCMemoryByteOffset + i) % 8);

                        this.m_CommDriver.AddDeviceInfo(oDevice);
                    }
                    break;

                case DEF_DATA_TYPE.DATA_TYPE_BYTE:
                    oDevice = factory.CreateDevice();

                    oDevice.ucDeviceType = (byte)PLCMemoryLocation;
                    oDevice.ucDataType = (byte)'B';
                    oDevice.lOffset = (Int32)PLCMemoryByteOffset;
                    oDevice.lSize = (Int32)PLCMemoryBitOffset;

                    this.m_CommDriver.AddDeviceInfo(oDevice);
                    break;

                case DEF_DATA_TYPE.DATA_TYPE_WORD:
                    oDevice = factory.CreateDevice();

                    oDevice.ucDeviceType = (byte)PLCMemoryLocation;
                    oDevice.ucDataType = (byte)'B';
                    PLCMemoryByteOffset *= 2;
                    PLCMemoryBitOffset *= 2;
                    oDevice.lOffset = (Int32)PLCMemoryByteOffset;
                    oDevice.lSize = (Int32)PLCMemoryBitOffset;

                    this.m_CommDriver.AddDeviceInfo(oDevice);
                    break;
            }
            long lRetValue = this.m_CommDriver.WriteRandomDevice(byteArrayWriteData);

            // 쓰기 실패 시 재시도
            if (0 == lRetValue)
            {
                uint dwReteurn = Connect(m_PLCIPAddress, m_PLCPortNumber);
                if (dwReteurn == (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS)
                {
                    lRetValue = this.m_CommDriver.ReadRandomDevice(byteArrayWriteData);
                    if (0 == lRetValue)
                    {
                        return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_FAILED_WRITE;
                    }
                }
                else
                {
                    return dwReteurn;
                }
            }

            m_nLastCommTime = Environment.TickCount;

            return (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS;
        }

        public string getResultCodeString(uint resultCode)
        {
            string resultString = "";

            switch (resultCode)
            {
                case (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS:
                    resultString = "성공";
                    break;

                case (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_CAN_NOT_FIND_DLL:
                    resultString = "“XGCommLib.dll”을 찾을 수 없음";
                    break;

                case (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_FAILED_CONNECT:
                    resultString = "연결 실패";
                    break;

                case (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_FAILED_KEEPALIVE:
                    resultString = "Keep alive 실패";
                    break;

                case (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_INVALID_COMM_DRIVER:
                    resultString = "잘못된 통신 드라이버";
                    break;

                case (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_INVALID_POINT:
                    resultString = "잘못된 데이터 포인트";
                    break;

                case (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_FAILED_RESULT:
                    resultString = "통신 드라이버 실패";
                    break;

                case (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_FAILED_READ:
                    resultString = "데이터 읽기 실패";
                    break;

                case (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_FAILED_WRITE:
                    resultString = "데이터 쓰기 실패";
                    break;

                case (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_ABOVE_MAX_BIT_SIZE:
                    resultString = "최대 비트 크기를 초과함[최대 비트: 32]";
                    break;

                case (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_ABOVE_MAX_BYTE_SIZE:
                    resultString = "최대 바이트 크기를 초과함[최대 바이트: 260]";
                    break;

                case (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_ABOVE_MAX_WORD_SIZE:
                    resultString = "최대 워드 크기를 초과함[최대 워드: 130]";
                    break;

                default:
                    resultString = "알 수 없는 반환 코드";
                    break;
            }

            return resultString;
        }

        private static Int32 TICKS_DIFF(int prev, int cur)
        {
            Int32 nReturn;
            if (cur >= prev)
            {
                nReturn = cur - prev;
            }
            else
            {
                unchecked
                {
                    nReturn = ((int)0xFFFFFFFF - prev) + 1 + cur;
                }
            }
            return nReturn;
        }
    }
}
