```python
import cv2
import time
import imutils
import numpy as np 
import os
import math
import socket

UDP_IP = '127.0.0.1'
UDP_PORT = 5959

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

#img = cv2.imread('lena.png')

thres = 0.45 # Threshold to detect object

FirstSum = 0
SecondSum = 0
ThirdSum = 0
FourthSum = 0

cap = cv2.VideoCapture("http://192.168.0.8:8090/?action=stream")
cap.set(3,1280)
cap.set(4,720)
cap.set(10,70)

classNames= []
classFile = 'coco.names'
with open(classFile,'rt') as f:
    classNames = f.read().rstrip('\n').split('\n')

configPath = 'ssd_mobilenet_v3_large_coco_2020_01_14.pbtxt'
weightsPath = 'frozen_inference_graph.pb'

net = cv2.dnn_DetectionModel(weightsPath,configPath)

net.setInputSize(320,320)

net.setInputScale(1.0/ 127.5)

net.setInputMean((127.5, 127.5, 127.5))

net.setInputSwapRB(True)

while True:
    success,img = cap.read()
    cv2.normalize(img, img, 0, 255, cv2.NORM_MINMAX)
    classIds, confs, bbox = net.detect(img,confThreshold=thres)
    
    if len(classIds) != 0:
        for classId, confidence,box in zip(classIds.flatten(),confs.flatten(),bbox):
            if classId == 77: # What Do You Want Object
                cv2.rectangle(img,box,color=(0,255,0),thickness=2)
                
                cv2.putText(img,classNames[classId-1].upper(),(box[0]+10,box[1]+30),
                            cv2.FONT_HERSHEY_COMPLEX,1,(0,255,0),2)

                cv2.putText(img,str(round(confidence*100,2)),(box[0]+200,box[1]+30),
                            cv2.FONT_HERSHEY_COMPLEX,1,(0,255,0),2)
            
                x1 = box[0]
                y1 = box[1]
                x2 = box[2]
                y2 = box[3]
                
                FirstVertexPosition = (x1, y1)
                SecondVertexPosition = (x1 + x2, y1)
                ThirdVertexPosition = (x1 + x2, y2+ y1)
                FourthVertexPosition = (x1 , y2 + y1)
                # print(FirstVertexPosition[0], FirstVertexPosition[1])
                MiddleX = int(math.floor( ( FirstVertexPosition[0] + SecondVertexPosition[0] ) / 2))
                MiddleY = int(math.floor ( (SecondVertexPosition[1] + ThirdVertexPosition[1]) / 2))
                
                cv2.circle(img, (MiddleX, MiddleY), 30, (0,0,255), -1)

                b1 = bytes("xPos " + str(MiddleX) + " yPos " + str(MiddleY) + " 1", encoding='utf-8')
                sock.sendto(b1 ,(UDP_IP,UDP_PORT)) # 이 부분이 보내는거

    cv2.imshow("Output",img)
    cv2.waitKey(1)
```