from cobot import CobotController
from distanceDetector.motorizedDetector import MotorizedDetector
from distanceDetector.testDetector import TestDetector
import sys

class Main():
    def __init__(self, controller: MotorizedDetector):
        self.controller = controller
    
    def start(self):
        print("Starting main")
        # self.controller.start()
    
    def run(self):
        try:
            xCoordinates, yCoordinates = self.controller.run()
            self.controller.plotPoints(xCoordinates, yCoordinates)
            # self.controller.moveToDirect([600, -250, 250], 100)
            # self.controller.moveToSteps([600, 0, 250], 100, True)
        
        except KeyboardInterrupt:
            pass
        except Exception as e:
            print(e)
        finally:
            # self.controller.stop()
            sys.exit()

if __name__ == "__main__":
    detector = MotorizedDetector("COM5", calculationPoints=4)
    # cobotController = CobotController(detector, 20)

    main = Main(detector)
    main.start()
    main.run()