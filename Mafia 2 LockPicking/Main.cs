using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.UI;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NAudio;
using GTA.Native;

namespace LockPicking
{
    public class Main : Script
    {
        public Main()
        {
            Notification.Show("Loaded LockPicking");
            LoadConfig();
            Tick += EachTick;
            Aborted += abort;
            KeyDown += KeyTick;
        }
        static  Vehicle ClosestVeh;
        static int LockStat, EnterTimer;
        static bool Picklocking;
        static Keys PressedKey;

        #region ConfigRegion
        static Keys[] keys = new Keys[5];
        public static ScriptSettings config = ScriptSettings.Load("scripts\\Okoniewitz\\LockPick\\config.ini");
        public static float CopFov;
        public static int CopDistance;

        static void LoadConfig()
        {
            string[] ConfigLines =
            {
                "[KEYS]",
                "UpKey=S",
                "ExitKey=Back",
                "NextKey=E",
                "DestroyWindowKey=Q",
                "LockpickStartKey=F",
                "[SETTINGS]",
                "CopFOV=90f",
                "CopDistance=40"
            };
            if (!File.Exists("scripts\\Okoniewitz\\LockPick\\config.ini")) { File.WriteAllLines("scripts\\Okoniewitz\\LockPick\\config.ini", ConfigLines); }
            keys[0] = config.GetValue<Keys>("KEYS", "UpKey", Keys.S);
            keys[1] = config.GetValue<Keys>("KEYS", "ExitKey", Keys.Back);
            keys[2] = config.GetValue<Keys>("KEYS", "NextKey", Keys.E);
            keys[3] = config.GetValue<Keys>("KEYS", "DestroyWindowKey", Keys.Q);
            keys[4] = config.GetValue<Keys>("KEYS", "LockpickStartKey", Keys.F);
            CopFov = config.GetValue<float>("SETTINGS", "CopFOV", 90f);
            CopDistance = config.GetValue<int>("SETTINGS", "CopDistance", 40);
        }
        #endregion

        static void KeyTick(object sender, KeyEventArgs e)
        {
            PressedKey = e.KeyCode;
        }

