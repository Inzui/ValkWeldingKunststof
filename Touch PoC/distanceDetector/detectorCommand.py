from enum import Enum

class DetectorCommand(Enum):
    UNKNOWN = b'\x00'
    START_DETECTING = b'0x01'
    SEND_DETECTION_VALUE = b'0x02'
    RECEIVED = b'0x04'