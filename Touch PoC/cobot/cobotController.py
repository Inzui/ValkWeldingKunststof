from distanceDetector.touchDetector import TouchDetector
from cobot.EasyModbusPy.cobotConnect import CobotConnect
import time

class CobotController():
    def __init__(self, detector: TouchDetector, stepSize: float = 5.0):
        self.detector = detector
        self.hasStarted = False
        self.stepSize = stepSize
        self.headPos = [-179, 0, -90]

    def start(self):
        print("Starting controller")
        # self.cob = CobotConnect()
        self.detector.start()
        self.hasStarted = True
    
    def stop(self):
        self.cob.stop()
        self.detector.stop()
        self.hasStarted = False
        print("Stopped controller")

    def detectObject(self) -> bool:
        return self.detector.objectDetected

    def moveToSteps(self, point: list, speed: int, stopOnDetection: bool = False):
        if (not self.hasStarted):
            raise Exception("Cobot has not been started.")

        point += self.headPos
        point = self._cleanPoint(point)
        P = self._cleanPoint(self.cob.readPos())

        while not self._arrivedOnPos(P, point):
            if (stopOnDetection and self.detector.objectDetected):
                return
            
            self._checkStatus()

            #-------------------------------------------------------#
            relativeMove = self._getRelativeMove(P, point)
            self.cob.sendCobotMove(relativeMove, speed)
            P = self._cleanPoint(self.cob.readPos())

    def moveToDirect(self, point: list, speed: int):
        if (not self.hasStarted):
            raise Exception("Cobot has not been started.")

        point += self.headPos
        point = self._cleanPoint(point)
        self.cob.sendCobotPos(point, speed) 

        P = self._cleanPoint(self.cob.readPos())

        while not self._arrivedOnPos(P, point):
            self._checkStatus()
            P = self._cleanPoint(self.cob.readPos())

    def _getRelativeMove(self, currentPos: list, desPos: list) -> list:
        newPos = [0, 0, 0, 0, 0, 0]
        for i in range(len(currentPos)):
            if(abs(currentPos[i] - desPos[i]) < self.stepSize):
                newPos[i] = currentPos[i] - desPos[i]
            elif(currentPos[i] < desPos[i]):
                newPos[i] = -self.stepSize
            elif(currentPos[i] > desPos[i]):
                newPos[i] = self.stepSize
        newPos[0], newPos[1] = newPos[1], newPos[0]
        return newPos

    def _arrivedOnPos(self, currentPos: list, desPos: list) -> bool:
        for i in range(len(currentPos)):
            if(currentPos[i] != desPos[i]):
                return False
        return True

    def _cleanPoint(self, point: list) -> list:
        for i in range(len(point)):
            point[i] = int(round(point[i], 0))
        return point
    
    def _checkStatus(self):
        statusCode, statusText = self.cob.readError()
        if(statusCode != 6):
            raise Exception(statusText)