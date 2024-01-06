using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using ValkWelding.Welding.PolyTouchApplication.Configuration;
using ValkWelding.Welding.PolyTouchApplication.Types;
using System.Windows.Controls;

namespace ValkWelding.Welding.PolyTouchApplication.DistanceDetectors
{
    public class TouchDetector : IDistanceDetector
    {
        public bool Connected { 
            get 
            { 
                return _serialPort.IsOpen && SendCommand(DetectorCommand.Heartbeat) == DetectorResponse.Succes; 
            }
            private set { } 
        }

        private readonly SerialPort _serialPort;
        private readonly Dictionary<DetectorCommand, DetectorResponse> _dataQueue;

        public TouchDetector(IOptions<LocalConfig> configuration)
        {
            _serialPort = new()
            {
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

        public bool Connect(string comPort)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            _serialPort.PortName = comPort;
            _serialPort.Open();

            if (SendCommand(DetectorCommand.Heartbeat) == DetectorResponse.Succes)
            {
                return true;
            }
            else
            {
                _serialPort.Close();
                return false;
            }
        }

        public DetectorResponse SendCommand(DetectorCommand command)
        {
            _dataQueue[command] = DetectorResponse.UNKNOWN;
            SendToSensor(command);
            int tryCounter = 0;
            while (_dataQueue[command] == DetectorResponse.UNKNOWN && tryCounter <= 50)
            {
                SendToSensor(command);
                Thread.Sleep(100);
                tryCounter++;
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
