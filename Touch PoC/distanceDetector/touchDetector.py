import serial, threading, time
from distanceDetector.detectorCommand import DetectorCommand

class TouchDetector(threading.Thread):
    def __init__(self, serialPort: str, baudrate: int = 9600):
        super(TouchDetector, self).__init__()
        self.serialPort = serialPort
        self.baudrate = baudrate
        self.objectDetected = None
        self.started = False
    
    def connect(self):
        print("Starting detector")
        self.ser = serial.Serial(self.serialPort, self.baudrate, timeout = 1)
        time.sleep(2)
        receivedData = self.ser.read()
        while (receivedData == b''):
            self.ser.write(DetectorCommand.START_DETECTING.value)
            receivedData = self.ser.read()
        self.started = True

    def stop(self):
        self.ser.close()
        self.started = False
        print("Stopped detector")

    def run(self):
        while self.started:
            while self.ser.in_waiting:
                self.objectDetected = self.ser.read() == b'\x01'
                print(self.objectDetected)