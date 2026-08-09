using UnityEngine;

public class CharacterDataManager : MonoBehaviour
{
    public static CharacterDataManager Instance;
    public CharacterData data = new CharacterData();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (data == null)
                data = new CharacterData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
