import cv2
import numpy as np

g_hsv = None
cam_id = 0

# 各色の位置を検出する
def color_pick(img, color):
    global g_hsv

    if color == 1:
        # HSV色空間に変換
        hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
        # 青色の検出
        hsv_min = np.array([80,20,180])
        hsv_max = np.array([150,255,255])
        mask = cv2.inRange(hsv, hsv_min, hsv_max)
    elif color == 2:
        # HSV色空間に変換
        hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
        # 緑色の検出
        hsv_min = np.array([30,50,50])
        hsv_max = np.array([90,255,255])
        mask = cv2.inRange(hsv, hsv_min, hsv_max)
    elif color == 3:
        # HSV色空間に変換
        hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
        # 赤色の検出
        hsv_min = np.array([0,65,65])
        hsv_max = np.array([40,255,255])
        mask = cv2.inRange(hsv, hsv_min, hsv_max)
        hsv_min = np.array([160,65,65])
        hsv_max = np.array([180,255,255])
        mask += cv2.inRange(hsv, hsv_min, hsv_max)
    else:
        # グレースケール化
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        # 黒色の検出
        ret, mask = cv2.threshold(gray, 10, 255, cv2.THRESH_BINARY_INV)
        hsv = None
    # 輪郭抽出
    image, contours, hierarchy = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
    if hsv is not None:
        g_hsv = cv2.hconcat(cv2.split(hsv))
    # 最大の領域を選定
    max_area = 0
    best_cnt = None
    for cnt in contours:
        epsilon = 0.01 * cv2.arcLength(cnt, True)
        tmp = cv2.approxPolyDP(cnt, epsilon, True)
        area = cv2.contourArea(tmp)
        if max_area < area:
            best_cnt = cnt
            max_area = area
    # 対象が見つかったか判定
    if best_cnt is None:
        # print("color pick failed.")
        return None, None, None
    # 領域の重心を計算
    try:
        M = cv2.moments(best_cnt)
        cx = int(M['m10']/M['m00'])
        cy = int(M['m01']/M['m00'])
    except ZeroDivisionError:
        # たまにゼロ割になってしまうケースが有るので対処
        print("ZeroDivisionError!!")
        return None, None, None
    # 検出した領域を表示
    cv2.drawContours(img, [best_cnt], -1, (0, 255, 0), 3)
    return mask, cx, cy

cap = cv2.VideoCapture(cam_id)
while(1):
    ret, img = cap.read()
    mask, cx, cy = color_pick(img, 1)
    cv2.imshow("img", cv2.resize(img, (320, 180)))
    if mask is not None:
        cv2.imshow("mask", cv2.resize(mask, (320, 180)))
    if g_hsv is not None:
        cv2.imshow("hsv", cv2.resize(g_hsv, (960, 180)))
    # ESCキーでプログラムを終了
    if cv2.waitKey(50) == 27:
        break
cv2.destroyAllWindows()
cap.release()
