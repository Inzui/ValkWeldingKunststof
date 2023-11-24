using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Modbus.Device;
using Microsoft.Extensions.Options;
using ValkWelding.Welding.Touch_PoC.Configuration;
using System.Globalization;
using System.Diagnostics;
using System.Threading;
using System.Net;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public class CobotConnectionService : ICobotConnectionService
    {
        public bool CobotConnected { get; set; }
        public bool CobotInRunMode { get { return ReadError() == 6; } }

        private string _ipAddress;
        private int _port;

        public CobotConnectionService(IOptions<LocalConfig> configuration)
        {
            CobotConnected = false;
            _port = configuration.Value.CobotSettings.Port;
        }

        public async Task CheckConnection(string ipAddress)
        {
            if (IPAddress.TryParse(ipAddress, out _))
            {
                _ipAddress = ipAddress;
                try
                {
                    await Task.Run(readPos);
                    CobotConnected = true;
                }
                catch (Exception ex)
                {
                    CobotConnected = false;
                    Debug.WriteLine(ex);
                    throw new Exception("Connection Error");
                }
            }
            else
            {
                CobotConnected = false;
                throw new Exception("Invalid IP-Address");
            }
        }

        public void sendCobotMove(float[] point, float speed)
        {
            string command = $"Move_PTP(\"TPP\",{toText(point)},{speed},200,0,false)";

            using (Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                byte[] byteCommand = Encoding.UTF8.GetBytes(packetSender(command));

                socket.Connect(_ipAddress, _port);
                socket.Send(byteCommand);
            }
        }

        public void sendCobotPos(float[] point, float speed)
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

            using (TcpClient client = new(_ipAddress, 502))
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

        public void StopCobot()
        {
            throw new NotImplementedException();
            //using (TcpClient client = new TcpClient(ipAddress, 502))
            //{
            //    using (ModbusIpMaster master = ModbusIpMaster.CreateIp(client))
            //    {
            //        master.WriteSingleCoil(9, false);

            //        master.WriteSingleCoil(7106, false);
            //        Thread.Sleep(500);
            //        master.WriteSingleCoil(7106, true);
            //        Thread.Sleep(500);
            //        master.WriteSingleCoil(7106, false);

            //        master.WriteSingleCoil(9, true);

            //        Debug.WriteLine("Stopped Cobot");
            //    }
            //}
        }

        public int ReadError()
        {
            if (!string.IsNullOrEmpty(_ipAddress))
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
            return 0;
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
            return $"{point[0].ToString(CultureInfo.InvariantCulture)}, {point[1].ToString(CultureInfo.InvariantCulture)}, {point[2].ToString(CultureInfo.InvariantCulture)}, {point[3].ToString(CultureInfo.InvariantCulture)}, {point[4].ToString(CultureInfo.InvariantCulture)}, {point[5].ToString(CultureInfo.InvariantCulture)}";
        }
    }
}
