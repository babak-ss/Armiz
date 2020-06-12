﻿using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Armiz
{
    public class AttackState : State
    {
        public AttackState(GameController _gameController) : base(_gameController) { }

        public override IEnumerator Start()
        {
            Debug.Log("Attack State Started!");
            //gameController.ChangeUIState();
            gameController.changeStateBtn.color = Color.red;
            gameController.changeStateBtn.transform.GetChild(0).GetComponent<Text>().text = "STOP!";
            gameController.addOneBtn.interactable = false;
            gameController.upgradeBtn.interactable = false;

            yield break;
        }

        public override IEnumerator AlliesAttack()
        {
            Debug.Log("Allies Attack!");
            gameController.shootTimer = new TimerTool(Time.time, gameController.shootTime);
            for (int i = 0; i < gameController.allyControllers.Count; i++)
            {
                gameController.allyControllers[i].FireProjectile();
            }

            // TODO: enemies get damage when actually hit by a projectile
            if (gameController.enemy.Damage(GameData.AllyCount * gameController.ally.GetDamage()))
            {
                gameController.EnemyDied();
            }
            Debug.Log("Enemy Damaged! \nEnemy health:" + gameController.enemy.GetCurrentHealth());
            yield break;
        }

        public override IEnumerator EnemiesAttack()
        {
            Debug.Log("Enemies Attack!");
            yield break;
        }
    }
}