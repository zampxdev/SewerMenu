using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;

namespace SewerMenu.Features.World
{
    public class NPCFreeze : FeatureBase
    {
        public override string Id => "npcfreeze";
        public override string Name => "Freeze NPCs";
        public override string Description => "Freeze all NPCs in place";
        public override FeatureCategory Category => FeatureCategory.World;

        public override void OnEnable()
        {
            FreezeAllNPCs(true);
            SewerLogger.Debug("NPCFreeze enabled");
        }

        public override void OnDisable()
        {
            FreezeAllNPCs(false);
            SewerLogger.Debug("NPCFreeze disabled");
        }

        public int GetNPCCount()
        {
            try
            {
                var npcs = GameTypes.GetAllNPCs();
                return npcs?.Length ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        public void FreezeAllNPCs(bool freeze)
        {
            SafeExecute(() =>
            {
                var npcs = GameTypes.GetAllNPCs();
                if (npcs == null || npcs.Length == 0)
                {
                    SewerLogger.Debug("No NPCs found to freeze");
                    return;
                }

                int affectedCount = 0;
                foreach (var npc in npcs)
                {
                    if (npc == null) continue;

                    try
                    {
                        var movement = npc.Movement;
                        if (movement != null)
                        {
                            movement.IsPaused = freeze;
                            
                            // NavMeshAgent.isStopped isn't directly accessible in Il2Cpp, use reflection
                            try
                            {
                                var agentProp = movement.GetType().GetProperty("Agent");
                                if (agentProp != null)
                                {
                                    var agent = agentProp.GetValue(movement);
                                    if (agent != null)
                                    {
                                        var isStoppedProp = agent.GetType().GetProperty("isStopped");
                                        if (isStoppedProp != null && isStoppedProp.CanWrite)
                                        {
                                            isStoppedProp.SetValue(agent, freeze);
                                        }
                                    }
                                }
                            }
                            catch { }
                            
                            affectedCount++;
                        }
                    }
                    catch { }
                }

                if (affectedCount > 0)
                {
                    string action = freeze ? "Frozen" : "Unfrozen";
                    SewerLogger.Debug($"{action} {affectedCount} NPCs");
                }
            }, freeze ? "freezing NPCs" : "unfreezing NPCs");
        }

        public void FreezeNPC(Il2CppScheduleOne.NPCs.NPC npc, bool freeze)
        {
            if (npc == null) return;

            try
            {
                var movement = npc.Movement;
                if (movement != null)
                {
                    movement.IsPaused = freeze;
                    
                    // NavMeshAgent.isStopped isn't directly accessible in Il2Cpp, use reflection
                    try
                    {
                        var agentProp = movement.GetType().GetProperty("Agent");
                        if (agentProp != null)
                        {
                            var agent = agentProp.GetValue(movement);
                            if (agent != null)
                            {
                                var isStoppedProp = agent.GetType().GetProperty("isStopped");
                                if (isStoppedProp != null && isStoppedProp.CanWrite)
                                {
                                    isStoppedProp.SetValue(agent, freeze);
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        public void KillAllNPCs()
        {
            SafeExecute(() =>
            {
                var npcs = GameTypes.GetAllNPCs();
                if (npcs == null || npcs.Length == 0) return;

                int killedCount = 0;
                foreach (var npc in npcs)
                {
                    if (npc == null) continue;

                    try
                    {
                        var health = npc.Health;
                        if (health != null)
                        {
                            var killMethod = health.GetType().GetMethod("Kill");
                            if (killMethod != null)
                            {
                                killMethod.Invoke(health, null);
                                killedCount++;
                            }
                        }
                    }
                    catch { }
                }

                if (killedCount > 0)
                    SewerLogger.Success($"Killed {killedCount} NPCs");
            }, "killing NPCs");
        }

        public void TeleportNPCsAway()
        {
            SafeExecute(() =>
            {
                var npcs = GameTypes.GetAllNPCs();
                var playerPos = GameTypes.PlayerPosition;
                
                if (npcs == null || npcs.Length == 0 || playerPos == Vector3.zero) return;

                int movedCount = 0;
                foreach (var npc in npcs)
                {
                    if (npc == null) continue;

                    try
                    {
                        var direction = (npc.transform.position - playerPos).normalized;
                        if (direction == Vector3.zero)
                            direction = Vector3.forward;
                        
                        npc.transform.position = playerPos + direction * 200f;
                        movedCount++;
                    }
                    catch { }
                }

                if (movedCount > 0)
                    SewerLogger.Success($"Teleported {movedCount} NPCs away");
            }, "teleporting NPCs away");
        }
    }
}
