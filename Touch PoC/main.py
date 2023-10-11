from cobot import CobotController
from distanceDetector.touchDetector import TouchDetector

class Main():
    def __init__(self, controller: CobotController):
        self.controller = controller
    
    def start(self):
        print("Starting main")
        self.controller.start()
    
    def run(self):
        try:
            # self.controller.moveToDirect([600, -250, 250], 100)

            # for i in range(10):
            #     self.controller.moveToSteps([600, 0, 250], 100)
            #     self.controller.moveToSteps([600, -250, 500], 100)
            #     self.controller.moveToSteps([350, 0, 500], 100)

            # self.controller.moveToSteps([600, 0, 250], 100, True)

            while (True):
                print(self.controller.detectObject())
        
        except KeyboardInterrupt:
            pass
        except Exception as e:
            print(e)
        finally:
            self.controller.stop()

if __name__ == "__main__":
    detector = TouchDetector("COM3")
    cobotController = CobotController(detector, 20)

    main = Main(cobotController)
    main.start()
    main.run()