from distanceDetector import DistanceDetector

class Main():
    def __init__(self, detectionService: DistanceDetector):
        self.detectionService = detectionService
    
    def run(self):
        coordinates = self.detectionService.run()
        self.detectionService.plotPoints(coordinates[0], coordinates[1])

if __name__ == "__main__":
    main = Main(DistanceDetector("COM3"))
    main.run()