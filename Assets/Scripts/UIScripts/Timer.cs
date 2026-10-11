using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] private float timer = 300f;

    [SerializeField] private TextMeshProUGUI textTimer;

    [SerializeField] private bool timeStarted = false;


    private void Awake()
    {
        timeStarted = true;
    }
    private void Update()
    {
        if (timeStarted == true)
        {
            timer -= Time.deltaTime;
        }

        int minutes = Mathf.FloorToInt(timer / 60F);
        int seconds = Mathf.FloorToInt(timer - minutes * 60);

        textTimer.text = string.Format("{0:0}:{1:00}", minutes, seconds);
    }
}
