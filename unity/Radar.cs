using UnityEngine;

public class Radar : MonoBehaviour {
    // 敵検出範囲
    public float detectionRange = 100;

    private GameObject player;
    private GameObject enemy;
    private GameObject icon;
    private float radarRadius = 0;

    // Use this for initialization
    private void Start() {
        // 各オブジェクトを取得
        player = GameObject.FindGameObjectWithTag("Player");
        enemy = GameObject.FindGameObjectWithTag("Enemy");
        icon = GameObject.FindGameObjectWithTag("Icon");

        // アイコンを非表示
        icon.SetActive(false);
        // レーダー半径を取得
        radarRadius = GetComponent<RectTransform>().sizeDelta.x / 2.0f;
    }

    // Update is called once per frame
    private void Update() {
        // アイコンを非表示
        icon.SetActive(false);

        // アイコンの位置を計算して範囲内なら表示
        if (Vector3.Distance(player.transform.position, enemy.transform.position) < detectionRange) {
            icon.SetActive(true);
            // 相対位置を計算
            Vector3 pos = (enemy.transform.position - player.transform.position) * (radarRadius / detectionRange);
            // カメラ角度に合わせて回転
            pos = Quaternion.Euler(0, -Camera.main.gameObject.transform.eulerAngles.y, 0) * pos;
            // 相対位置をレーダに反映
            icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos.x, pos.z);
        }
    }
}