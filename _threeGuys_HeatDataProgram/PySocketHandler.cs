using System.Net.Sockets;
using System.Text;

namespace PySocketHandler
{
    class PySocketHandler
    {
        private TcpClient client;
        private NetworkStream stream;

        public string prepareSocket(string Address)
        {
            try
            {
                
                string serverIp = Address;
                //string serverIp = "127.0.0.1";
                int serverPort = 56792;

                client = new TcpClient(serverIp, serverPort);

                stream = client.GetStream();

                return "Success";
            }
            catch (Exception e)
            {
                return ("Error: " + e.Message);
            }
        }

        public void sendSocketString(string sendString)
        {
            byte[] data = Encoding.UTF8.GetBytes(sendString);
            stream.Write(data, 0, data.Length);
        }

        public string receivedSocketString()
        {
            byte[] data = new byte[256];
            int bytesRead = stream.Read(data, 0, data.Length);
            string receivedMessage = Encoding.UTF8.GetString(data, 0, bytesRead);

            return receivedMessage;
        }

        public string checkReceivedString(string checkString)
        {
            string[] strings = checkString.Split();
            if (strings[0] == "1")
            {
                return "1";
            }

            else if (strings[0] == "2")
            {
                return "2";
            }

            return "Error: " + strings[0] + "is not define";
        }
    }
}
