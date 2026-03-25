using UnityEngine;

// ============================================================
// Singleton<T>
// MonoBehaviour 기반 싱글톤 베이스 클래스.
// 씬 내에 같은 타입의 인스턴스가 2개 이상 생기면 나중 것을 자동 제거한다.
// 씬 전환 시 파괴됨 — 씬이 바뀌어도 유지해야 한다면 PersistentSingleton 사용.
//
// 사용법:
//   public class MyManager : Singleton<MyManager> { ... }
//   MyManager.Instance.DoSomething();
// ============================================================
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null)
        {
            // 이미 인스턴스가 존재하면 중복 오브젝트 제거
            Destroy(gameObject);
            return;
        }
        Instance = this as T;
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}

// ============================================================
// PersistentSingleton<T>
// Singleton을 상속하되 씬 전환 후에도 파괴되지 않는 버전.
// GameManager처럼 게임 내내 살아있어야 하는 매니저에 사용.
// ============================================================
public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
            DontDestroyOnLoad(gameObject);
    }
}
