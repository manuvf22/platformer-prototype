using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [HideInInspector] public int deaths = 0;
    [HideInInspector] public int coins = 0;
    [HideInInspector] public int materials = 0;
    [HideInInspector] public int structuresBuilt = 0;
    [HideInInspector] public float timeElapsed = 0f;

    private bool levelActive = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (levelActive) timeElapsed += Time.deltaTime;
    }

    public void StartLevel() => levelActive = true;
    public void StopLevel() => levelActive = false;

    public void AddCoin(int amount) => coins += amount;
    public void AddMaterial(int amount) => materials += amount;
    public void AddDeath() => deaths++;
    public void AddStructure() => structuresBuilt++;

    public bool SpendMaterials(int amount)
    {
        if (materials >= amount) { materials -= amount; return true; }
        return false;
    }
}