using UnityEngine;

public class OxygenTimer : MonoBehaviour
{
    [SerializeField] private float startingTime = 5f;
    
    private float timeLeft;

    void Update()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
        }
        else
        {

        }
    }
}
