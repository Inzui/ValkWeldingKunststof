from cobot import CobotController
from distanceDetector.touchDetector import TouchDetector
from distanceDetector.testDetector import TestDetector
import sys

class Main():
    def __init__(self, controller: CobotController):
        self.controller = controller
    
    def start(self):
        print("Starting main")
        self.controller.start()
    
    def run(self):
        try:
            while True:
                print(self.controller.detectObject())
            self.controller.moveToDirect([600, -250, 250], 100)
            self.controller.moveToSteps([600, 0, 250], 100, True)
        
        except KeyboardInterrupt:
            pass
        except Exception as e:
            print(e)
        finally:
            self.controller.stop()
            sys.exit()

if __name__ == "__main__":
    #detector = TouchDetector("COM3")
    detector = TestDetector()
    cobotController = CobotController(detector, 20)

    main = Main(cobotController)
    main.start()
    main.run()