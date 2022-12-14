using UnityEngine;

public enum LaunchObjectSpawnPosition { North, South, West, East }
public class SpawnManager : MonoBehaviour
{
    #region Variables

    [SerializeField] int amountLaunchObjectToSpawn;

    [SerializeField] private float enemySpawnTime;
    [SerializeField] private float powerupSpawnTime;
    [SerializeField] private float launchObjectSpawnTime;
    [SerializeField] private float spawnStartDelay;
    [SerializeField] private float powerupSpawningHeight;

    [SerializeField] private bool spawnEnemies;
    [SerializeField] private bool spawnPowerups;
    [SerializeField] private bool spawnLaunchObjects;

    [SerializeField] private GameObject[] enemies;
    [SerializeField] private GameObject[] powerups;
    [SerializeField] private GameObject[] launchObjects;
    [SerializeField] private GameObject[] souls;
    [SerializeField] private GameObject[] powerupVFX;
    [SerializeField] private Transform[] enemySpawnPositions;

    private Transform playerTransform;
    private Environment environment;

    #endregion

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        environment = FindObjectOfType<Environment>();
    }

    #region Spawning methods

    // Starts spawning enemies, powerups and launch objects in random places
    public void StartAllSpawns()
    {
        if (spawnEnemies)
        {
            InvokeRepeating("SpawnEnemy", spawnStartDelay, enemySpawnTime);
        }

        if (spawnPowerups)
        {
            InvokeRepeating("SpawnPowerup", spawnStartDelay, powerupSpawnTime);
        }

        if (spawnLaunchObjects)
        {
            InvokeRepeating("SpawnLaunchObject", spawnStartDelay, launchObjectSpawnTime);
        }
    }

    // Spawns an enemy in a random place near the limits
    void SpawnEnemy()
    {
        int enemyIndex = Random.Range(0, enemies.Length);
        int enemySpawnPosIndex = Random.Range(0, enemySpawnPositions.Length);
        Vector3 position = enemySpawnPositions[enemySpawnPosIndex].position;

        Instantiate(enemies[enemyIndex], position, enemies[enemyIndex].transform.rotation);
    }

    // Spawns a powerup in a random place
    void SpawnPowerup()
    {
        int powerupIndex = Random.Range(0, powerups.Length);
        Vector3 position = GenerateSpawningPowerupPosition(powerups[powerupIndex].gameObject);

        Instantiate(powerups[powerupIndex], position, powerups[powerupIndex].transform.rotation);
    }

    // Spawns a launch object in a random place near the limits
    void SpawnLaunchObject()
    {
        for (int amountLaunchObjectSpawned = 0; amountLaunchObjectSpawned < amountLaunchObjectToSpawn; amountLaunchObjectSpawned++)
        {
            int launchObjectIndex = Random.Range(0, launchObjects.Length);

            GameObject launchObject = launchObjects[launchObjectIndex];
            Vector3 position = GenerateSpawningLaunchObjectPosition();

            Instantiate(launchObject, position, launchObject.transform.rotation);
        }
    }

    // Spawns the corresponding soul of the enemy in the same position as the enemy
    public void SpawnEnemySoul(GameObject enemy)
    {
        const float differenceYPos = 0.2f;
        Vector3 position = new Vector3(enemy.transform.position.x, enemy.transform.position.y - differenceYPos, enemy.transform.position.z);
        GameObject soul = FindSoulByColor(enemy.GetComponent<EnemyController>().SoulColor);

        if (soul != null)
        {
            Instantiate(soul, position, soul.transform.rotation);
        }
        else
        {
            throw new System.Exception("No se encontro un alma con ese color");
        }
    }

    // Finds a soul in the soul array that has the same color as the given monster
    private GameObject FindSoulByColor(SoulColors soulColor)
    {
        foreach (GameObject soul in souls)
        {
            if (soul.GetComponent<Soul>().Color == soulColor)
            {
                return soul;
            }
        }

        return null;
    }

    // Spawns powerup visual effects
    public void SpawnPowerupVFX(Transform powerupPos)
    {
        foreach (GameObject visualEffect in powerupVFX)
        {
            Instantiate(visualEffect, powerupPos.position, visualEffect.transform.rotation);
        }
    }

    // Stop all spawnings
    public void StopAllSpawns()
    {
        CancelInvoke();
    }

    #endregion

    #region Generating position methods

    Vector3 GenerateSpawningPowerupPosition(GameObject powerup)
    {
        float xPosition = Random.Range(environment.LeftLimit.position.x, environment.RightLimit.position.x);
        float zPosition = Random.Range(environment.LowerLimit.position.z, environment.UpperLimit.position.z);
        
        Vector3 position = new Vector3(xPosition, environment.Ground.position.y + powerupSpawningHeight, zPosition);

        return position;
    }

    Vector3 GenerateSpawningLaunchObjectPosition()
    {
        // Generates a randomly place
        int enumPosition = Random.Range(0, 4);
        Vector3 position = new Vector3(0, playerTransform.transform.localScale.y, 0);

        // Depending on the place, a position is generated
        if ((LaunchObjectSpawnPosition)enumPosition == LaunchObjectSpawnPosition.West)
        {
            position.x = environment.LeftLimit.position.x;
            position.z = Random.Range(environment.LowerLimit.position.z, environment.UpperLimit.position.z);
        }

        else if ((LaunchObjectSpawnPosition)enumPosition == LaunchObjectSpawnPosition.North)
        {
            position.x = Random.Range(environment.LeftLimit.position.x, environment.RightLimit.position.x);
            position.z = environment.UpperLimit.position.z;
        }
        else if ((LaunchObjectSpawnPosition)enumPosition == LaunchObjectSpawnPosition.East)
        {
            position.x = environment.RightLimit.position.x;
            position.z = Random.Range(environment.LowerLimit.position.z, environment.UpperLimit.position.z);
        }
        else
        {
            position.x = Random.Range(environment.LeftLimit.position.x, environment.RightLimit.position.x);
            position.z = environment.LowerLimit.position.z;
        }        

        return position;
    }

    #endregion
}
