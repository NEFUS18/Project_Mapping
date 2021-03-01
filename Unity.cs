```csharp
using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class SocketClient : MonoBehaviour
{

   // Use this for initialization

   public Transform spawnPos;
   public GameObject hero;
   public GameObject[] heros = new GameObject[20];

   private float xPos = 10.0f;
   private float yPos = 10.0f;

   private int ObjectCount = 0;
   private int Count = 0;
   
   Thread receiveThread;
   UdpClient client;
   public int port;

   //info

   public string lastReceivedUDPPacket = "";
   public string allReceivedUDPPackets = "";

   void Start () {
      init();
   }

   void OnGUI(){  // 얘는 신경 안써도 댐
      Rect  rectObj=new Rect (40,10,200,400);
      
      GUIStyle  style  = new GUIStyle ();
      
      style .alignment  = TextAnchor.UpperLeft;
      
      GUI .Box (rectObj,"# UDPReceive\n127.0.0.1 "+port +" #\n"
                
                //+ "shell> nc -u 127.0.0.1 : "+port +" \n"
                
                + "\nLast Packet: \n"+ lastReceivedUDPPacket
                
                //+ "\n\nAll Messages: \n"+allReceivedUDPPackets
                
                + "\nObject Count : \n" + ObjectCount
         
                + "\nCount : \n" + Count
                
                ,style );

   }

   private void init(){   // udp 소켓 연결 부분
      print ("UPDSend.init()");

      port = 5959;

      print ("Sending to 127.0.0.1 : " + port);

      receiveThread = new Thread (new ThreadStart(ReceiveData));
      receiveThread.IsBackground = true;
      receiveThread.Start ();

   }

   private void ReceiveData(){    // 데이터 받아서 가공하는 곳
      client = new UdpClient (port);
      
      while (true) {
         try{
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            byte[] data = client.Receive(ref anyIP);

            string texts = Encoding.UTF8.GetString(data);  // 전체적인 데이터를 받아서 아래에서 자를거임
                                                // xPos 200 yPos 200 1 ( 현재 내 파이썬 코드는 이렇게만 보냄 )
                                                // 저기 제일 끝에 1은 오브젝트 아이디인데 임시로 고정값으로 넣어둠

            //print (">> " + texts);
            lastReceivedUDPPacket=texts;
            allReceivedUDPPackets=allReceivedUDPPackets+texts;

            string[] text = texts.Split(' ');
            
            // 여기서 하나씩 자른거 저장함
            string xPos_name = text[0];       // xPos     
            string xPos_value = text[1];   // 200
            string yPos_name = text[2];       // yPos
            string yPos_value = text[3];   // 200
            int hero_count = int.Parse(text[4]);   // 1
            
            
            if (xPos_name.Equals("xPos"))
            {
               xPos = float.Parse(xPos_value);
               xPos *= 0.021818f; // 얘는 모르겠음 아마 유니티에서 현실값과 동일하게 움직일 수 있도록 해주는 계산인듯         
            }
            if (yPos_name.Equals("yPos"))
            {
               yPos = float.Parse(yPos_value);
               yPos *= 0.021818f;
            }

            if (hero_count > 0)
            {
               ObjectCount = hero_count;  // hero는 객체이름 객체 이름을 받아와서
                                    // 이따 void Update에서 연산할 ObjectCount에 넣어줄꺼임
            }
            else
            {
               ObjectCount = 0;
            }
            
         }catch(Exception e){
            print (e.ToString());
         }
      }
   }

   public string getLatestUDPPacket(){    // 신경 안써도댐
      allReceivedUDPPackets = "";
      return lastReceivedUDPPacket;
   }
   
   // Update is called once per frame
   void Update ()
   {
      if (Count < ObjectCount)   // Count : 현재 총 생성된 객체 수, ObjectCount : 받아온 객체 수
      {
         Count++;
         heros[Count] = Instantiate(hero, new Vector3(0, 0, 0), Quaternion.identity); // Count에 맞게 객체 생성 시키는 부분
      }
      else if(Count > ObjectCount)
      {
         Count--;
         Destroy(heros[Count]); // Count에 맞게 객체 삭제하는 부분 ( 테스트는 못해봄 제대로 동작하는지 모름 )
      }
      
      heros[Count].transform.position = new Vector3(xPos - 6.0f, 0, yPos - 6.0f);    // Count에 맞게 객체를 움직이게 하는 부분
   }

   void OnApplicationQuit(){
      if (receiveThread != null) {
         receiveThread.Abort();
         Debug.Log(receiveThread.IsAlive); //must be false
      }
   }
}

```