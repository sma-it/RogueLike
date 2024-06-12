using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Actor : MonoBehaviour
{
    private AdamMilVisibility algorithm;

    [Header("FieldOfView")]
    public List<Vector3Int> FieldOfView = new List<Vector3Int>();
    public int FieldOfViewRange = 8;

    [Header("Powers")]
    [SerializeField] private int maxHitPoints;
    [SerializeField] private int hitPoints;
    [SerializeField] private int defense;
    [SerializeField] private int power;
    [SerializeField] private int level;
    [SerializeField] private int xp;
    [SerializeField] private int xpToNextLevel;

    public int MaxHitPoints { get => maxHitPoints; }
    public int HitPoints { get => hitPoints; }
    public int Defense { get => defense; }
    public int Power { get => power; }
    public int Level { get => level; }
    public int Xp { get => xp; }


    private void Start()
    {
        algorithm = new AdamMilVisibility();
        UpdateFieldOfView();

        if (GetComponent<Player>())
        {
            UIManager.Get.UpdateHealth(HitPoints, MaxHitPoints);
        }
    }

    public void Move(Vector3 direction)
    {
        if (MapManager.Get.IsWalkable(transform.position + direction))
        {
            transform.position += direction;
        }
    }

    public void UpdateFieldOfView()
    {
        var pos = MapManager.Get.FloorMap.WorldToCell(transform.position);

        FieldOfView.Clear();
        algorithm.Compute(pos, FieldOfViewRange, FieldOfView);

        if (GetComponent<Player>())
        {
            MapManager.Get.UpdateFogMap(FieldOfView);
        }
    }

    public void DoDamage(int hp, Actor attacker)
    {
        hitPoints -= hp;

        if (hitPoints < 0) hitPoints = 0;

        if (GetComponent<Player>())
        {
            UIManager.Get.UpdateHealth(hitPoints, MaxHitPoints);
        }

        if (hitPoints == 0)
        {
            Die();
            if (attacker.GetComponent<Player>())
            {
                attacker.AddXp(xp);
            }
        }
    }

    public void Heal(int hp)
    {
        int maxHealing = maxHitPoints - hitPoints;
        if (hp > maxHealing) hp = maxHealing;

        hitPoints += hp;

        if (GetComponent<Player>())
        {
            UIManager.Get.UpdateHealth(hitPoints, MaxHitPoints);
            UIManager.Get.AddMessage($"You are healed for {hp} hit points.", Color.green);
        }
    }

    public void AddXp(int xp)
    {
        this.xp += xp;
        if (this.xp >= xpToNextLevel)
        {
            level++;
            xpToNextLevel += (int)(xpToNextLevel * 1.2f);
            UIManager.Get.AddMessage("You've gained a level!", Color.yellow);
            maxHitPoints += 2;
            defense++;
            power++;
        }
        if (GetComponent<Player>())
        {
            UIManager.Get.UpdateXP(this.xp);
            UIManager.Get.UpdateLevel(level);
        }
    }

    private void Die()
    {
        if (GetComponent<Player>())
        {
            UIManager.Get.AddMessage("You died!", Color.red); //Red
            GameManager.Get.DeleteSaveGame();
        }
        else
        {
            UIManager.Get.AddMessage($"{name} is dead!", Color.green); //Light Orange
        }
        GameManager.Get.CreateGameObject("Dead", transform.position).name = $"Remains of {name}";
        GameManager.Get.RemoveEnemy(this);
        Destroy(gameObject);
    }

    public void SetFromGameData(GameData data)
    {
        xp = data.XP;
        xpToNextLevel = data.XpToNextLevel;
        level = data.Level;
        defense = data.Defense;
        power = data.Power;
        hitPoints = data.HitPoints;
        maxHitPoints = data.MaxHitPoints;

        UIManager.Get.UpdateXP(this.xp);
        UIManager.Get.UpdateLevel(level);
        UIManager.Get.UpdateHealth(hitPoints, MaxHitPoints);
    }

    public GameData GetGameData()
    {
        return new GameData
        {
            XP = xp,
            XpToNextLevel = xpToNextLevel,
            Level = level,
            Defense = defense,
            Power = power,
            HitPoints = hitPoints,
            MaxHitPoints = maxHitPoints
        };
    }
}
