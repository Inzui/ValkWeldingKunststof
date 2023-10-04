from distanceDetector import DistanceDetector
from cobot import CobotController

class Main():
    def __init__(self, detectionService: DistanceDetector, cobotController: CobotController):
        self.detectionService = detectionService
        self.cobotController = cobotController
    
    def run(self):
        self.cobotController.run()
        # coordinates = self.detectionService.run()
        # self.detectionService.plotPoints(coordinates[0], coordinates[1])

if __name__ == "__main__":
    distanceDetector = DistanceDetector("COM3")
    cobotController = CobotController()

    main = Main(distanceDetector, cobotController)
    main.run()