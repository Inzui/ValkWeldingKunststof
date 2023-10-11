import keyboard
import threading

class TestDetector():
    def __init__(self):
        pass

    def start(self):
        print("Starting detector")

    def stop(self):
        print("Stopped detector")

    def detectObject(self) -> bool:
        return False

    