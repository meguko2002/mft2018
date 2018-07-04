using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

using System;

public class Mqtt : MonoBehaviour {
    private GameObject enemy;
    private Vector3 pos;
    private bool update = false;
    private MqttClient client;

    // Use this for initialization
    void Start () {
        // サーバに接続
        client = new MqttClient("localhost", 1883, false, null); 
        
        // 受信データを処理するコールバックを登録
        client.MqttMsgPublishReceived += client_MqttMsgPublishReceived; 

        // サーバへ接続
        string clientId = Guid.NewGuid().ToString(); 
        client.Connect(clientId); 
        
        // analogトピックからデータを受信する
        client.Subscribe(new string[] { "analog" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE }); 

        // 座標を移動する対象のオブジェクトを取得
        enemy = GameObject.FindGameObjectWithTag("Enemy");
        pos = enemy.transform.position;
    }

    void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        // 受信したデータを取り出す
        var str = System.Text.Encoding.UTF8.GetString(e.Message);
        // Debug.Log("Received: " + str);

        // 受信したデータを座標データに変換
        string[] arr = str.Split(':');
        pos.x = int.Parse(arr[0]);
        pos.z = int.Parse(arr[1]);
        update = true;
    } 

    // MQTT送信用のテストコードなので、あとで消す
    void OnGUI(){
        if ( GUI.Button (new Rect (20,40,80,20), "Level 1")) {
            Debug.Log("sending...");
            client.Publish("analog", System.Text.Encoding.UTF8.GetBytes("Sending from Unity3D!!!"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
            Debug.Log("sent");
        }
    }

    // Update is called once per frame
    void Update () {
        // 敵の位置を保持している座標データに置き換える
        if (update) {
            enemy.transform.position = pos;
            update = false;
        }
    }
}
