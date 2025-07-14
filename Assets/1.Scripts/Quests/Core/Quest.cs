using System;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using _1.Scripts.Quests.Data;
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
        }

        public void ResumeQuest(int index, QuestInfo info, Console[] consoles)
        {
            if (currentObjectiveIndex >= Objectives.Count) { isCompleted = true; return; }
            currentObjectiveIndex = index;
            for (var i = 0; i < Objectives.Count; i++)
            {
                Objectives[i].currentAmount = info.progresses[i];
                if (i < currentObjectiveIndex)
                {
                    foreach(var console in consoles) if(console.Id == i) console.OpenDoors();
                    Objectives[i].Deactivate();
                }
                else Objectives[i].Activate();
            }
            CurrentObjective = Objectives[currentObjectiveIndex];
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
                
                currentObjectiveIndex++;
                CoreManager.Instance.gameManager.Player.PlayerCondition.UpdateLastSavedTransform();
                CoreManager.Instance.SaveData_QueuedAsync();
                if (currentObjectiveIndex < data.objectives.Count)
                {
                    CurrentObjective = Objectives[currentObjectiveIndex];
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