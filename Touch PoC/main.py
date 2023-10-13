# from cobot import CobotController
# from distanceDetector.touchDetector import TouchDetector
# from distanceDetector.testDetector import TestDetector
# import sys

class Main():
    def __init__(self, controller):
        self.controller = controller
    
    def start(self):
        print("Starting main")
        self.controller.start()
    
    def run(self):
        try:
            self.controller.moveToDirect([600, 50, 250], 100)
            self.controller.moveToDirect([600, -250, 250], 100)

            for i in range(10):
                self.controller.moveToSteps([600, 0, 250], 100)
                self.controller.moveToSteps([600, -250, 500], 100)
                self.controller.moveToSteps([350, 0, 500], 100)
        
        except KeyboardInterrupt:
            self.controller.stop()
        except Exception as e:
            self.controller.stop()
            print(e)
        finally:
            self.controller.stop()
            sys.exit()

if __name__ == "__main__":
    #detector = TouchDetector("COM3")
    # detector = TestDetector()
    # cobotController = CobotController(detector, 20)

    # main = Main(cobotController)
    # main.start()
    # main.run()

    lst = (2,3)
    print(*lst)