        static int TimerTick, DelayPlayer;
        static void EachTick(object sender, EventArgs e)
        {
            NAudio.Wave.WaveChannel32 Channel = new NAudio.Wave.WaveChannel32(new NAudio.Wave.WaveFileReader(".\\scripts\\Okoniewitz\\LockPick\\push0.wav"));
            NAudio.Wave.DirectSoundOut player = new NAudio.Wave.DirectSoundOut();
            if (Game.GameTime > DelayPlayer)
            {
                player.Init(Channel);
                DelayPlayer = Game.GameTime + 150;
            }

            if (Game.GameTime > TimerTick)
            {
                TimerTick = Game.GameTime + 18;
                try
                {
                    ClosestVeh = World.GetClosestVehicle(Game.Player.Character.Position, 10);
                    if (ClosestVeh != World.GetClosestVehicle(Game.Player.Character.Position, 10) && ClosestVeh.ClassType != VehicleClass.Motorcycles && ClosestVeh.ClassType != VehicleClass.Utility && ClosestVeh.ClassType != VehicleClass.Cycles && ClosestVeh.ClassType != VehicleClass.Boats && ClosestVeh.ClassType != VehicleClass.Planes && ClosestVeh.ClassType != VehicleClass.Helicopters)
                    {
                        ClosestVeh.LockStatus = (VehicleLockStatus)LockStat;
                    }

                    if (Picklocking) PickLock(ClosestVeh, player);


                    if ((ClosestVeh.LockStatus == VehicleLockStatus.CanBeBrokenInto || ClosestVeh.LockStatus == VehicleLockStatus.CanBeBrokenIntoPersist) && ClosestVeh.ClassType != VehicleClass.Motorcycles && ClosestVeh.ClassType != VehicleClass.Utility && ClosestVeh.ClassType != VehicleClass.Cycles && ClosestVeh.ClassType != VehicleClass.Boats && ClosestVeh.ClassType != VehicleClass.Planes && ClosestVeh.ClassType != VehicleClass.Helicopters)
                    {
                        LockStat = (int)ClosestVeh.LockStatus;
                        ClosestVeh.LockStatus = VehicleLockStatus.Locked;
                    }

                    if (!ClosestVeh.Windows.AllWindowsIntact) { ClosestVeh.LockStatus = VehicleLockStatus.Unlocked; LockStat = (int)VehicleLockStatus.Unlocked; }
                    foreach (VehicleDoor door in ClosestVeh.Doors)
                    {
                        if (door.IsOpen)
                        {
                            ClosestVeh.LockStatus = VehicleLockStatus.Unlocked;
                            LockStat = (int)VehicleLockStatus.Unlocked;
                        }
                    }

                    if (Game.Player.Character.IsTryingToEnterALockedVehicle)
                    {
                        if (EnterTimer == 0) EnterTimer = Game.GameTime + 1000;
                        if (EnterTimer <= Game.GameTime && (Game.IsKeyPressed(keys[4])||Game.IsControlPressed(GTA.Control.Enter)))
                        {
                            PinNow = 0;
                            PickLoc = 4;
                            Rot = 3;
                            int[] Randoms = new int[4];
                            Randoms[0] = new Random().Next(607, 629);
                            Wait(3);
                            Randoms[1] = new Random().Next(607, 629);
                            Wait(3);
                            Randoms[2] = new Random().Next(607, 629);
                            Wait(3);
                            Randoms[3] = new Random().Next(607, 629);
                            Pins = new (int, float, float)[] { (0, 629, Randoms[0]), (1, 629, Randoms[1]), (2, 629, Randoms[2]), (3, 629, Randoms[3]) };
                            AnimDelay = Game.GameTime + 1200;
                            InAnim = false;
                            Picklocking = true;
                            Game.Player.Character.Task.PlayAnimation("amb@prop_human_parking_meter@male@enter", "enter");
                        }
                        if (EnterTimer <= Game.GameTime && (Game.IsKeyPressed(keys[3])||Game.IsControlPressed(GTA.Control.Reload)))
                        {
                            ClosestVeh.LockStatus = VehicleLockStatus.CanBeBrokenInto;
                        }
                    }
                    else EnterTimer = 0;
                }
                catch { }
            }
        }

