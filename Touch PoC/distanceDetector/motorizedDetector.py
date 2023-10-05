import serial, numpy
import matplotlib.pyplot as plt
from distanceDetector.detectorCommand import DetectorCommand

class MotorizedDetector():
    def __init__(self, serialPort: str, baudrate: int = 9600, calculationPoints: int = 5, calculationInterval: int = 10):
        self.serialPort = serialPort
        self.baudrate = baudrate
        self.calculationPoints = calculationPoints
        self.calculationInterval = calculationInterval

    def run(self) -> [[], []]:
        self.ser = serial.Serial(self.serialPort, self.baudrate, timeout=1)  
        xCoordinates, yCoordinates = [], []
        for i in range(self.calculationPoints):
            print("Give x in mm")
            x = int(input())
            y = self._detectObject()
            if (len(xCoordinates) == 0):
                xCoordinates.append(x)
                yCoordinates.append(y)

            elif (len(xCoordinates) >= 1):
                generatedXCoordinates = numpy.linspace(xCoordinates[-1], x, num = self.calculationInterval)
                generatedXCoordinates = numpy.delete(generatedXCoordinates, 0)
                xCoordinates.extend(generatedXCoordinates)

                generatedYCoordinates = numpy.linspace(yCoordinates[-1], y, num = self.calculationInterval)
                generatedYCoordinates = numpy.delete(generatedYCoordinates, 0)
                yCoordinates.extend(generatedYCoordinates)
        return [xCoordinates, yCoordinates]

    def _detectObject(self) -> float:
        print("Detecting object...")
        try:
            receivedData = self.ser.read()
            while (receivedData == b''):
                self.ser.write(DetectorCommand.START_DETECTING.value)
                receivedData = self.ser.read()
            
            while (self.ser.read() == DetectorCommand.UNKNOWN.value):
                pass

            self.ser.write(DetectorCommand.SEND_DETECTION_VALUE.value)
            movedSteps = self.ser.readline().decode()

            self.ser.write(DetectorCommand.RECEIVED.value)
            return float(movedSteps)

        except KeyboardInterrupt:
            self.ser.close()

    def plotPoints(self, xCoordinates, yCoordinates):
        plt.plot(xCoordinates, yCoordinates)
        plt.xlabel("x (mm)")
        plt.ylabel("y (mm)")
        plt.show()