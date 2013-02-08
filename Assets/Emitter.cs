using UnityEngine;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

// http://stackoverflow.com/questions/794249/c-sharp-threading-and-queues

public class Emitter : MonoBehaviour {
	public Transform CharacterTranform;
	protected static Vector3 CharacterPos;
	protected static Boolean running;
	protected static string rawEntityCoords = "";
	public Transform preFactBot;

	// Use this for initialization
	void Start () {
		Debug.Log ("Starting emitter...");
		CharacterPos = CharacterTranform.position;
		running = true;
		Thread t = new Thread(sendData);
		t.Start();
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log (CharacterTranform.position);
		CharacterPos = CharacterTranform.position;
		
		ProcessData(rawEntityCoords);
	}
	
	static void sendData ()
	{
		//Debug.Log ("sendData");
		try
		{
		    Int32 port = 7000;
			TcpClient client = new TcpClient("localhost", port);
			NetworkStream stream = client.GetStream();
			while (running)
			{
				//Debug.Log ("sending data...");
				//Debug.Log (CharacterPos);

			    // Translate the passed message into ASCII and store it as a Byte array.
				//String message = CharacterPos.x.ToString() + "," + CharacterPos.y.ToString() + "," + CharacterPos.z.ToString();
				String message = CharacterPos.x.ToString() + "," + CharacterPos.z.ToString();
			    Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);			
			    // Send the message to the connected TcpServer.
				stream.Write(data, 0, data.Length);
				// Receive the TcpServer.response. 			
			    // Buffer to store the response bytes.
				data = new Byte[2048];			
			    // String to store the response ASCII representation.
				String responseData = String.Empty;			
			    // Read the first batch of the TcpServer response bytes.
				Int32 bytes = stream.Read(data, 0, data.Length);
				responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
				//Debug.Log("received: " + responseData);

				rawEntityCoords = responseData;
				
				Thread.Sleep(10);
			}
			// Close everything.
			stream.Close();
			client.Close();  
		}
		catch (Exception ex)
		{
			Debug.Log("Failed To Connect to the Server: " + ex.Message);
		}
	}
	
	void ProcessData(string responseData)
	{
		//Debug.Log ("r: " + responseData);
		if(responseData.Length == 0) return;
		if(responseData == "null") return;
		
		// #Ref<0.0.0.1728>=66.94,41.21|#Ref<0.0.0.1778>=15.94,47.10|#Ref<0.0.0.1833>=68.32,58.76|#Ref<0.0.0.1680>=1.20,3.40
		Array coords = responseData.Split('|');
		foreach (string coord in coords) 
		{
			Array element = coord.Split('=');
			string entityId = element.GetValue(0).ToString();
			Array entityCoords = element.GetValue(1).ToString().Split(',');
			float entityCoordsX = float.Parse (entityCoords.GetValue(0).ToString());
			float entityCoordsZ = float.Parse (entityCoords.GetValue(1).ToString());
			float entityVectorX = float.Parse (entityCoords.GetValue(2).ToString());
			float entityVectorZ = float.Parse (entityCoords.GetValue(3).ToString());
			//Debug.Log ("entityId: " + entityId + "; X: " + entityCoords.GetValue(0).ToString() + "; Z: " + entityCoords.GetValue(1).ToString());
			
			// create a cube if not exists
			GameObject obj = GameObject.Find(entityId);
			if (obj == null) createCube(entityId, entityCoordsX, entityCoordsZ, entityVectorX, entityVectorZ);
			// move if exists
			else 
			{
				obj.transform.position = new Vector3 (entityCoordsX, (float)0.6, entityCoordsZ);
				//obj.transform.rotation = new Quaternion(entityVectorX, 0, entityVectorZ, 0);
				//obj.transform.rotation = Quaternion.FromToRotation(new Vector3(0, 0, 0), new Vector3(entityVectorX, 0, entityVectorZ));
				obj.transform.rotation = Quaternion.LookRotation(new Vector3(entityVectorX, 0, entityVectorZ));
			}
		}
	}
	
	void createCube(string entityId, float entityCoordsX, float entityCoordsZ, float entityVectorX, float entityVectorZ)
	{
		//GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		//cube.name = entityId;
		//cube.transform.position = new Vector3 (entityCoordsX, (float)0.6, entityCoordsZ);
		//cube.transform.localScale = new Vector3 (1, 1, 1);
		
		UnityEngine.Object bot = Instantiate(preFactBot, new Vector3 (entityCoordsX, (float)0.6, entityCoordsZ), Quaternion.LookRotation(new Vector3(entityVectorX, 0, entityVectorZ)));
		bot.name = entityId;		
	}
	
	void OnDestroy()
	{
		Debug.Log("Stopping...");
		running = false;
	}

}
