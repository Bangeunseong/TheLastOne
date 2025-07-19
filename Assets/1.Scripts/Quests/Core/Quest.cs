using System;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using _1.Scripts.Quests.Data;
using _1.Scripts.UI.InGame.Mission;
using AYellowpaper.SerializedCollections;
using Console = _1.Scripts.Map.Console.Console;

namespace _1.Scripts.Quests.Core
{
    [Serializable] public class Quest
    {
        public QuestData data;
        public int currentObjectiveIndex;
        public SerializedDictionary<int, ObjectiveProgress> Objectives;
        public bool isCompleted;

        public void Initialize()
        {
            Objectives = new SerializedDictionary<int, ObjectiveProgress>();
            foreach(var objective in data.objectives) 
                Objectives.Add(objective.targetID, new ObjectiveProgress{questId = data.questID, data = objective});
        }
        
        public void StartQuest()
        {
            currentObjectiveIndex = 0;
            foreach(var objective in Objectives) objective.Value.Activate();

            var currentObjective = Objectives[currentObjectiveIndex];
            QuestTargetBinder.Instance.SetCurrentTarget(currentObjective.data.targetID);
            CoreManager.Instance.uiManager.InGameUI.MissionUI.AddMission(currentObjective.data.targetID, currentObjective.data.description, currentObjective.currentAmount, currentObjective.data.requiredAmount);
        }

        public void ResumeQuest(QuestInfo info, Console[] consoles)
        {
            currentObjectiveIndex = info.currentObjectiveIndex;
            foreach (var objective in info.progresses)
            {
                Objectives[objective.Key].currentAmount = objective.Value;
                if (Objectives[objective.Key].IsCompleted)
                {
                    foreach (var console in consoles)
                    {
                        Service.Log($"{console.Id}, {Objectives[objective.Key].data.targetID}");
                        if (console.Id == Objectives[objective.Key].data.targetID)
                        {
                            console.OpenDoors();
                        }
                    }
                    Objectives[objective.Key].Deactivate();
                }
                else Objectives[objective.Key].Activate();
            }
            
            if (info.completionList.All(val => val.Value)) { isCompleted = true; return; }
            
            var currentObjective = Objectives[currentObjectiveIndex];
            QuestTargetBinder.Instance.SetCurrentTarget(currentObjective.data.targetID);
            CoreManager.Instance.uiManager.InGameUI.MissionUI.AddMission(currentObjective.data.targetID, currentObjective.data.description, currentObjective.currentAmount, currentObjective.data.requiredAmount);
        }

        public void UpdateObjectiveProgress(int objectiveId)
        {
            var objective = Objectives[objectiveId];
            
            CoreManager.Instance.uiManager.InGameUI.MissionUI.UpdateMissionProgress(
                objective.data.targetID,
                objective.currentAmount,
                objective.data.requiredAmount
            );

            if (!objective.IsCompleted) return;
            
            objective.Deactivate();
            CoreManager.Instance.uiManager.InGameUI.MissionUI.CompleteMission(objective.data.targetID);

            if (Objectives.Any(val => val.Value.IsActivated))
                currentObjectiveIndex = Objectives.FirstOrDefault(val => val.Value.IsActivated).Key;
            else currentObjectiveIndex = -1;
            
            
            CoreManager.Instance.gameManager.Player.PlayerCondition.UpdateLastSavedTransform();
            CoreManager.Instance.SaveData_QueuedAsync();
            
            if (currentObjectiveIndex != -1)
            {
                var currentObjective = Objectives[currentObjectiveIndex];
                QuestTargetBinder.Instance.SetCurrentTarget(currentObjective.data.targetID);
                CoreManager.Instance.uiManager.InGameUI.MissionUI.AddMission(currentObjective.data.targetID, currentObjective.data.description, currentObjective.currentAmount, currentObjective.data.requiredAmount);
            } else 
            {
                Service.Log("Quest Completed!");
                isCompleted = true;
                CoreManager.Instance.gameManager.Player.PlayerCondition.UpdateLastSavedTransform();
                CoreManager.Instance.SaveData_QueuedAsync();
            }
        }
    }
}