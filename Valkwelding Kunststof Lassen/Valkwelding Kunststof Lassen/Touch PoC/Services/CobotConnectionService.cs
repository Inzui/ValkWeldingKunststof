using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Modbus.Device;
using Microsoft.Extensions.Options;
using ValkWelding.Welding.PolyTouchApplication.Configuration;
using System.Globalization;
using System.Diagnostics;
using System.Threading;
using System.Net;

namespace ValkWelding.Welding.PolyTouchApplication.Services
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

        /// <summary>
        /// Asynchronously checks the connection to a specified IP address.
        /// </summary>
        /// <param name="ipAddress">The IP address to connect to.</param>
        /// <remarks>
        /// This method attempts to parse the provided IP address and then runs the ReadPos task.
        /// If the task completes successfully, it sets the CobotConnected flag to true.
        /// If any exception occurs during the execution of the task, it sets the CobotConnected flag to false, logs the exception, and throws a new exception indicating a connection error.
        /// If the provided IP address cannot be parsed, it sets the CobotConnected flag to false and throws an exception indicating an invalid IP address.
        /// </remarks>
        /// <exception cref="Exception">Thrown when a connection error occurs or when an invalid IP address is provided.</exception>
        public async Task CheckConnection(string ipAddress)
        {
            if (IPAddress.TryParse(ipAddress, out _))
            {
                _ipAddress = ipAddress;
                try
                {
                    await Task.Run(ReadPos);
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

        /// <summary>
        /// Sends a move command to a Cobot.
        /// </summary>
        /// <param name="point">An array of floats representing the point to move to.</param>
        /// <param name="speed">A float representing the speed of the movement.</param>
        /// <remarks>
        /// This method constructs a command string based on the provided point and speed, encodes it to bytes, and sends it over a TCP socket to the Cobot.
        /// It creates a new socket, connects to the robot, sends the command, and then disposes of the socket.
        /// </remarks>
        public void SendCobotMove(float[] point, float speed)
        {
            string command = $"Move_Line(\"TPP\",{ToText(point)},{speed},200,0,false)";

            using (Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                byte[] byteCommand = Encoding.UTF8.GetBytes(PacketSender(command));

                socket.Connect(_ipAddress, _port);
                socket.Send(byteCommand);
            }
        }

        /// <summary>
        /// Sends a position command to a Cobot robot.
        /// </summary>
        /// <param name="point">An array of floats representing the point to move to.</param>
        /// <param name="speed">A float representing the speed of the movement.</param>
        /// <remarks>
        /// This method constructs a command string based on the provided point and speed, encodes it to bytes, and sends it over a TCP socket to the Cobot.
        /// It creates a new socket, connects to the robot, sends the command, and then disposes of the socket.
        /// </remarks>
        public void SendCobotPos(float[] point, float speed)
        {
            string command = $"Line(\"CPP\",{ToText(point)},{speed},200,0,false)";

            using (Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                byte[] byteCommand = Encoding.UTF8.GetBytes(PacketSender(command));

                socket.Connect(_ipAddress, _port);
                socket.Send(byteCommand);
            }
        }

        /// <summary>
        /// Reads the current position of a Cobot robot.
        /// </summary>
        /// <returns>An array of floats representing the current position of the robot.</returns>
        /// <remarks>
        /// This method establishes a TCP connection to the robot, reads input registers from the robot's Modbus interface, converts the register values to floats, and returns these values in an array.
        /// It uses a Modbus master to perform the reading, and ensures that the TCP connection is properly closed after the operation.
        /// </remarks>
        public float[] ReadPos()
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
                        ret[i] = Floater(output[1], output[0]);
                    }
                }
            }

            return ret;
        }

        public void StopCobot()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the value of a coil in a Cobot's Modbus interface.
        /// </summary>
        /// <param name="address">The address of the coil to set.</param>
        /// <param name="value">The boolean value to set the coil to.</param>
        /// <remarks>
        /// This method establishes a TCP connection to the robot, reads the current value of the specified coil from the robot's Modbus interface, and if the current value does not match the desired value, writes the desired value to the coil.
        /// It uses a Modbus master to perform the reading and writing, and ensures that the TCP connection is properly closed after the operation.
        /// </remarks>
        public void SetCoilValue(int address, bool value)
        {
            using (TcpClient client = new TcpClient(_ipAddress, 502))
            {
                using (ModbusIpMaster master = ModbusIpMaster.CreateIp(client))
                {
                    if(master.ReadCoils((ushort)address, 1)[0] != value)
                    {
                        master.WriteSingleCoil((ushort)address, value);
                    }
                }
            }
        }

        /// <summary>
        /// Reads the error status from a Cobot's Modbus interface.
        /// </summary>
        /// <returns>An integer representing the error status.</returns>
        /// <remarks>
        /// This method checks if the IP address has been set, and if so, establishes a TCP connection to the robot, reads the error status from the robot's Modbus interface, and returns this value.
        /// If the IP address has not been set, it simply returns 0.
        /// It uses a Modbus master to perform the reading, and ensures that the TCP connection is properly closed after the operation.
        /// </remarks>
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

        private string PacketSender(string command)
        {
            string h = ChkStr(command);

            string d = Checksum(h);

            return $"${h}*{d}\r\n";
        }

        private string ChkStr(string str)
        {
            return $"TMSCT,{str.Length + 2},3,{str},";
        }

        private float Floater(ushort x, ushort y)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(x & 0xFF);
            bytes[1] = (byte)(x >> 8);
            bytes[2] = (byte)(y & 0xFF);
            bytes[3] = (byte)(y >> 8);
            return BitConverter.ToSingle(bytes, 0);
        }


        private string Checksum(string str)
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

        private string ToText(float[] point)
        {
            return $"{point[0].ToString(CultureInfo.InvariantCulture)}, {point[1].ToString(CultureInfo.InvariantCulture)}, {point[2].ToString(CultureInfo.InvariantCulture)}, {point[3].ToString(CultureInfo.InvariantCulture)}, {point[4].ToString(CultureInfo.InvariantCulture)}, {point[5].ToString(CultureInfo.InvariantCulture)}";
        }
    }
}
