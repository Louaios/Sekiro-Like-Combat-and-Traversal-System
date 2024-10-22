using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public List<GameObject> enemies = new List<GameObject>();
    public int enemyCounter;
    void Start()
    {
        GameObject[] foundEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        enemies.AddRange(foundEnemies);

        enemyCounter = enemies.Count;
    }

   
    void Update()
    {
        SelectEnemyToAttack();
    }


    public void RemoveEnemy(GameObject enemy)
    {
        enemies.Remove(enemy);
        enemyCounter = enemies.Count;
        Debug.Log("Enemy removed. Remaining enemies: " + enemyCounter);
    }

    public void SelectEnemyToAttack()
    {
        if (enemies.Count > 0)
        {
            
            int randomIndex = Random.Range(0, enemies.Count);
            GameObject selectedEnemy = enemies[randomIndex];

            
            EnemyStateMachine enemyState = selectedEnemy.GetComponent<EnemyStateMachine>();
            //To be optimized

            if (enemyState != null)
            {
                enemyState.canAttack = true;
            }
            else
            {
                Debug.LogError("EnemyState script not found on " + selectedEnemy.name);
            }
        }
      
    }
}
