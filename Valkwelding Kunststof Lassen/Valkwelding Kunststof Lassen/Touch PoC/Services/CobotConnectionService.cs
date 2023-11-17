using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Modbus.Device;
using Microsoft.Extensions.Options;
using ValkWelding.Welding.Touch_PoC.Configuration;
using System.Net;
using System.Diagnostics;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public class CobotConnectionService : ICobotConnectionService
    {
        private string _ipAddress;
        private int _port;

        public CobotConnectionService(IOptions<LocalConfig> configuration)
        {
            _port = configuration.Value.CobotSettings.Port;
        }

        public void Connect(string ipAddress)
        {
            if (IPAddress.TryParse(ipAddress, out _))
            {
                _ipAddress = ipAddress;
            }
            else
            {
                Debug.WriteLine("INVALID IP");
            }
        }

        public void sendCobotMove(float[] point, int speed)
        {
            string command = $"Move_PTP(\"TPP\",{toText(point)},{speed},200,0,false)";

            using (Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                byte[] byteCommand = Encoding.UTF8.GetBytes(packetSender(command));

                socket.Connect(_ipAddress, _port);
                socket.Send(byteCommand);
            }
        }

        public void sendCobotPos(float[] point, int speed)
        {
            string command = $"PTP(\"CPP\",{toText(point)},{speed},200,0,false)";

            using (Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                byte[] byteCommand = Encoding.UTF8.GetBytes(packetSender(command));

                socket.Connect(_ipAddress, _port);
                socket.Send(byteCommand);
            }
        }

        public float[] readPos()
        {
            float[] ret = new float[6];

            using (TcpClient client = new TcpClient(_ipAddress, 502))
            {
                using (ModbusIpMaster master = ModbusIpMaster.CreateIp(client))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        ushort address = Convert.ToUInt16(7001 + i * 2);
                        var output = master.ReadInputRegisters(address, 2);
                        ret[i] = floater(output[1], output[0]);
                    }
                }
            }

            return ret;
        }

        public int readError()
        {
            using (TcpClient client = new TcpClient(_ipAddress, 502))
            {
                using (ModbusIpMaster master = ModbusIpMaster.CreateIp(client))
                {
                    var output = master.ReadInputRegisters(7332, 1);
                    return output[0];
                }
            }
        }

        private string packetSender(string command)
        {
            string h = chkStr(command);

            string d = checksum(h);

            return $"${h}*{d}\r\n";
        }

        private string chkStr(string str)
        {
            return $"TMSCT,{str.Length + 2},3,{str},";
        }

        private float floater(ushort x, ushort y)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(x & 0xFF);
            bytes[1] = (byte)(x >> 8);
            bytes[2] = (byte)(y & 0xFF);
            bytes[3] = (byte)(y >> 8);
            return BitConverter.ToSingle(bytes, 0);
        }


        private string checksum(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            int result = bytes[0];

            for (int i = 1; i < bytes.Length; i++)
            {
                result = result ^ bytes[i];
            }

            string d = result.ToString("X").ToUpper();

            if (result < 16)
            {
                d = $"0" + d[^1];
            }
            else
            {
                d = d[^2..^0];
            }

            return d;

        }

        private string toText(float[] point)
        {
            return $"{point[0]}, {point[1]}, {point[2]}, {point[3]}, {point[4]}, {point[5]}";
        }
    }
}
