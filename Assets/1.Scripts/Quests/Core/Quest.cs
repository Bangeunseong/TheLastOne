using System;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using _1.Scripts.Quests.Data;
using _1.Scripts.UI.InGame.Mission;
using Console = _1.Scripts.Map.Console.Console;

namespace _1.Scripts.Quests.Core
{
    [Serializable] public class Quest
    {
        public QuestData data;
        public int currentObjectiveIndex;
        public List<ObjectiveProgress> Objectives;
        public ObjectiveProgress CurrentObjective;
        public bool isCompleted;

        public void Initialize()
        {
            Objectives = new List<ObjectiveProgress>();
            foreach(var objective in data.objectives) Objectives.Add(new ObjectiveProgress{data = objective});
        }
        
        public void StartQuest()
        {
            currentObjectiveIndex = 0;
            foreach(var objective in Objectives) objective.Activate();
            CurrentObjective = Objectives.First();
            QuestTargetBinder.Instance.SetCurrentTarget(CurrentObjective.data.targetID);
            CoreManager.Instance.uiManager.GetUI<MissionUI>()?.AddMission(CurrentObjective.data.targetID, CurrentObjective.data.description, CurrentObjective.currentAmount, CurrentObjective.data.requiredAmount);
        }

        public void ResumeQuest(int index, QuestInfo info, Console[] consoles)
        {
            currentObjectiveIndex = index;
            for (var i = 0; i < Objectives.Count; i++)
            {
                Objectives[i].currentAmount = info.progresses[i];
                if (Objectives[i].IsCompleted)
                {
                    foreach (var console in consoles)
                    {
                        Service.Log($"{console.Id}, {Objectives[i].data.targetID}");
                        if (console.Id == Objectives[i].data.targetID) { console.OpenDoors(); }
                    }
                        
                    Objectives[i].Deactivate();
                } else Objectives[i].Activate();
            }
            
            if (info.completionList.All(val => val)) { isCompleted = true; return; }
            CurrentObjective = Objectives[currentObjectiveIndex];
            QuestTargetBinder.Instance.SetCurrentTarget(CurrentObjective.data.targetID);
            CoreManager.Instance.uiManager.GetUI<MissionUI>()?.AddMission(CurrentObjective.data.targetID, CurrentObjective.data.description, CurrentObjective.currentAmount, CurrentObjective.data.requiredAmount);
        }

        public void UpdateProgress()
        {
            if (isCompleted) return;
            
            foreach (var objective in Objectives)
            {
                if (objective.IsCompleted && objective.IsActivated) { objective.Deactivate(); }
            }
            
            CoreManager.Instance.uiManager.GetUI<MissionUI>()?.UpdateMissionProgress(
                CurrentObjective.data.targetID,
                CurrentObjective.currentAmount,
                CurrentObjective.data.requiredAmount
            );

            if (!CurrentObjective.IsCompleted) return;
            
            CurrentObjective.Deactivate();
            CoreManager.Instance.uiManager.GetUI<MissionUI>()?.CompleteMission(CurrentObjective.data.targetID);
                
            currentObjectiveIndex++;
            CoreManager.Instance.gameManager.Player.PlayerCondition.UpdateLastSavedTransform();
            CoreManager.Instance.SaveData_QueuedAsync();
            if (currentObjectiveIndex < data.objectives.Count)
            {
                CurrentObjective = Objectives[currentObjectiveIndex];
                QuestTargetBinder.Instance.SetCurrentTarget(CurrentObjective.data.targetID);
                CoreManager.Instance.uiManager.GetUI<MissionUI>()?.AddMission(CurrentObjective.data.targetID, CurrentObjective.data.description, CurrentObjective.currentAmount, CurrentObjective.data.requiredAmount);
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