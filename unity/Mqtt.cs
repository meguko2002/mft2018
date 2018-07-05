using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

using System;

public class Mqtt : MonoBehaviour {
    // MQTTによる座標移動を行う
    public bool update = true;

    private GameObject player;
    private float timeleft;

    private GameObject enemy;
    private Vector3 pos;
    private MqttClient client;

    // Use this for initialization
    void Start () {
        // サーバに接続
        client = new MqttClient("localhost", 1883, false, null); 
        
        // 受信データを処理するコールバックを登録
        client.MqttMsgPublishReceived += client_MqttMsgPublishReceived; 

        // サーバへ接続
        string clientId = Guid.NewGuid().ToString();
        int ret = client.Connect(clientId);
        if (ret == MqttMsgConnack.CONN_ACCEPTED) {
            // analogトピックからデータを受信する
            client.Subscribe(new string[] { "enemy" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE }); 
        } else {
            // サーバへの接続に失敗したので座標移動をしないように変更
            update = false;
        }

        // 座標を移動する対象のオブジェクトを取得
        player = GameObject.FindGameObjectWithTag("Player");
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

    // Update is called once per frame
    void Update () {
        if (update) {
            // 敵の位置を移動先まで移動する
            enemy.transform.position = Vector3.Lerp(enemy.transform.position, pos, Time.deltaTime * 10);

            // 0.1秒ごとに処理する
            timeleft -= Time.deltaTime;
            if (timeleft <= 0.0) {
                timeleft = 0.1f;
                // プレイヤーの位置情報を送信する
                Vector3 p = player.transform.position;
                client.Publish("player", System.Text.Encoding.UTF8.GetBytes(String.Format("{0}:{1}", (int)p.x, (int)p.z)), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
            }
        }
    }
}
