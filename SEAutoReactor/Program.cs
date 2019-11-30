using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    internal sealed class Program : MyGridProgram
    {
        private const string BatteryGroupName = "Javelin Batteries";
        private const string ReactorName = "Javelin Reactor";


        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            IMyBlockGroup batteries = GridTerminalSystem.GetBlockGroupWithName(BatteryGroupName);
            if(batteries == null)
            {
                Echo("No batteries found.");
                return;
            }

            IMyReactor reactor = GridTerminalSystem.GetBlockWithName(ReactorName) as IMyReactor;

            if(reactor == null)
            {
                Echo("No reactor found.");
                return;
            }

            Echo($"{batteries.Name}: ");
            List<IMyBatteryBlock> batteryBlocks = new List<IMyBatteryBlock>();
            batteries.GetBlocksOfType(batteryBlocks);
            double maxPower = 0.0;
            double currentPower = 0.0;
            double maxOutput = 0.0;
            int count = 0;
            bool startReactor = false;
            foreach (IMyBatteryBlock battery in batteryBlocks.Where(battery => battery.IsFunctional))
            {
                maxPower += battery.MaxStoredPower;
                currentPower += battery.CurrentStoredPower;
                maxOutput += battery.MaxOutput;
                count++;
                double localPercentage = Math.Abs(battery.MaxStoredPower) < 0.0 ? 1 : battery.CurrentStoredPower / battery.MaxStoredPower;
                if (localPercentage < 0.2) startReactor = true;
            }

            double percentage = Math.Abs(maxPower) < 0.0 ? 1 : currentPower / maxPower;
            Echo($"    Stored Power: {Math.Round(percentage * 100, 2)}%");
            Echo($"    Max Output: {Math.Round(maxOutput, 3)} MW");

            if (reactor.IsFunctional)
            {
                if (startReactor)
                {
                    reactor.Enabled = true;
                    foreach (IMyBatteryBlock battery in batteryBlocks.Where(battery => battery.IsFunctional))
                    {
                        battery.ChargeMode = ChargeMode.Recharge;
                    }
                }
                else
                {
                    bool keepReactorRunning = false;
                    foreach (IMyBatteryBlock battery in batteryBlocks.Where(battery => battery.IsFunctional))
                    {
                        double localPercentage = Math.Abs(battery.MaxStoredPower) < 0.0 ? 1 : battery.CurrentStoredPower / battery.MaxStoredPower;
                        if (localPercentage < 0.8)
                        {
                            keepReactorRunning = true;
                        }
                    }

                    if (!keepReactorRunning)
                    {
                        foreach (IMyBatteryBlock battery in batteryBlocks.Where(battery => battery.IsFunctional))
                        {
                            battery.ChargeMode = ChargeMode.Auto;
                        }
                        reactor.Enabled = false;
                    }
                }

                string status = reactor.Enabled ? "" : "not ";
                Echo($"\nReactor is {status}running\n");

            } else
            {
                Echo($"Reactor is not functional.");
            }

            Echo($"{batteries.Name} ({count}):");
            foreach (IMyBatteryBlock battery in batteryBlocks.Where(battery => battery.IsFunctional))
            {
                double localPercentage = Math.Abs(battery.MaxStoredPower) < 0.0 ? 1 : battery.CurrentStoredPower / battery.MaxStoredPower;
                Echo($"    {battery.CustomName} ({Math.Round(localPercentage * 100, 2)}%)");
            }

        }
    }
}
