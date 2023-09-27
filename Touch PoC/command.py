from enum import Enum

class Command(Enum):
    UNKNOWN = b'\x00'
    START_MOVING = b'0x01'
    SEND_DISTANCE = b'0x02'
    RECEIVED = b'0x04'