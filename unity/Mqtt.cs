using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

using System;

public class Mqtt : MonoBehaviour {
    // MQTTによる座標移動を行う
    public bool update = true;

    private GameObject player;
    private float timeleft = 0.0f;

    private GameObject enemy;
    private Vector3 pos;
    private MqttClient client;
    private float move_timeleft = 0.0f;

    private int size_x = 8;
    private int size_y = 12;
    protected Animator animator;

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
            // 開始用のコードを送信
            client.Publish("player", System.Text.Encoding.UTF8.GetBytes("200"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
        } else {
            // サーバへの接続に失敗したので座標移動をしないように変更
            update = false;
        }

        // 座標を移動する対象のオブジェクトを取得
        player = GameObject.FindGameObjectWithTag("Player");
        enemy = GameObject.FindGameObjectWithTag("Enemy");
        pos = enemy.transform.position;

        // Animatorオブジェクトを取得
        animator = enemy.GetComponent<Animator>();
    }

    void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e) {
        // 受信したデータを取り出す
        var str = System.Text.Encoding.UTF8.GetString(e.Message);
        // Debug.Log("Received: " + str);

        // 受信したデータを座標データに変換
        string[] arr = str.Split(':');
        pos.x = (float.Parse(arr[1]) / 257 * size_x) - (size_x/2);
        pos.y = 0.5f;
        pos.z = (float.Parse(arr[0]) / 364 * size_y) - (size_y/2);
    }

    // Update is called once per frame
    void Update () {
        if (update) {
            // 目的地と現在地の差分を計算
            Vector3 diff = pos - enemy.transform.position;
            // 距離が遠いか近いか判定
            if (diff.magnitude > 0.1f) {
                // 遠い場合は移動アニメーションに変更
                animator.SetBool("is_move", true);
                // なめらかな移動を計算する
                enemy.transform.position = Vector3.Lerp(enemy.transform.position, pos, Time.deltaTime * 10);
                // キャラクターの向きを変更
                enemy.transform.rotation = Quaternion.LookRotation(diff);
            } else {
                // ある程度近くなったら停止アニメーションに変更
                animator.SetBool("is_move", false);
                // 目的地に移動してしまう
                enemy.transform.position = pos;
            }

            // 0.05秒ごとに処理する
            timeleft -= Time.deltaTime;
            if (timeleft <= 0.0) {
                timeleft = 0.05f;
                // プレイヤーの位置情報を取得
                Vector3 p = player.transform.position;
                // LEDの座標に変換
                int x = Convert.ToInt32((p.x + (size_x/2)) / size_x * 9);
                int y = Convert.ToInt32((p.z + (size_y/2)) / size_y * 18);
                // 範囲外に行かないように補正
                if (x < 0) { x = 0; }
                else if (x > 9) { x = 9; }
                if (y < 0) { y = 0; }
                else if (x > 9) { x = 9; }
                // LED用のコードに変換
                string str;
                if (x % 2 == 1) {
                    str = Convert.ToString(x * 19 + 18 - y);
                } else {
                    str = Convert.ToString(x * 19 + y);
                }
                // LED用のコードを送信
                client.Publish("player", System.Text.Encoding.UTF8.GetBytes(str), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
            }

            // 勝敗決定時の処理
            if (Timer.time <= 1) {
                // 敗北用のコードを送信
                client.Publish("player", System.Text.Encoding.UTF8.GetBytes("201"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                // 更新フラグを寝かせて通信を無効化する
                update = false;
            } else if (Goal.goal) {
                // 勝利用のコードを送信
                client.Publish("player", System.Text.Encoding.UTF8.GetBytes("202"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                // 更新フラグを寝かせて通信を無効化する
                update = false;
            }
        }
    }
}
