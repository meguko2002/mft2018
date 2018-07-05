import cv2
import numpy as np
import time
import urllib
import urllib.request
import paho.mqtt.client as mqtt

# 設定
color = 3 # 検出する色を指定（1=青,2=緑,3=赤,0=黒）
# IP WebcamのURLを指定
webcam = True
url='http://192.168.43.146:8080/shot.jpg'

# 迷路を検出して台形補正する
def keystone_correction(img):
    # 大きさの計算
    size = img.shape[0] * img.shape[1]
    # グレースケール化
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    # 入力座標の選定
    best_white = 0
    best_rate = 0.0
    best_approx = []
    # スレッシュホールドを変えて対象が見つかるまでループする
    for white in range(20, 220, 50):
        # 二値化
        ret, th1 = cv2.threshold(gray, white, 255, cv2.THRESH_BINARY)
        # 輪郭抽出
        image, contours, hierarchy = cv2.findContours(th1, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
        # 角の数が4つ、面積が10%〜70%を満たすものを選定
        max_area = 0
        for cnt in contours:
            epsilon = 0.01 * cv2.arcLength(cnt, True)
            tmp = cv2.approxPolyDP(cnt, epsilon, True)
            if 4 == len(tmp):
                area = cv2.contourArea(tmp)
                if max_area < area and size * 0.1 <= area and area <= size * 0.7:
                    best_approx = tmp
                    max_area = area
        if 0 != max_area:
            best_rate = max_area / size * 100
            best_white = white
            break
    # 台形が見つかったか判定
    if 0 == best_white:
        # print("The analysis failed.")
        return None
    # 4つの頂点を並び替える
    points = sorted(best_approx[:,0,:], key=lambda x:x[1]) # yが小さいもの順に並び替え。
    top = sorted(points[:2], key=lambda x:x[0]) # 前半二つは四角形の上。xで並び替えると左右も分かる。
    bottom = sorted(points[2:], key=lambda x:x[0], reverse=True) # 後半二つは四角形の下。同じくxで並び替え。
    points = top + bottom # 分離した二つを再結合。
    # 台形補正を実行
    pts1 = np.float32(points)
    pts2 = np.float32([[0,0],[364,0],[364,257],[0,257]])
    M = cv2.getPerspectiveTransform(pts1,pts2)
    dst = cv2.warpPerspective(img,M,(364,257))
    # 結果出力
    # print("Best parameter: white={} (rate={})".format(best_white, best_rate))
    # 検出した矩形を表示
    cv2.drawContours(img, [best_approx], -1, (0, 255, 0), 3)
    return dst

# 各色の位置を検出する
def color_pick(img, color):
    # 大きさの計算
    size = img.shape[0] * img.shape[1]
    if color == 1:
        # HSV色空間に変換
        hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
        # 青色の検出
        hsv_min = np.array([90,70,70])
        hsv_max = np.array([110,255,255])
        mask = cv2.inRange(hsv, hsv_min, hsv_max)
    elif color == 2:
        # HSV色空間に変換
        hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
        # 緑色の検出
        hsv_min = np.array([50,50,50])
        hsv_max = np.array([80,255,255])
        mask = cv2.inRange(hsv, hsv_min, hsv_max)
    elif color == 3:
        # HSV色空間に変換
        hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
        # 赤色の検出
        hsv_min = np.array([0,100,30])
        hsv_max = np.array([5,255,255])
        mask = cv2.inRange(hsv, hsv_min, hsv_max)
    else:
        # グレースケール化
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        # 黒色の検出
        ret, mask = cv2.threshold(gray, 30, 255, cv2.THRESH_BINARY_INV)
    # 輪郭抽出
    image, contours, hierarchy = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
    # 面積が1%以上で最大の領域を選定
    max_area = 0
    best_cnt = []
    for cnt in contours:
        epsilon = 0.01 * cv2.arcLength(cnt, True)
        tmp = cv2.approxPolyDP(cnt, epsilon, True)
        area = cv2.contourArea(tmp)
        if max_area < area:
            best_cnt = cnt
            max_area = area
    # 対象が見つかったか判定
    if best_cnt == []:
        # print("color pick failed.")
        return None, None, None
    # 領域の重心を計算
    M = cv2.moments(best_cnt)
    cx = int(M['m10']/M['m00'])
    cy = int(M['m01']/M['m00'])
    # 検出した領域を表示
    cv2.drawContours(img, [best_cnt], -1, (0, 255, 0), 3)
    return mask, cx, cy

# 初期化
prev_t = 0
current_t = 0
cnt = 0

# カメラをキャプチャする
cap = cv2.VideoCapture(0) # 0はカメラのデバイス番号
# mqttの初期化
client = mqtt.Client()
client.connect('127.0.0.1', port=1883, keepalive=60)

while(1):
    if webcam:
        # Use urllib to get the image from the IP camera
        imgResp = urllib.request.urlopen(url)
        # Numpy to convert into a array
        imgNp = np.array(bytearray(imgResp.read()),dtype=np.uint8)    
        # Finally decode the array to OpenCV usable format ;) 
        frame = cv2.imdecode(imgNp,-1)
    else:
        # 動画を1フレーム読み込む
        ret, frame = cap.read()
    if frame is None:
        cv2.waitKey(1)
        continue
    # 迷路を検出して台形補正する
    dst = keystone_correction(frame)
    # 結果を表示
    cv2.imshow('frame', frame)
    if dst is not None:
        # 迷路の中から指定色の物体を検出する
        mask, cx, cy = color_pick(dst, color)
        # 結果を表示
        cv2.imshow('dst', dst)
        if cx is not None and cy is not None:
            # mqttで座標を送信
            client.publish('enemy', '%d:%d' % (cx,cy))
    # wait for ESC key to exit
    k = cv2.waitKey(1)
    if k == 27:
        break
    # FPSを計算する
    current_t = time.perf_counter()
    cnt += 1
    if current_t - prev_t > 1.0: 
        dt = current_t - prev_t
        prev_t = current_t
        fps = cnt / dt 
        cnt = 0
        print('fps = %.2f' % fps)

cap.release()
cv2.destroyAllWindows()