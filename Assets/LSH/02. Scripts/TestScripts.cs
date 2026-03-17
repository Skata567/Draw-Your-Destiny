using UnityEngine;

public class TestScripts : MonoBehaviour
{
    [SerializeField] HumanPool humanPool;
    // Update is called once per frame
    void Start()
    {
        humanPool = FindAnyObjectByType<HumanPool>();
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            humanPool.GetHuman();
        }
    }
}
