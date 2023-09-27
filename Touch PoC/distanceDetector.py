import serial, numpy
import matplotlib.pyplot as plt
from command import Command

class DistanceDetector():
    def __init__(self, serialPort: str, baudrate: int = 9600, calculationPoints: int = 5, calculationInterval: int = 10):
        self.ser = serial.Serial(serialPort, baudrate, timeout=1)
        self.calculationPoints = calculationPoints
        self.calculationInterval = calculationInterval

    def run(self) -> [[], []]:  
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
                self.ser.write(Command.START_MOVING.value)
                receivedData = self.ser.read()
            
            while (self.ser.read() == Command.UNKNOWN.value):
                pass

            self.ser.write(Command.SEND_DISTANCE.value)
            movedSteps = self.ser.readline().decode()

            self.ser.write(Command.RECEIVED.value)
            return float(movedSteps)

        except KeyboardInterrupt:
            self.ser.close()

    def plotPoints(self, xCoordinates, yCoordinates):
        plt.plot(xCoordinates, yCoordinates)
        plt.xlabel("x (mm)")
        plt.ylabel("y (mm)")
        plt.show()