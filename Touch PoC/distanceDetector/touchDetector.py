import serial, multiprocessing, time
from distanceDetector.detectorCommand import DetectorCommand

class TouchDetector(multiprocessing.Process):
    def __init__(self, serialPort: str, baudrate: int = 9600):
        super(TouchDetector, self).__init__()
        self.serialPort = serialPort
        self.baudrate = baudrate
        self.objectDetected = None

    def stop(self):
        pass
    #     self.ser.close()
    #     self.started = False
    #     print("Stopped detector")

    def run(self):
        print("Starting detector")
        self.ser = serial.Serial(self.serialPort, self.baudrate, timeout = 1)
        time.sleep(2)
        receivedData = self.ser.read()
        while (receivedData == b''):
            self.ser.write(DetectorCommand.START_DETECTING.value)
            receivedData = self.ser.read()

        while True:
            while self.ser.in_waiting > 0:
                read = self.ser.read() == b'\x01'
                print(read)