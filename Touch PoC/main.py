from cobot import CobotController
from distanceDetector.touchDetector import TouchDetector

class Main():
    def __init__(self, controller):
        self.controller = controller
    
    def start(self):
        print("Starting main")
        self.controller.start()
    
    def run(self):
        try:
            pass
        
        except KeyboardInterrupt:
            self.controller.stop()
        except Exception as e:
            self.controller.stop()
            print(e)

if __name__ == "__main__":
    detector = TouchDetector("COM3")
    cobotController = CobotController(detector)

    main = Main(cobotController)
    main.start()
    main.run()