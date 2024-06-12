using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public static GameManager Get { get => instance; }

    private Actor player;
    public Actor Player
    {
        get { return player; }
        set
        {
            player = value;
            var data = LoadGameData();
            if (data != null)
            {
                player.SetFromGameData(data);
                Debug.Log("previous game loaded");
            }
        }
    }

    public List<Actor> Enemies = new List<Actor>();
    public List<Consumable> Items = new List<Consumable>();
    public List<Ladder> Ladders = new List<Ladder>();
    public List<TombStone> TombStones = new List<TombStone>();

    private GameData gameData;
    private string filePath;

    private void Start()
    {
        filePath = Application.persistentDataPath + "/gamedata.json";
    }

    private void OnDestroy()
    {
        if (Player != null)
        {
            SaveGameData();
            Debug.Log("Player data saved");
        } else
        {
            Debug.Log("Cannot save game: no player found");
        }
    }

    public void ClearFloor()
    {
        foreach(var enemy in Enemies)
        {
            Destroy(enemy.gameObject);
        }
        foreach(var item in Items)
        {
            Destroy(item.gameObject);
        }
        foreach(var ladder in Ladders)
        {
            Destroy(ladder.gameObject);
        }
        foreach(var stone in TombStones)
        {
            Destroy(stone.gameObject);
        }
        Enemies.Clear();
        Items.Clear();
        Ladders.Clear();
        TombStones.Clear();
    }

    public GameObject CreateGameObject(string name, Vector2 position)
    {
        GameObject actor = Instantiate(Resources.Load<GameObject>($"Prefabs/{name}"), new Vector3(position.x + 0.5f, position.y + 0.5f, 0), Quaternion.identity);
        actor.name = name;
        return actor;
    }

    public void AddItem(Consumable item)
    {
        Items.Add(item);
    }

    public void RemoveItem(Consumable item)
    {
        if (Items.Contains(item))
        {
            Items.Remove(item);
        }
    }

    public void AddEnemy(Actor enemy)
    {
        Enemies.Add(enemy);
        UIManager.Get.Floor.SetEnemies(Enemies.Count);
    }

    public void RemoveEnemy(Actor enemy)
    {
        if (Enemies.Contains(enemy))
        {
            Enemies.Remove(enemy);
        }
        UIManager.Get.Floor.SetEnemies(Enemies.Count);
    }

    public void AddLadder(Ladder ladder)
    {
        Ladders.Add(ladder);
    }

    public void AddTombStone(TombStone stone)
    {
        TombStones.Add(stone);
    }

    public void StartEnemyTurn()
    {
        foreach(var enemy in Enemies)
        {
            enemy.GetComponent<Enemy>().RunAI();
        }
    }

    public Actor GetActorAtLocation(Vector3 location)
    {
        if (Player.transform.position == location)
        {
            return Player;
        } else
        {
            foreach(Actor enemy in Enemies)
            {
                if (enemy.transform.position == location)
                {
                    return enemy;
                }
            }
        }
        return null;
    }

    public List<Actor> GetNearbyEnemies(Vector3 location)
    {
        var result = new List<Actor>();
        foreach(Actor enemy in Enemies)
        {
            if (Vector3.Distance(enemy.transform.position, location) < 5)
            {
                result.Add(enemy);
            }
        }
        return result;
    }

    public Consumable GetItemAtLocation(Vector3 location)
    {
        foreach(var item in Items)
        {
            if (item.transform.position == location)
            {
                return item;
            }
        }
        return null;
    }

    public Ladder GetLadderAtLocation(Vector3 location)
    {
        foreach(var ladder in Ladders)
        {
            if (ladder.transform.position == location)
            {
                return ladder;
            }
        }
        return null;
    }

    public GameData LoadGameData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            var data = JsonUtility.FromJson<GameData>(json);
            return data;
        }
        return null;
    }

    public void SaveGameData()
    {
        var data = Player.GetGameData();
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
    }

    public void DeleteSaveGame()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
