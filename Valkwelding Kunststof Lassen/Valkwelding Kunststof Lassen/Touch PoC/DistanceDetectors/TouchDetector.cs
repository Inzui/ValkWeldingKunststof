using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using ValkWelding.Welding.Touch_PoC.Types;

namespace ValkWelding.Welding.Touch_PoC.DistanceDetectors
{
    public class TouchDetector : IDistanceDetector
    {
        public bool ObjectDetected { get; set; }

        private readonly SerialPort _serialPort;
        private bool _connected;

        public TouchDetector()
        {
            _serialPort = new()
            {
                PortName = "COM6",
                BaudRate = 9600,
            };
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);
            _connected = false;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = (SerialPort)sender;
            int _bytesToRead = serialPort.BytesToRead;
            byte[] recvData = new byte[_bytesToRead];
            serialPort.Read(recvData, 0, _bytesToRead);

            _connected = true;
            ObjectDetected = recvData.Last() == 1;
        }

        public void Start()
        {
            _serialPort.Open();
            while (!_connected)
            {
                SendCommand(DetectorCommand.StartDetecting);
            }
        }

        private void SendCommand(DetectorCommand command)
        {
            byte[] commandArray = { (byte)command };
            _serialPort.Write(commandArray, 0, 1);
        }
    }
}
