import serial
from distanceDetector.detectorCommand import DetectorCommand

class TouchDetector():
    def __init__(self, serialPort: str, baudrate: int = 9600):
        self.serialPort = serialPort
        self.baudrate = baudrate

    def start(self):
        print("Starting detector")
        self.ser = serial.Serial(self.serialPort, self.baudrate, timeout=1)
        receivedData = self.ser.read()
        while (receivedData == b''):
            self.ser.write(DetectorCommand.START_DETECTING.value)
            receivedData = self.ser.read()
    
    def stop(self):
        self.ser.close()
        print("Stopped detector")

    def detectObject(self) -> bool:
        return self.ser.read() == b'\x01'