        //static int DelayTimer;
        public static Model[] CopModels =
{
            "s_m_y_ranger_01",
            "s_m_y_sheriff_01",
            "s_m_y_cop_01",
            "s_f_y_sheriff_01",
            "s_f_y_cop_01",
            "s_m_y_hwaycop_01",

            "0x15F8700D",
            "0x5E3DA4A4",
            "0x9AB35F63",
            "0x739B1EF5",
            "0x8D8F1B10",
            "0x5CDEF405",
            "0x7B8B434B",
            "0x1AE8BB58",
            "0x4161D042",
            "0xB144F9B9",
            "0x9FC7F637",
            "0xEF7135AE",
            "0x625D6958",
            "0x63858A4A",
            "0x95C76ECD",
            "0xCDEF5408",
            "0xDA2C984E",
            "0x56C96FC6",
            "0x625D6958",
            "0xF161D212",
            "0x2930C1AB",
            "0xACCCBDB6",
            "0x709220C7",
            "0x7FA2F024",
            "0x27B3AD75",
            "0x1E9314A2",
            "0xD768B228",
            "0x2EFEAFD5",
            "0x613E626C",
            "0x5E3DA4A4"
        };
        private static int DelayTimer;
        private static bool InAnim;
        private static int AnimDelay;
        private static (int, float,float)[] Pins = new (int, float,float)[] { (0, 629, 0), (1, 629, 0), (2, 629, 0), (3, 629, 0)};
        private static ushort PinNow;
        private static float Rot=3;
        private static int PickLoc=4;
        private static int EnterDelay;
        public static void PickLock(Vehicle veh, NAudio.Wave.DirectSoundOut player)
        {
            if (Game.GameTime > AnimDelay)
            {
                InAnim = true;
                AnimDelay = Game.GameTime + 2000;
                Game.Player.Character.Task.PlayAnimation("amb@prop_human_parking_meter@male@base", "base");
            }
            Game.Player.CanControlCharacter = false;
            if ((PressedKey == keys[1]||Game.IsControlJustPressed(GTA.Control.Reload))&&InAnim)
            {
                Picklocking = false;
                Game.Player.CanControlCharacter = true;
                Game.Player.Character.Task.PlayAnimation("amb@prop_human_parking_meter@male@exit", "exit");
            }

            if (InAnim)
            {
                Ped[] peds = World.GetNearbyPeds(Game.Player.Character.Position, 40f);
                foreach(Ped ped in peds)
                {
                    if (CopModels.Contains(ped.Model) && Visible(Game.Player.Character, ped, CopFov,CopDistance, CopFov) && Game.Player.WantedLevel == 0)
                    {

                        Game.Player.WantedLevel = 1;
                        Game.Player.Character.PlayAmbientSpeech("SPOT_POLICE", SpeechModifier.Force);

                    }
                }
                if (Game.GameTime > DelayTimer)
                {
                    if (Game.IsKeyPressed(keys[0])||Game.IsControlPressed(GTA.Control.Sprint))
                    {
                        if (Rot > -16)
                        {
                            Rot -= 0.30f;
                            if (676 + Rot * 1.2f - 49 < 629)
                            {
                                if (player.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
                                {
                                    player.Play();
                                }
                            }
                        }
                    }
                    else
                    {
                        Rot += 0.18f;
                        if (Rot > 3) { Rot = 3; }
                    }
                }
                bool Green = false;

                if (Math.Abs(Pins[PinNow].Item3 - Pins[PinNow].Item2) < 2)
                {
                    Green = true;
                }
                else Green = false;

                if (PressedKey == keys[2]||(Game.IsControlPressed(GTA.Control.Enter)&&Game.GameTime>EnterDelay))
                {
                    EnterDelay = Game.GameTime + 300;
                    PressedKey = Keys.None;
                    DelayTimer = Game.GameTime + 100;

                    NAudio.Wave.WaveChannel32 Channel = new NAudio.Wave.WaveChannel32(new NAudio.Wave.WaveFileReader(".\\scripts\\Okoniewitz\\LockPick\\next.wav"));
                    NAudio.Wave.DirectSoundOut playa = new NAudio.Wave.DirectSoundOut();
                    playa.Init(Channel);
                    playa.Play();
                    if (Math.Abs(Pins[PinNow].Item3 - Pins[PinNow].Item2) < 2.5)
                    {
                        playa.Play();
                        Green = false;
                        PinNow++;
                        Rot = 3;
                        PickLoc = -(35 * PinNow);
                        if (PinNow == 4)
                        {
                            Picklocking = false;
                            Game.Player.CanControlCharacter = true;
                            veh.LockStatus = VehicleLockStatus.Unlocked;
                            Game.Player.Character.Task.PlayAnimation("amb@prop_human_parking_meter@male@exit", "exit");
                        }
                    }
                    else if (PinNow > 0)

                    {
                        Green = false;
                        PinNow--;
                        playa.Play();

                        Rot = 3;
                        PickLoc = -(35 * PinNow);
                    }
                }

                new GTA.UI.CustomSprite(".\\scripts\\Okoniewitz\\LockPick\\Background.png", new SizeF(181, 115), new PointF(19, 582), Color.FromArgb(255, 255, 255, 255)).Draw();
                new GTA.UI.CustomSprite(".\\scripts\\Okoniewitz\\LockPick\\pick.png", new SizeF(181, 115), new PointF(PickLoc, 627), Color.FromArgb(255, 255, 255, 255), Rot).Draw();

                if (Game.GameTime > DelayTimer)
                {
                    switch (PinNow)
                    {
                        case 0:
                            if (Pins[0].Item2 >= 629) Pins[0].Item2 = 629;
                            if (626 + Rot * 1.2f < Pins[0].Item2) Pins[0].Item2 = 627 + Rot * 1.2f; else Pins[0].Item2 += 0.60f;

                            if (Pins[1].Item2 >= 629) Pins[1].Item2 = 629;
                            if (Pins[2].Item2 >= 629) Pins[2].Item2 = 629; else Pins[2].Item2 += 0.60f;
                            if (Pins[3].Item2 >= 629) Pins[3].Item2 = 629; else Pins[3].Item2 += 0.60f;
                            if (634 + Rot * 0.7f < Pins[1].Item2) Pins[1].Item2 = 634 + Rot * 0.7f; else Pins[1].Item2 += 0.60f;
                            break;
                        case 1:
                            if (Pins[1].Item2 >= 629) Pins[1].Item2 = 629;
                            if (626 + Rot * 1.2f < Pins[1].Item2) Pins[1].Item2 = 627 + Rot * 1.2f; else Pins[1].Item2 += 0.60f;

                            if (Pins[2].Item2 >= 629) Pins[2].Item2 = 629;
                            if (Pins[3].Item2 >= 629) Pins[3].Item2 = 629; else Pins[3].Item2 += 0.60f;
                            if (634 + Rot * 0.7f < Pins[2].Item2) Pins[2].Item2 = 634 + Rot * 0.7f; else Pins[2].Item2 += 0.60f;
                            break;
                        case 2:
                            if (Pins[2].Item2 >= 629) Pins[2].Item2 = 629;
                            if (626 + Rot * 1.2f < Pins[2].Item2) Pins[2].Item2 = 627 + Rot * 1.2f; else Pins[2].Item2 += 0.60f;

                            if (Pins[3].Item2 >= 629) Pins[3].Item2 = 629;
                            if (634 + Rot * 0.7f < Pins[3].Item2) Pins[3].Item2 = 634 + Rot * 0.7f; else Pins[3].Item2 += 0.60f;
                            break;
                        case 3:
                            if (Pins[3].Item2 >= 629) Pins[3].Item2 = 629;
                            if (626 + Rot * 1.2f < Pins[3].Item2) Pins[3].Item2 = 627 + Rot * 1.2f; else Pins[3].Item2 += 0.60f;
                            break;
                    }
                }
                CustomSprite[] customSprites = new CustomSprite[]
                {
                new GTA.UI.CustomSprite(".\\scripts\\Okoniewitz\\LockPick\\Pin.png", new SizeF(18, 50), new PointF(152, Pins[0].Item2)),
                new GTA.UI.CustomSprite(".\\scripts\\Okoniewitz\\LockPick\\Pin.png", new SizeF(18, 50), new PointF(115, Pins[1].Item2)),
                new GTA.UI.CustomSprite(".\\scripts\\Okoniewitz\\LockPick\\Pin.png", new SizeF(18, 50), new PointF(81, Pins[2].Item2)),
                new GTA.UI.CustomSprite(".\\scripts\\Okoniewitz\\LockPick\\Pin.png", new SizeF(18, 50), new PointF(47, Pins[3].Item2))
                };

                int CSinc = 0;
                foreach (CustomSprite CS in customSprites)
                {
                    if (CSinc == PinNow && Green) new GTA.UI.CustomSprite(".\\scripts\\Okoniewitz\\LockPick\\GPin.png", new SizeF(18, 50), CS.Position).Draw(); else CS.Draw();
                    CSinc++;
                }
            }
        }

        public static bool Visible(Ped Who, Ped byWhom, float FieldOfView, int MaxDist, float InVehFov)
        {
            float FOV = 0;
            if (byWhom.IsInVehicle()) FOV = InVehFov; else FOV = FieldOfView;
            if (!Who.IsInVehicle())
            {
                Vector3 pos = World.Raycast(byWhom.Bones[Bone.SkelHead].Position, Who.Bones[Bone.SkelHead].Position, IntersectFlags.Everything).HitPosition;
                bool CanSee = (Math.Abs(pos.X - Who.Position.X) <= 0.7f && Math.Abs(pos.Y - Who.Position.Y) <= 0.5f && Math.Abs(pos.Z - Who.Position.Z) <= 0.7f);
                return (Who.Position.DistanceTo(byWhom.Position) < MaxDist && CanSee && Function.Call<bool>(Hash.IS_PED_FACING_PED, byWhom, Who, FOV));
            }
            else return false;
        }

        public static void abort(object sender, EventArgs e)
        {
            Game.Player.CanControlCharacter = true;
        }
    }
}
