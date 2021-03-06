﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Arimz;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Armiz
{
    public class GameController : StateMachine
    {
        private GameState gameState;

        [Header("Game Prameters")]
        public float alliesAttackTime;
        public float enemiesAttackTime;
        public Vector3 enemyPos;

        [Header("ScriptRefrences")]
        //[SerializeField] public List<EnemyController> enemyControllers;
        //[SerializeField] public List<AllyController> allyControllers;
        [SerializeField] public List<FighterController> allyFighterControllers;
        [SerializeField] public List<FighterController> enemyFighterControllers;
        private SaveLoadManager saveLoadManager;

        [Header("Editor Parameters")]
        //public GameObject uiPanel;
        public Image changeStateBtn;
        public Button addOneBtn;
        public Button upgradeBtn;
        public Text coinCountTxt;

        [Header("Prefabs")]
        public GameObject allyPrefab;
        public GameObject enemyPrefab;
        public GameObject allyBulletPrefab;
        public GameObject enemyBulletPrefab;

        [Header("Scriptable Objects")]
        public Fighter enemy;
        public Fighter ally;

        [HideInInspector] public TimerTool alliesAttackTimer;
        [HideInInspector] public TimerTool enemyAttackTimer;

        private FightersSaveData fightersSaveData;

        void Awake()
        {
            enemy = new Fighter();
            enemy.ResetFighter(Fighter.FighterType.Enemy);
            ally = new Fighter();
            ally.ResetFighter(Fighter.FighterType.Ally);

            saveLoadManager = new SaveLoadManager();
            fightersSaveData = saveLoadManager.LoadSavedData();
            if (fightersSaveData == null)
            {
                fightersSaveData = FightersSaveData.NullData();
            }

            gameState = GameState.Idle;
            SetState(new IdleState(this));
            SpawnNewAllies();
            SpawnEnemies();
        }

        void Start()
        {

        }

        void Update()
        {
            switch (gameState)
            {
                case GameState.Idle:
                    // upgrade allies!
                    break;
                case GameState.Attack:
                    //if (attackTimer.isFinished(Time.time))
                    //{
                    //    OnAttackTimerFinished();
                    //}
                    //else
                    //{
                    //    if (shootTimer.isFinished(Time.time))
                    //    {
                    //        shootTimer = new TimerTool(Time.time, shootTime);
                    //        StartCoroutine(state.AlliesAttack());
                    //    }

                    //    if (Input.GetMouseButtonDown(0))
                    //    {
                    //        StartCoroutine(state.AlliesAttack());
                    //    }
                    //}
                    if (enemyAttackTimer.isFinished(Time.time))
                    {
                        enemyAttackTimer = new TimerTool(Time.time, enemiesAttackTime);
                        StartCoroutine(state.EnemiesAttack());
                    }
                    if (alliesAttackTimer.isFinished(Time.time))
                    {
                        alliesAttackTimer = new TimerTool(Time.time, alliesAttackTime);
                        StartCoroutine(state.AlliesAttack());
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        StartCoroutine(state.AlliesAttack());
                    }

                    //if (Input.GetKey(KeyCode.F))
                    //    StartCoroutine(state.EnemiesAttack());
                    break;
                default:
                    break;
            }
        }

        private void SpawnNewAllies()
        {
            DespawnAllAllies();

            int _allyCount = fightersSaveData.alliesHealth.Count;
            GameData.AllyCount = _allyCount;

            if (_allyCount <= 0)
            {
                GameData.AllyCount = 1;
                _allyCount = 1;
            }

            float segmentDegree = 360 / _allyCount;
            int radius = (_allyCount < 20) ? 2 : 5;
            Vector3 pos = Vector3.zero;
            for (int i = 0; i < _allyCount; i++)
            {
                allyFighterControllers.Add(ObjectPool.Spawn(allyPrefab, Vector3.zero).GetComponent<FighterController>());
                allyFighterControllers[i].Initialize(this, ally, fightersSaveData.alliesHealth[i], allyBulletPrefab);
            }

            SetAlliesPositions(_allyCount);
        }
        private void SetAlliesPositions(int _allyCount)
        {
            float segmentDegree = 360 / _allyCount;
            int radius = (_allyCount < 20) ? 2 : 5;
            for (int i = 0; i < _allyCount; i++)
            {
                if (fightersSaveData.alliesPosition.Count > i)
                {
                    allyFighterControllers[i].SetPosition(new Vector3(fightersSaveData.alliesPosition[i].X, fightersSaveData.alliesPosition[i].Y, fightersSaveData.alliesPosition[i].Z));
                }
                else
                {
                    allyFighterControllers[i].SetPosition(new Vector3(enemyPos.x + Utility.rCos(radius, i * segmentDegree),
                                                                        enemyPos.y * allyPrefab.transform.localScale.y,
                                                                        enemyPos.z + Utility.rSin(radius, i * segmentDegree)));
                }
            }
        }
        private void DespawnAllAllies()
        {
            for (int i = 0; i < allyFighterControllers.Count; i++)
            {
                ObjectPool.Despawn(allyFighterControllers[i].gameObject);
            }
            allyFighterControllers = new List<FighterController>();
        }

        public void SpawnEnemies()
        {
            //TODO: spawn multiple Enemies!
            for (int i = 0; i < enemyFighterControllers.Count; i++)
            {
                ObjectPool.Despawn(enemyFighterControllers[i].gameObject);
            }
            enemyFighterControllers = new List<FighterController>();

            GameObject enemyGO = ObjectPool.Spawn(enemyPrefab, enemyPos);
            enemyFighterControllers.Add(enemyGO.GetComponent<FighterController>());
            enemyFighterControllers[enemyFighterControllers.Count - 1].Initialize(this, enemy, fightersSaveData.enemyHealth, enemyBulletPrefab);
            enemyFighterControllers[enemyFighterControllers.Count - 1].SetHealthBar();
            enemyFighterControllers[enemyFighterControllers.Count - 1].SetPosition(enemyPos);
        }
        public void EnemyDied()
        {
            Debug.Log("Enemy Died!");
            GameData.Coin += enemy.GetBountyValue();
            // enemy coin get animation
            SpawnEnemies();
        }

        #region OnClicks
        public void OnStateChangeBtnClick()
        {
            if (gameState == GameState.Idle)
            {
                gameState = GameState.Attack;
                SetState(new AttackState(this));
                alliesAttackTimer = new TimerTool(Time.time, alliesAttackTime);
                enemyAttackTimer = new TimerTool(Time.time, enemiesAttackTime);
                StartCoroutine(state.AlliesAttack());
            }
            else if (gameState == GameState.Attack)
            {
                gameState = GameState.Idle;
                SetState(new IdleState(this));
            }
        }
        public void OnAddAllyBtnClick()
        {
            if (GameData.Coin < ally.GetCost()) return;

            GameData.Coin -= ally.GetCost();
            GameData.AllyCount++;

            allyFighterControllers.Add(ObjectPool.Spawn(allyPrefab, Vector3.zero).GetComponent<FighterController>());
            allyFighterControllers[GameData.AllyCount - 1].Initialize(this, ally, ally.GetTotalHealth_Enemy(1), allyBulletPrefab);

            SetAlliesPositions(GameData.AllyCount);
        }
        public void OnLevelUpAllyBtnClick()
        {
            if (allyFighterControllers.Count <= 0) return;
            if (GameData.Coin < ally.GetUpgradeCost()) return;

            int minLevel = 10000;
            for (int i = 0; i < allyFighterControllers.Count; i++)
            {
                if (allyFighterControllers[i].GetLevel() < minLevel)
                {
                    minLevel = allyFighterControllers[i].GetLevel();
                }
            }

            List<FighterController> minLevelAllies = new List<FighterController>();
            for (int i = 0; i < allyFighterControllers.Count; i++)
            {
                if (allyFighterControllers[i].GetLevel() == minLevel)
                {
                    minLevelAllies.Add(allyFighterControllers[i]);
                }
            }

            var allyToLevelUp = minLevelAllies[UnityEngine.Random.Range(0, minLevelAllies.Count - 1)];
            allyToLevelUp.LevelUp();
        }
        public void OnResetProgressBtnClick()
        {
            GameData.AllyCount = 1;
            GameData.Coin = 500;
            enemy.ResetLevel();
            ally.ResetLevel();
            SceneManager.LoadScene(0);
        }
        public void OnAttackTimerFinished()
        {
            if (gameState != GameState.Attack) return;
            gameState = GameState.Idle;
            SetState(new IdleState(this));
        }
        #endregion

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                var data = new FightersSaveData();
                data.enemyLevel = enemy.GetLevel();
                data.enemyHealth = enemy.GetCurrentHealth();
                data.alliesLevel = new List<float>();
                data.alliesHealth = new List<float>();
                data.alliesPosition = new List<aVector3>();
                for (int i = 0; i < allyFighterControllers.Count; i++)
                {
                    data.alliesLevel.Add(allyFighterControllers[i].fighter.GetLevel());
                    data.alliesHealth.Add(allyFighterControllers[i].fighter.GetCurrentHealth());
                    data.alliesPosition.Add(new aVector3(allyFighterControllers[i].transform.position.x, allyFighterControllers[i].transform.position.y, allyFighterControllers[i].transform.position.z));
                }
                saveLoadManager.SaveThisData(data);
            }
        }
    }
}