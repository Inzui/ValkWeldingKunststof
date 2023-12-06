using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using ValkWelding.Welding.Touch_PoC.Configuration;
using ValkWelding.Welding.Touch_PoC.Types;

namespace ValkWelding.Welding.Touch_PoC.DistanceDetectors
{
    public class TouchDetector : IDistanceDetector
    {
        public bool Connected { get; private set; }

        private readonly SerialPort _serialPort;
        private readonly Dictionary<DetectorCommand, DetectorResponse> _dataQueue;

        public TouchDetector(IOptions<LocalConfig> configuration)
        {
            _serialPort = new()
            {
                PortName = configuration.Value.DistanceDetectorSettings.ComPort,
                BaudRate = configuration.Value.DistanceDetectorSettings.BaudRate
            };
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedEvent);

            Connected = false;
            _dataQueue = new();
        }

        private void DataReceivedEvent(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = (SerialPort)sender;
            int _bytesToRead = serialPort.BytesToRead;
            byte[] recvData = new byte[_bytesToRead];
            serialPort.Read(recvData, 0, _bytesToRead);

            Connected = true;
            byte lastByte = recvData.Last();
            if ((lastByte & (byte)DetectorResponse.Succes) != 0)
            {
                if (_dataQueue.ContainsKey(DetectorCommand.Heartbeat))
                    _dataQueue[DetectorCommand.Heartbeat] = DetectorResponse.Succes;

                if (_dataQueue.ContainsKey(DetectorCommand.StartDetecting))
                    _dataQueue[DetectorCommand.StartDetecting] = DetectorResponse.Succes;
            }
            if ((lastByte & (byte)DetectorResponse.ObjectDetected) != 0)
            {
                _dataQueue[DetectorCommand.RequestObjectDetected] = DetectorResponse.ObjectDetected;
            }
            if ((lastByte & (byte)DetectorResponse.ObjectNotDetected) != 0)
            {
                _dataQueue[DetectorCommand.RequestObjectDetected] = DetectorResponse.ObjectNotDetected;
            }
        }

        public void Start()
        {
            _serialPort.Open();
            SendCommand(DetectorCommand.Heartbeat);
        }

        public DetectorResponse SendCommand(DetectorCommand command)
        {
            _dataQueue[command] = DetectorResponse.UNKNOWN;
            SendToSensor(command);
            while (_dataQueue[command] == DetectorResponse.UNKNOWN)
            {
                SendToSensor(command);
                Thread.Sleep(100);
            }
            return _dataQueue[command];
        }

        private void SendToSensor(DetectorCommand command)
        {
            byte[] commandArray = { (byte)command };
            _serialPort.Write(commandArray, 0, 1);
        }
    }
}
