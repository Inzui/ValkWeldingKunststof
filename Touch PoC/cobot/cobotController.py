from cobot.EasyModbusPy.cobotConnect import CobotConnect

class CobotController():
    def __init__(self, detector, stepSize = 5):
        self.detector = detector
        self.hasStarted = False
        self.stepSize = stepSize
        self.headPos = [-179, 0, -90]

    def start(self):
        print("Starting controller")
        #Import here as it automatically tries to connect
    
        self.cob = CobotConnect()
        self.detector.start()
        self.hasStarted = True
    
    def stop(self):
        self.cob.stop()
        self.detector.stop()
        self.hasStarted = False
        print("Stopped controller")
    
    def detectObject(self):
        print(self.detector.detectObject())

    def moveToSteps(self, point: list, speed: int):
        if(not self.hasStarted):
            raise Exception("Cobot has not been started.")

        point += self.headPos
        point = self._cleanPoint(point)
        P = self._cleanPoint(self.cob.readPos())

        while not self._arrivedOnPos(P, point):
            self._checkStatus()

            #-------------------------------------------------------#
            relativeMove = self._getRelativeMove(P, point)
            self.cob.sendCobotMove(relativeMove, speed)
            P = self._cleanPoint(self.cob.readPos())

    def moveToDirect(self, point: list, speed: int):
        if(not self.hasStarted):
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