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
            CoreManager.Instance.uiManager.InGameUI.MissionUI.AddMission(data.questID, CurrentObjective.data.description);
        }

        public void ResumeQuest(int index, QuestInfo info, Console[] consoles)
        {
            if (info.completionList.All(val => val)) { isCompleted = true; return; }
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
            CurrentObjective = Objectives[currentObjectiveIndex];
            CoreManager.Instance.uiManager.InGameUI.MissionUI.AddMission(data.questID, CurrentObjective.data.description);
        }

        public void UpdateProgress()
        {
            foreach (var objective in Objectives)
            {
                if (objective.IsCompleted && objective.IsActivated) { objective.Deactivate(); }
            }
            
            if (CurrentObjective.IsCompleted)
            {
                CurrentObjective.Deactivate();
                CoreManager.Instance.uiManager.InGameUI.MissionUI.CompleteMission(data.questID);
                
                currentObjectiveIndex++;
                CoreManager.Instance.gameManager.Player.PlayerCondition.UpdateLastSavedTransform();
                CoreManager.Instance.SaveData_QueuedAsync();
                if (currentObjectiveIndex < data.objectives.Count)
                {
                    CurrentObjective = Objectives[currentObjectiveIndex];
                    CoreManager.Instance.uiManager.InGameUI.MissionUI.AddMission(data.questID, CurrentObjective.data.description);
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
}