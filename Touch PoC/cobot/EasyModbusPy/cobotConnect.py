import socket
import time
import re
import cobot.EasyModbusPy.EasyModbusPy.easymodbus.modbusClient as mc #pip install easymodbus
import struct

class CobotConnect():
    def __init__(self, ipAddress = '192.168.0.1', port = 5890):
        self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.socket.connect((ipAddress, port))
        self.modbusclient = mc.ModbusClient(ipAddress, 502)   #zelfde IP-adres, poort 502
        self.modbusclient.connect()

    def sendCobotMove(self, point, speed = 100):
        command = "Move_PTP(\"TPP\"," + self.toText(point) + "," + str(speed) + ",200,0,false)"   # 500,100,true

        self.socket.send(self.packetSender(command).encode())

    def sendCobotPos(self, point, speed = 100):
        command = "PTP(\"CPP\"," + self.toText(point) + "," + str(speed) + ",200,0,false)"

        self.socket.send(self.packetSender(command).encode())

    def readPos(self):
        P=[0.0,0.0,0.0,0.0,0.0,0.0]
        inputRegisters = self.modbusclient.read_inputregisters(7001, 2)
        P[0]=self.floater(inputRegisters[1],inputRegisters[0]) #x
        inputRegisters = self.modbusclient.read_inputregisters(7003, 2)
        P[1]=self.floater(inputRegisters[1],inputRegisters[0]) #y
        inputRegisters = self.modbusclient.read_inputregisters(7005, 2)
        P[2]=self.floater(inputRegisters[1],inputRegisters[0])#z
        inputRegisters = self.modbusclient.read_inputregisters(7007, 2)
        P[3]=self.floater(inputRegisters[1],inputRegisters[0]) #a in grd
        inputRegisters = self.modbusclient.read_inputregisters(7009, 2)
        P[4]=self.floater(inputRegisters[1],inputRegisters[0]) #b in grd
        inputRegisters = self.modbusclient.read_inputregisters(7011, 2)
        P[5]=self.floater(inputRegisters[1],inputRegisters[0]) #c in grd

        return P

    def readError(self):
        inputRegisters = self.modbusclient.read_inputregisters(7332, 1)
        errCode=inputRegisters[0]
        #print("foutcode ", errCode)
        switcher = {
            1: "Solid Red, fatal error.",
            3: "Solid Blue, standby in Auto Mode.",
            4: "Flashing Blue, project running in Auto Mode.",
            5: "Solid Green, standby in Manual Mode.",
            6: "Flashing Green, project running in Manual Mode.",
            9: "Alternating Blue&Red, Auto Mode error.",
            10: "Alternating Green&Red, Manual Mode error.",
            15: "Light Blue, safe activation mode.",
            18: "Flashing Green(9), project pause in Manual Mode.",
            19: "Flashing Blue(9), project pause in Auto Mode."
        }
        strerror=switcher.get(errCode, "No error")
        return (errCode, strerror) 

    def chkStr(self,s):
        result = "TMSCT," + str(len(s)+2) + ",3," + s + ","
        return result
    
    def checksum(self,b):
        result = b[0]
        for i in range(1, len(b)):
            result = result ^ b[i]
        c=result

        if (c < 16):
            d=str(hex(c)).upper()
            d="0" +d[-1:]
        else:
            d=str(hex(c).upper())
            d=d[-2:]

        return d

    def packetSender(self,s):
        h=self.chkStr(s)
        b=bytearray(h,'utf-8')
        d=self.checksum(b)
        result="$" + h + "*" + d + "\r\n"
        return result   
    
    def floater(self,x,y):
        t = (x,y) #31544,16632 => 7.765041351318359, 56191,49724 => -47.214351654052734
        packed_string = struct.pack("HH", *t)
        unpacked_float = struct.unpack("f", packed_string)[0]
        return unpacked_float

    def toText(self, P: list) -> list:
        return str(P[0]) + " ,"  + str(P[1]) + " ,"  + str(P[2]) + " ,"  + str(P[3]) + " ,"  + str(P[4]) + " ,"  + str(P[5]) 
    
    def stop(self):
        self.socket.close() #detach()

