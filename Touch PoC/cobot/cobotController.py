from cobot.EasyModbusPy.cobotconnect19216801 import cobotconnect

class CobotController():
    def __init__(self):
        pass

    def run(self):
        self.cob = cobotconnect()
    
    def stop(self):
        self.cob.stop()