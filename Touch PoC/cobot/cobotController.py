class CobotController():
    def __init__(self, detector):
        self.detector = detector

    def start(self):
        print("Starting controller")
        # Import here as it automatically tries to connect
        # from cobot.EasyModbusPy.cobotconnect19216801 import cobotconnect
        # self.cob = cobotconnect()
        self.detector.start()
    
    def stop(self):
        # self.cob.stop()
        self.detector.stop()
        print("Stopped controller")
    
    def detectObject(self):
        print(self.detector.detectObject())