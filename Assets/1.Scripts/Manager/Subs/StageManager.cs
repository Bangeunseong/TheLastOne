using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using UnityEngine;
using _1.Scripts.Map.Console;
using _1.Scripts.Map.Door;
using _1.Scripts.Manager.Core;
using _1.Scripts.Map;

namespace _1.Scripts.Manager.Subs
{
    [Serializable]
    public class StageManager : MonoBehaviour
    {
        public List<Stage> stages;
        public Stage CurrentStage => stages.Count > 0 ? stages[0] : null;
        private CoreManager coreManager;

        private void Awake()
        {
            coreManager  = CoreManager.Instance;
        }
        private void Start()
        {
            foreach (var stage in stages)
            {
                foreach (var door in stage.doors)
                {
                    foreach (var condition in door.conditions)
                    {
                        switch (condition.conditionType)
                        {
                            case StageDoorConditionType.ConsoleInteract:
                                if (condition.targetConsole != null)
                                {
                                    condition.targetConsole.OnInteractEvent += () => OnConsoleInteracted(condition, stage);
                                }
                                break;
                            /*case StageDoorConditionType.EnemyDefeat:
                                if (condition.targetEnemy != null)
                                {
                                    
                                }
                                break;*/
                            case StageDoorConditionType.LocationReached:
                                if (condition.targetLocation != null)
                                {
                                    var trigger = condition.targetLocation.GetComponent<LocationTrigger>();
                                    if (trigger != null)
                                    {
                                        trigger.OnEnter += () => OnPlayerReached(condition.targetLocation, stage);
                                    }
                                }
                                break;
                        }
                    }
                }

                foreach (var condition in stage.clearConditions)
                {
                    switch (condition.conditionType)
                    {
                        case StageDoorConditionType.ConsoleInteract:
                            if (condition.targetConsole != null)
                            {
                                condition.targetConsole.OnInteractEvent += () => CheckStageClear(condition, stage);
                            }
                            break;
                        /*case StageDoorConditionType.EnemyDefeat:
                            if (condition.TargetEnemy != null)
                            {
                                
                            }
                            break;*/
                        case StageDoorConditionType.LocationReached:
                            if (condition.targetLocation != null)
                            {
                                var trigger = condition.targetLocation.GetComponent<LocationTrigger>();
                                if (trigger != null)
                                {
                                    trigger.OnEnter += () => CheckStageClear(condition, stage);
                                }
                            }
                            break;
                    }
                }
            }
        }

        private void CheckStageClear(StageClearCondition condition, Stage stage)
        {
            condition.isConditionMet = true;
            if (stage.CheckStageCleared())
            {
                switch (stage.stageName)
                {
                    case "Stage1":
                        coreManager.sceneLoadManager.OpenScene(SceneType.Stage2);
                        break;
                    case "Stage2":
                        coreManager.sceneLoadManager.OpenScene(SceneType.EndingScene);
                        break;
                }
            }
        }
        
        public void OnConsoleInteracted(StageDoorCondition condition, Stage stage)
        {
            condition.isConditionMet = true;
            stage.CheckAllDoors();
        }

        public void OnEnemyDefeated(GameObject enemy, Stage stage)
        {
            foreach (var door in stage.doors)
            {
                foreach (var condition in door.conditions)
                {
                    if (condition.conditionType == StageDoorConditionType.EnemyDefeat && condition.targetEnemy == enemy)
                    {
                        condition.isConditionMet = true;
                    }
                }
            }
            stage.CheckAllDoors();
        }

        public void OnPlayerReached(Transform location, Stage stage)
        {
            foreach (var door in stage.doors)
            {
                foreach (var condition in door.conditions)
                {
                    if (condition.conditionType == StageDoorConditionType.LocationReached && condition.targetLocation == location)
                    {
                        condition.isConditionMet = true;
                    }
                }
            }
            stage.CheckAllDoors();
        }
    }
}