# 簡単に使い方を記載しておきます
## Unity部分
1. Unityをインストール
1. 新規のプロジェクトを作成
1. [こちら](https://qiita.com/syuhei/items/0e751a8dfaef941cf46b)を参考にunity3d_mqtt.unitypackageを取り込む
1. csとpingをAssetsに入れる
1. オブジェクトを2つ作って、それぞれPlayer,Enemyのタグをつける
1. UI->RawImageを作る
    1. Rader.csを割り当てる
    1. Textureにradar.pngを割り当てる
1. 6で作ったオブジェクトの子供としてUI->RawImageを作る
    1. Iconのタグをつける
    1. Textureにicon.pngを割り当てる
1. 空のオブジェクトを作って、Mqtt.csを割り当てる
## Node-RED部分
1. [こちら](https://nodered.jp/docs/getting-started/installation)を参考にNode-RED環境を構築
1. [こちら](https://qiita.com/zuhito/items/d8ac0f602e0f09b02eac)を参考にノードを追加する
1. mqtt.jsonをインポートする
1. デプロイするとMQTTが使えるようになります
## OpenCV部分
1. OpenCV3とPython3をインストール(OS毎にやり方が異なります)
1. maze_capture.pyの設定を変更
    1. colorに検出する色の種類を設定
    1. AndroidのIP Webcamを使用する場合はwebcamをTrueに変更
    1. IP WebcamのURLをurlに設定
1. python3 maze_capture.pyで実行
1. カメラで四角い物体を移すと自動で解析します
