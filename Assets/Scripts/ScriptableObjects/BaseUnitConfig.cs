using UnityEngine;

[CreateAssetMenu(fileName = "GeckoConfig", menuName = "Scriptable Objects/GeckoConfig")]
public class BaseUnitConfig : ScriptableObject
{

    [SerializeField] private int _baseHealth;
    public int BaseHealth => _baseHealth;

    [SerializeField] private int _baseDamage;
    public int BaseDamage => _baseDamage;

    [SerializeField] private int _moveSpeed;
    public int MoveSpeed => _moveSpeed;

    [SerializeField] private string _name;
    public string Name => _name;

    public void Move(Transform transform, int speed, Vector3 direction)
    {
        transform.position += direction * speed * Time.deltaTime;  
    }

}
