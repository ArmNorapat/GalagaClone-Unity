using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0;
    [SerializeField] private float startPosY = 0;
    [SerializeField] private float limitPosY = 0;
    [SerializeField] private Transform[] gameBg = null;

    private static BackgroundController Instance;

    /// <summary>
    /// Singleton for smooth ui when game is restart.
    /// </summary>
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        var bgVector = Vector3.down * moveSpeed * Time.deltaTime;

        foreach (Transform bg in gameBg)
        {
            if (bg == null)
            {
                continue;
            }

            bg.position += bgVector;

            if (bg.position.y < limitPosY)
            {
                var warpPos = new Vector2(bg.position.x, startPosY);
                bg.position = warpPos;
            }
        }
    }
}