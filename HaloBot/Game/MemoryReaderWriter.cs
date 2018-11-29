using System;
using System.Threading;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace HaloBot
{
    public class MemoryReaderWriter
    {
        private const float PLAYER_Z_CORRECTION = 0.62F;

        private IntPtr hProcessRead;
        private IntPtr hProcessWrite;
        private Thread reader;
        public bool processRunning;

        //debug
        public List<uint> hitEdges;
        public Structures.FLOAT3 hitPoint;

        //BSP
        public bool BSPloaded;
        private Structures.BSP3D_NODE[] bsp3d_nodes;
        private Structures.PLANE[] planes;
        private Structures.LEAF[] leaves;
        private Structures.BSP2D_REFERENCE[] bsp2d_refs;
        private Structures.BSP2D_NODE[] bsp2d_nodes;
        private Structures.SURFACE[] surfaces;
        public Structures.EDGE[] edges;
        public Structures.VERTEX[] vertices;

        private enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        private enum HALO_MEMORY : int
        {
            //BSP STUFF

            //haloce.exe
            MAP_DATA = 0x40440000,
            TAG_REFERENCE_ARRAY = 0x20,
            TAG_REFERENCE_SIZE = 0x20,
            PSBS_STRING = 0x08,
            TAG_POINTER = 0x1C,
            //bsp tag
            BLOCK_POINTERS = 0xB4,



            PLAYER_CONTROL_GLOBALS_POINTER = 0x64C2C4,
            LOOKING_VECTOR_OFFSET = 0x1C,
            CAMERA_POS = 0x00647424,

            CAN_WALK = 0x00622058,
            CLEAR_SHOT = 0x0067F631,
            FLASHLIGHT_ON = 0x007FBA30,
            ZOOM_LEVEL = 0x400008D0, //nonstatic but always in same place anyway?

            OBJECT_TABLE_HEADER = 0x400506B4,
            LOCAL_PLAYER = 0x402AD408,
            STATIC_PLAYER_HEADER = 0x402AAF94,
            OBJECTIVES_LIST = 0X0068C774
        }

        public MemoryReaderWriter()
        {
            BSPloaded = false;
            reader = new Thread(new ThreadStart(MainLoop));
            reader.Name = "GameFinder";
            reader.IsBackground = true;
        }

        public void Start()
        {
            reader.Start();
        }

        #region weps
        public enum WEAPON_TYPES : ushort
        {
            FUEL_ROD_GUN = 0,
            NEEDLER = 1,
            ASSAULT_RIFLE = 2,
            FLAMETHROWER = 3,
            GRAVITY_RIFLE = 4,
            NEEDLER2 = 5,
            PISTOL = 6,
            PLASMA_PISTOL = 7,
            PLASMA_RIFLE = 8,
            ROCKET_LAUNCHER = 9,
            SHOTGUN = 10,
            SNIPER_RIFLE = 11
        }


        public enum WEP_FUEL_ROD_GUN : ushort
        {
            BATTLE_CREEK = 1246,
            SIDEWINDER = 1500,
            DAMNATION = 1081,
            RAT_RACE = 1315,
            PRISONER = 1301,
            HANG_EM_HIGH = 984,
            CHILL_OUT = 1195,
            DERELICT = 1216,
            BOARDING_ACTION = 862,
            BLOOD_GULCH = 1438,
            WIZARD = 1338,
            CHIRON_TL34 = 1179,
            LONGEST = 1205,
            ICE_FIELDS = 1051,
            DEATH_ISLAND = 1069,
            DANGER_CANYON = 1121,
            INFINITY = 1040,
            TIMBERLAND = 1291,
            GEPHYROPHOBIA = 1214
        }


        public enum WEP_NEEDLER : ushort
        {
            BATTLE_CREEK = 496,
            SIDEWINDER = 1327,
            DAMNATION = 1312,
            RAT_RACE = 1373,
            PRISONER = 1359,
            HANG_EM_HIGH = 37,
            CHILL_OUT = 441,
            DERELICT = 430,
            BOARDING_ACTION = 762,
            BLOOD_GULCH = 1773,
            WIZARD = 531,
            CHIRON_TL34 = 1237,
            LONGEST = 1263,
            ICE_FIELDS = 1354,
            DEATH_ISLAND = 1423,
            DANGER_CANYON = 1035,
            INFINITY = 1412,
            TIMBERLAND = 1384,
            GEPHYROPHOBIA = 1288
        }


        public enum WEP_ASSAULT_RIFLE : ushort
        {
            BATTLE_CREEK = 122,
            SIDEWINDER = 190,
            DAMNATION = 480,
            RAT_RACE = 127,
            PRISONER = 120,
            HANG_EM_HIGH = 680,
            CHILL_OUT = 551,
            DERELICT = 550,
            BOARDING_ACTION = 325,
            BLOOD_GULCH = 234,
            WIZARD = 127,
            CHIRON_TL34 = 53,
            LONGEST = 419,
            ICE_FIELDS = 1189,
            DEATH_ISLAND = 1014,
            DANGER_CANYON = 1451,
            INFINITY = 802,
            TIMBERLAND = 692,
            GEPHYROPHOBIA = 999
        }


        public enum WEP_FLAMETHROWER : ushort
        {
            BATTLE_CREEK = 1039,
            SIDEWINDER = 1423,
            DAMNATION = 1004,
            RAT_RACE = 781,
            PRISONER = 936,
            HANG_EM_HIGH = 907,
            CHILL_OUT = 909,
            DERELICT = 730,
            BOARDING_ACTION = 926,
            BLOOD_GULCH = 1361,
            WIZARD = 963,
            CHIRON_TL34 = 811,
            LONGEST = 657,
            ICE_FIELDS = 1277,
            DEATH_ISLAND = 1347,
            DANGER_CANYON = 1186,
            INFINITY = 1112,
            TIMBERLAND = 764,
            GEPHYROPHOBIA = 1375
        }


        public enum WEP_GRAVITY_RIFLE : ushort
        {
            BATTLE_CREEK = 1115,
            SIDEWINDER = 1711,
            DAMNATION = 1215,
            RAT_RACE = 944,
            PRISONER = 1177,
            HANG_EM_HIGH = 1209,
            CHILL_OUT = 1148,
            DERELICT = 1169,
            BOARDING_ACTION = 1171,
            BLOOD_GULCH = 1653,
            WIZARD = 1202,
            CHIRON_TL34 = 964,
            LONGEST = 926,
            ICE_FIELDS = 1511,
            DEATH_ISLAND = 1694,
            DANGER_CANYON = 1570,
            INFINITY = 1559,
            TIMBERLAND = 1539,
            GEPHYROPHOBIA = 1523
        }


        public enum WEP_NEEDLER2 : ushort
        {
            BATTLE_CREEK = 1122,
            SIDEWINDER = 1718,
            DAMNATION = 1222,
            RAT_RACE = 951,
            PRISONER = 1184,
            HANG_EM_HIGH = 1216,
            CHILL_OUT = 1155,
            DERELICT = 1176,
            BOARDING_ACTION = 1178,
            BLOOD_GULCH = 1660,
            WIZARD = 1209,
            CHIRON_TL34 = 971,
            LONGEST = 933,
            ICE_FIELDS = 763,
            DEATH_ISLAND = 1701,
            DANGER_CANYON = 1577,
            INFINITY = 1566,
            TIMBERLAND = 1546,
            GEPHYROPHOBIA = 1530
        }


        public enum WEP_PISTOL : ushort
        {
            BATTLE_CREEK = 794,
            SIDEWINDER = 430,
            DAMNATION = 928,
            RAT_RACE = 366,
            PRISONER = 360,
            HANG_EM_HIGH = 789,
            CHILL_OUT = 751,
            DERELICT = 652,
            BOARDING_ACTION = 1003,
            BLOOD_GULCH = 473,
            WIZARD = 367,
            CHIRON_TL34 = 633,
            LONGEST = 524,
            ICE_FIELDS = 1122,
            DEATH_ISLAND = 1272,
            DANGER_CANYON = 1265,
            INFINITY = 905,
            TIMBERLAND = 859,
            GEPHYROPHOBIA = 914
        }


        public enum WEP_PLASMA_PISTOL : ushort
        {
            BATTLE_CREEK = 869,
            SIDEWINDER = 1559,
            DAMNATION = 790,
            RAT_RACE = 1031,
            PRISONER = 1013,
            HANG_EM_HIGH = 1048,
            CHILL_OUT = 986,
            DERELICT = 921,
            BOARDING_ACTION = 1180,
            BLOOD_GULCH = 1490,
            WIZARD = 1040,
            CHIRON_TL34 = 505,
            LONGEST = 734,
            ICE_FIELDS = 1357,
            DEATH_ISLAND = 1542,
            DANGER_CANYON = 1365,
            INFINITY = 1277,
            TIMBERLAND = 1198,
            GEPHYROPHOBIA = 1073
        }


        public enum WEP_PLASMA_RIFLE : ushort
        {
            BATTLE_CREEK = 396,
            SIDEWINDER = 1130,
            DAMNATION = 691,
            RAT_RACE = 552,
            PRISONER = 544,
            HANG_EM_HIGH = 390,
            CHILL_OUT = 39,
            DERELICT = 1042,
            BOARDING_ACTION = 490,
            BLOOD_GULCH = 1132,
            WIZARD = 626,
            CHIRON_TL34 = 405,
            LONGEST = 316,
            ICE_FIELDS = 1233,
            DEATH_ISLAND = 1500,
            DANGER_CANYON = 977,
            INFINITY = 985,
            TIMBERLAND = 1143,
            GEPHYROPHOBIA = 1163
        }

        public enum WEP_ROCKET_LAUNCHER : ushort
        {
            BATTLE_CREEK = 681,
            SIDEWINDER = 1200,
            DAMNATION = 373,
            RAT_RACE = 1124,
            PRISONER = 771,
            HANG_EM_HIGH = 587,
            CHILL_OUT = 829,
            DERELICT = 833,
            BOARDING_ACTION = 670,
            BLOOD_GULCH = 1223,
            WIZARD = 881,
            CHIRON_TL34 = 741,
            LONGEST = 1013,
            ICE_FIELDS = 1020,
            DEATH_ISLAND = 874,
            DANGER_CANYON = 944,
            INFINITY = 1361,
            TIMBERLAND = 940,
            GEPHYROPHOBIA = 882
        }

        public enum WEP_SHOTGUN : ushort
        {
            BATTLE_CREEK = 1124,
            SIDEWINDER = 1243,
            DAMNATION = 605,
            RAT_RACE = 652,
            PRISONER = 851,
            HANG_EM_HIGH = 302,
            CHILL_OUT = 269,
            DERELICT = 331,
            BOARDING_ACTION = 584,
            BLOOD_GULCH = 1045,
            WIZARD = 794,
            CHIRON_TL34 = 317,
            LONGEST = 64,
            ICE_FIELDS = 944,
            DEATH_ISLAND = 927,
            DANGER_CANYON = 724,
            INFINITY = 694,
            TIMBERLAND = 971,
            GEPHYROPHOBIA = 798
        }

        public enum WEP_SNIPER_RIFLE : ushort
        {
            BATTLE_CREEK = 583,
            SIDEWINDER = 1040,
            DAMNATION = 76,
            RAT_RACE = 1188,
            PRISONER = 676,
            HANG_EM_HIGH = 490,
            CHILL_OUT = 655,
            DERELICT = 89,
            BOARDING_ACTION = 66,
            BLOOD_GULCH = 1254,
            WIZARD = 1211,
            CHIRON_TL34 = 1051,
            LONGEST = 1077,
            ICE_FIELDS = 861,
            DEATH_ISLAND = 1124,
            DANGER_CANYON = 828,
            INFINITY = 1189,
            TIMBERLAND = 1057,
            GEPHYROPHOBIA = 683
        }



        public ushort RemapWeapon(ushort type)
        {
            if (Enum.IsDefined(typeof(WEP_FUEL_ROD_GUN), type))
                return (ushort)WEAPON_TYPES.FUEL_ROD_GUN;
            if (Enum.IsDefined(typeof(WEP_NEEDLER), type))
                return (ushort)WEAPON_TYPES.NEEDLER;
            if (Enum.IsDefined(typeof(WEP_ASSAULT_RIFLE), type))
                return (ushort)WEAPON_TYPES.ASSAULT_RIFLE;
            if (Enum.IsDefined(typeof(WEP_FLAMETHROWER), type))
                return (ushort)WEAPON_TYPES.FLAMETHROWER;
            if (Enum.IsDefined(typeof(WEP_GRAVITY_RIFLE), type))
                return (ushort)WEAPON_TYPES.GRAVITY_RIFLE;
            if (Enum.IsDefined(typeof(WEP_NEEDLER2), type))
                return (ushort)WEAPON_TYPES.NEEDLER2;
            if (Enum.IsDefined(typeof(WEP_PISTOL), type))
                return (ushort)WEAPON_TYPES.PISTOL;
            if (Enum.IsDefined(typeof(WEP_PLASMA_PISTOL), type))
                return (ushort)WEAPON_TYPES.PLASMA_PISTOL;
            if (Enum.IsDefined(typeof(WEP_PLASMA_RIFLE), type))
                return (ushort)WEAPON_TYPES.PLASMA_RIFLE;
            if (Enum.IsDefined(typeof(WEP_ROCKET_LAUNCHER), type))
                return (ushort)WEAPON_TYPES.ROCKET_LAUNCHER;
            if (Enum.IsDefined(typeof(WEP_SHOTGUN), type))
                return (ushort)WEAPON_TYPES.SHOTGUN;
            if (Enum.IsDefined(typeof(WEP_SNIPER_RIFLE), type))
                return (ushort)WEAPON_TYPES.SNIPER_RIFLE;
            return (ushort)WEAPON_TYPES.PISTOL;
        }

        public enum WEAPON_CLIPS : ushort
        {
            FUEL_ROD_GUN = 0,
            NEEDLER = 20,
            ASSAULT_RIFLE = 60,
            FLAMETHROWER = 100,
            GRAVITY_RIFLE = 0,
            NEEDLER2 = 20,
            PISTOL = 12,
            PLASMA_PISTOL = 0,
            PLASMA_RIFLE = 0,
            ROCKET_LAUNCHER = 2,
            SHOTGUN = 12,
            SNIPER_RIFLE = 4
        }
        public enum WEAPON_RESERVES : ushort
        {
            FUEL_ROD_GUN = 0,
            NEEDLER = 80,
            ASSAULT_RIFLE = 600,
            FLAMETHROWER = 600,
            GRAVITY_RIFLE = 0,
            NEEDLER2 = 80,
            PISTOL = 120,
            PLASMA_PISTOL = 0,
            PLASMA_RIFLE = 0,
            ROCKET_LAUNCHER = 8,
            SHOTGUN = 60,
            SNIPER_RIFLE = 24
        }
        #endregion

        #region Properties
        public bool CanWalk
        {
            get
            {
                return (ReadMemory((int)HALO_MEMORY.CAN_WALK, 1)[0] == 1);
            }
        }

        public bool HasClearShot
        {
            get
            {
                byte[] buf = ReadMemory((int)HALO_MEMORY.CLEAR_SHOT, 4);
                return buf[3] == 0xEB && buf[0] == 0;
            }
        }

        public int PlayerCount
        {
            get
            {
                byte[] buf = ReadMemory((int)HALO_MEMORY.STATIC_PLAYER_HEADER + 46, 2);
                return (int)System.BitConverter.ToUInt16(buf, 0);
            }
        }

        public int MaxSlots
        {
            get
            {
                //read the local player struct to get local player index
                byte[] buf = ReadMemory((int)HALO_MEMORY.STATIC_PLAYER_HEADER + 32, 2);
                return (int)System.BitConverter.ToUInt16(buf, 0);
            }
        }

        public int LocalPing
        {
            get
            {
                int myPointer = GetStaticPlayerPointer(LocalIndex);
                byte[] buf = ReadMemory(myPointer + 220, 2);
                return (int)System.BitConverter.ToUInt16(buf, 0);
            }
        }

        public int LocalIndex
        {
            get
            {
                //read the local player struct to get local player index
                byte[] buf = ReadMemory((int)HALO_MEMORY.LOCAL_PLAYER + 0, 2);
                return (int)System.BitConverter.ToUInt16(buf, 0);
            }
        }

        public int LocalObjectIndex
        {
            get
            {
                byte[] buf = ReadMemory((int)HALO_MEMORY.LOCAL_PLAYER + 164, 2);
                return (int)System.BitConverter.ToUInt16(buf, 0);
            }
        }

        public bool LocalCamo
        {
            get
            {
                return PlayerHasCamo(LocalIndex);
            }
        }

        public int LocalStance
        {
            get
            {
                return PlayerStance(LocalIndex);
            }
        }

        public int LocalTeam
        {
            get
            {
                int myPointer = GetStaticPlayerPointer(LocalIndex);
                return GetPlayerTeam(myPointer);
            }
        }

        public float LocalHealth
        {
            get
            {
                return PlayerHealth(LocalIndex);
            }
        }

        public float LocalShield
        {
            get
            {
                return PlayerShield(LocalIndex);
            }
        }

        public Structures.FLOAT3 LocalPosition
        {
            get
            {
                return PlayerPosition(LocalIndex);
            }
        }

        public bool FlashlightOn
        {
            get
            {
                return ReadMemory((int)HALO_MEMORY.FLASHLIGHT_ON, 1)[0] == 1;
            }
        }

        public int ZoomLevel
        {
            get
            {
                int zoom = ReadMemory((int)HALO_MEMORY.ZOOM_LEVEL, 1)[0];
                if (zoom == 255)
                    zoom = 0;
                return zoom;
            }
        }

        public Structures.FLOAT3 CameraPosition
        {
            get
            {
                byte[] positionFloats = ReadMemory((int)HALO_MEMORY.CAMERA_POS, 12);
                return new Structures.FLOAT3(
                    System.BitConverter.ToSingle(positionFloats, 0),
                    System.BitConverter.ToSingle(positionFloats, 4),
                    System.BitConverter.ToSingle(positionFloats, 8));
            }
        }

        public float CameraHorizontalViewAngle
        {
            get
            {
                byte[] rotationFloats = ReadMemory((int)HALO_MEMORY.CAMERA_POS + 0X20, 8);
                Structures.FLOAT3 vecs = new Structures.FLOAT3(
                    System.BitConverter.ToSingle(rotationFloats, 0),
                    System.BitConverter.ToSingle(rotationFloats, 4),
                    0.0F);

                return (float)Math.Atan2((double)vecs.Y, (double)vecs.X);
            }
        }

        public float PlayerHorizontalViewAngle
        {
            get
            {
                byte[] vectorFloats = ReadMemoryByPointer((int)HALO_MEMORY.PLAYER_CONTROL_GLOBALS_POINTER,
                    (int)HALO_MEMORY.LOOKING_VECTOR_OFFSET, 4);
                return System.BitConverter.ToSingle(vectorFloats, 0);
            }
            set
            {
                if (float.IsNaN(value))
                    return;
                WriteMemoryByPointer((int)HALO_MEMORY.PLAYER_CONTROL_GLOBALS_POINTER,
                    (int)HALO_MEMORY.LOOKING_VECTOR_OFFSET, System.BitConverter.GetBytes(value));
            }
        }

        public float PlayerVerticalViewAngle
        {
            get
            {
                byte[] vectorFloats = ReadMemoryByPointer((int)HALO_MEMORY.PLAYER_CONTROL_GLOBALS_POINTER,
                    (int)HALO_MEMORY.LOOKING_VECTOR_OFFSET + 4, 4);
                return System.BitConverter.ToSingle(vectorFloats, 0);
            }
            set
            {
                if (float.IsNaN(value) || value > Math.PI / 2 || value < -Math.PI / 2)
                    return;
                WriteMemoryByPointer((int)HALO_MEMORY.PLAYER_CONTROL_GLOBALS_POINTER,
                    (int)HALO_MEMORY.LOOKING_VECTOR_OFFSET + 4, System.BitConverter.GetBytes(value));
            }
        }

        //0 if weapon1, 1 if weapon2
        public bool PrimaryWeapon
        {
            get
            {
                ushort weapon1Index = GetWeaponIndex(LocalIndex, false);
                ushort primaryIndex = GetPrimaryWeaponIndex(LocalIndex);
                return weapon1Index == primaryIndex;
            }
        }

        public byte LocalGrenadeType
        {
            get
            {
                int myPointer = GetStaticPlayerPointer(LocalIndex);
                byte[] buf = ReadMemory(myPointer + 797, 1);
                return buf[0];
            }
        }

        public byte LocalFragGrenadeCount
        {
            get
            {
                return GetGrenadeCount(LocalIndex, true);
            }
        }
        public byte LocalPlasmaGrenadeCount
        {
            get
            {
                return GetGrenadeCount(LocalIndex, false);
            }
        }

        #endregion

        //------------------------------------------------------------------------
        //------------------------------------------------------------------------
        //------------------------------------------------------------------------

        #region halo_methods

        public byte GetGrenadeCount(int index, bool frag)
        {
            byte[] buf = ReadMemory(GetMasterchiefPointer(index) + (frag ? 798 : 799), 1);
            return buf[0];
        }

        public ushort GetWeaponIndex(int index, bool weapon1)
        {
            byte[] buf = ReadMemory(GetMasterchiefPointer(index) + (weapon1 ? 760 : 764), 2);
            return (ushort)System.BitConverter.ToUInt16(buf, 0);
        }

        public ushort GetPrimaryWeaponIndex(int index)
        {
            byte[] buf = ReadMemory(GetMasterchiefPointer(index) + 280, 2);
            return (ushort)System.BitConverter.ToUInt16(buf, 0);
        }

        public ushort GetWeaponType(int weaponAddress)
        {
            if (weaponAddress == 0)
                return 0;

            byte[] buf = ReadMemory(weaponAddress + 0, 2);
            return RemapWeapon((ushort)System.BitConverter.ToUInt16(buf, 0));
        }


        public float GetWeaponClip(int weaponAddress)
        {
            ushort weaponType = GetWeaponType(weaponAddress);
            bool usesBattery = WeaponUsesBattery(weaponType);

            float result;
            if (usesBattery)
            {
                //get heat
                byte[] buf = ReadMemory(weaponAddress + 292, 4);
                result = 1.0F - (float)System.BitConverter.ToSingle(buf, 0);
            }
            else
            {
                byte[] buf = ReadMemory(weaponAddress + 696, 4);
                result = (float)System.BitConverter.ToUInt32(buf, 0) /
                    (float)WeaponClipSize(weaponType);
            }
            return result;
        }

        public float GetWeaponReserve(int weaponAddress)
        {
            ushort weaponType = GetWeaponType(weaponAddress);
            bool usesBattery = WeaponUsesBattery(weaponType);

            float result;
            if (usesBattery)
            {
                //get battery
                byte[] buf = ReadMemory(weaponAddress + 304, 4);
                result = 1.0F - (float)System.BitConverter.ToSingle(buf, 0);
            }
            else
            {
                byte[] buf = ReadMemory(weaponAddress + 694, 2);
                result = (float)System.BitConverter.ToUInt16(buf, 0) /
                    (float)WeaponReserveSize(weaponType);
            }
            return result;
        }

        public ushort WeaponReserveSize(ushort type)
        {
            if (type == 0)
                return 0;

            String weaponName = Enum.GetName(typeof(WEAPON_TYPES), type);
            return (ushort)Enum.Parse(typeof(WEAPON_RESERVES), weaponName);
        }

        public ushort WeaponClipSize(ushort type)
        {
            if (type == 0)
                return 0;

            String weaponName = Enum.GetName(typeof(WEAPON_TYPES), type);
            return (ushort)Enum.Parse(typeof(WEAPON_CLIPS), weaponName);
        }

        public bool WeaponUsesBattery(ushort type)
        {
            return type == (ushort)WEAPON_TYPES.FUEL_ROD_GUN || type == (ushort)WEAPON_TYPES.PLASMA_PISTOL
                 || type == (ushort)WEAPON_TYPES.PLASMA_RIFLE;
        }

        //------------------------------------------------------------------------

        public Structures.FLOAT3 ObjectivePosition(int index)
        {
            byte[] positionFloats = ReadMemory((int)HALO_MEMORY.OBJECTIVES_LIST + 0xC + index * 0x20, 12);
            return new Structures.FLOAT3(
                System.BitConverter.ToSingle(positionFloats, 0),
                System.BitConverter.ToSingle(positionFloats, 4),
                System.BitConverter.ToSingle(positionFloats, 8));
        }

        public Structures.FLOAT3 PlayerPosition(int index)
        {
            byte[] positionFloats = ReadMemory(GetMasterchiefPointer(index) + 1984 + 40, 12);
            return new Structures.FLOAT3(
                System.BitConverter.ToSingle(positionFloats, 0),
                System.BitConverter.ToSingle(positionFloats, 4),
                System.BitConverter.ToSingle(positionFloats, 8));
        }

        public Structures.FLOAT3 PlayerVelocity(int index)
        {
            byte[] positionFloats = ReadMemory(GetMasterchiefPointer(index) + 104, 12);
            return new Structures.FLOAT3(
                System.BitConverter.ToSingle(positionFloats, 0),
                System.BitConverter.ToSingle(positionFloats, 4),
                System.BitConverter.ToSingle(positionFloats, 8));
        }

        public float PlayerHealth(int index)
        {
            byte[] buf = ReadMemory(GetMasterchiefPointer(index) + 224, 4);
            return (float)System.BitConverter.ToSingle(buf, 0);
        }

        public float PlayerShield(int index)
        {
            byte[] buf = ReadMemory(GetMasterchiefPointer(index) + 228, 4);
            return (float)System.BitConverter.ToSingle(buf, 0);
        }

        public bool PlayerHasCamo(int index)
        {
            byte[] buf = ReadMemory(GetMasterchiefPointer(index) + 516, 1);
            return buf[0] == 0X51;
        }

        public int PlayerStance(int index)
        {
            byte[] buf = ReadMemory(GetMasterchiefPointer(index) + 520, 4);
            return (int)System.BitConverter.ToUInt32(buf, 0);
        }

        public int GetStaticPlayerPointer(int index)
        {
            byte[] buf = ReadMemory((int)HALO_MEMORY.STATIC_PLAYER_HEADER + 52, 4);
            int playerPointer = (int)System.BitConverter.ToUInt32(buf, 0);


            //Debug.WriteLine("Player 0 pointer: " + playerPointer.ToString("X"));

            //each struct static_player is 512 bytes
            return playerPointer += index * 512;
        }

        public ushort GetPlayerObjectID(int playerPointer)
        {
            byte[] buf = ReadMemory(playerPointer + 54, 2);
            return (ushort)System.BitConverter.ToUInt16(buf, 0);
        }

        public ushort GetPlayerObjectIndex(int playerPointer)
        {
            byte[] buf = ReadMemory(playerPointer + 52, 2);
            return (ushort)System.BitConverter.ToUInt16(buf, 0);
        }

        public short GetPlayerId2(int playerPointer)
        {
            byte[] buf = ReadMemory(playerPointer + 2, 2);
            return (short)System.BitConverter.ToInt16(buf, 0);
        }

        //0 = red, 1 = blue
        public int GetPlayerTeam(int playerPointer)
        {
            byte[] buf = ReadMemory(playerPointer + 32, 4);
            return (int)System.BitConverter.ToUInt32(buf, 0);
        }

        public int GetMasterchiefPointer(int index)
        {
            int playerPointer = GetStaticPlayerPointer(index);
            byte[] buf = ReadMemory(playerPointer + 52, 4);
            UInt16 objectIndex = System.BitConverter.ToUInt16(buf, 0);

            //Debug.WriteLine("Player: " + GetObjectOffset(objectIndex).ToString("X"));

            return GetObjectOffset(objectIndex);
        }

        //returns the pointer to the object's data structure given the object index
        public int GetObjectOffset(UInt16 index)
        {
            byte[] buf = ReadMemory((int)HALO_MEMORY.OBJECT_TABLE_HEADER + 52, 4);
            int objectArrayPointer = (int)System.BitConverter.ToUInt32(buf, 0);

            objectArrayPointer += index * 12;

            byte[] offset = ReadMemory(objectArrayPointer + 8, 4);
            return (int)System.BitConverter.ToUInt32(offset, 0);
        }

        public ushort GetObjectID(UInt16 index)
        {
            byte[] buf = ReadMemory((int)HALO_MEMORY.OBJECT_TABLE_HEADER + 52, 4);
            int objectArrayPointer = (int)System.BitConverter.ToUInt32(buf, 0);

            objectArrayPointer += index * 12;

            byte[] offset = ReadMemory(objectArrayPointer + 0, 2);
            return (ushort)System.BitConverter.ToUInt16(offset, 0);
        }

        #endregion

        #region BSP

        //return true if part of the bsp intersects the line between these two points
        public bool BSPIntersectionCheck(Structures.FLOAT3 a, Structures.FLOAT3 b)
        {
            if (!BSPloaded)
                BSPGet();
            if (float.IsNaN(a.X) || float.IsNaN(a.Y) || float.IsNaN(a.Z) || float.IsNaN(b.X) || float.IsNaN(b.Y) || float.IsNaN(b.Z))
            {
                Debug.WriteLine("NaN target location");
                return true;
            }
            uint node = BSPGetContainingNode(0, a, b);
            return BSP3D_IntersectionCheck(node, a, b);
        }

        private bool BSP3D_IntersectionCheck(uint node, Structures.FLOAT3 a, Structures.FLOAT3 b)
        {
            //if the node flagged, check bsp2d refs
            if ((node & 0x80000000) != 0)
            {
                //sometimes halo's choice of BSP planes fails to divide the space
                //-1 means the surface does not exist
                if ((int)node == -1)
                    return false;

                uint leaf = node ^ 0x80000000;
                uint bsp2d_count = leaves[leaf].BSP2DReferenceCount;
                uint bsp2d_firstIndex = leaves[leaf].firstBSP2DReferenceIndex;

                for (uint i = bsp2d_firstIndex; i < bsp2d_firstIndex + bsp2d_count; i++)
                {
                    if (BSP2D_IntersectionCheck(bsp2d_refs[i].BSP2DNodeIndex_f, a, b))
                        return true;
                }

                hitEdges = null;
                return false;
            }

            return BSP3D_IntersectionCheck(bsp3d_nodes[node].backNodeIndex_f, a, b) ||
                BSP3D_IntersectionCheck(bsp3d_nodes[node].frontNodeIndex_f, a, b);
        }

        private bool BSP2D_IntersectionCheck(uint node, Structures.FLOAT3 a, Structures.FLOAT3 b)
        {
            //if the node is flagged, check surfaces
            if ((node & 0x80000000) != 0)
            {
                uint surface = node ^ 0x80000000;
                
                //given an edge follow its forward edge if the surface is on the left side
                //otherwise follow its reverse edge. upon reaching the starting vertex, all
                //a loop is formed.

                uint currentEdge = surfaces[surface].firstEdgeIndex;
                uint terminalVertex = (edges[currentEdge].leftSurfaceIndex == surface) ?
                    edges[currentEdge].startVertexIndex : edges[currentEdge].endVertexIndex;
                List<uint> polyEdges = new List<uint>(4);
                polyEdges.Add(currentEdge);

                //collect all edges of the polygon
                do
                {
                    //move ahead, add edge
                    if (edges[currentEdge].leftSurfaceIndex == surface)
                        currentEdge = edges[currentEdge].forwardEdgeIndex;
                    else
                        currentEdge = edges[currentEdge].reverseEdgeIndex;
                    polyEdges.Add(currentEdge);

                }
                while (edges[currentEdge].startVertexIndex != terminalVertex &&
                    edges[currentEdge].endVertexIndex != terminalVertex);

                //parameterized ray: a + t * d = p; t = 1 --> p = b
                Structures.FLOAT3 d = b - a;

                //calculate t_p to the surface's plane
                uint plane = surfaces[surface].planeIndex;
                if ((plane & 0x80000000) != 0)                  //what does a flagged plane mean?
                    plane ^= 0x80000000;
                Structures.FLOAT3 n = planes[plane].normal;
                float t_p = Structures.DotProduct((vertices[terminalVertex].position - a), n) /
                    Structures.DotProduct(d, n);

                //if the surface intersection point does not lie on our line segment
                //then the surface is non-obstructing
                if (t_p > 1 || t_p < 0)
                    return false;

                //get the point of intersection of the line and plane
                Structures.FLOAT3 p = a + d * t_p;

                //determine which coordinate plane to project to
                float maxMag = Math.Max(Math.Max(Math.Abs(n.X), Math.Abs(n.Y)), Math.Abs(n.Z));
                int proj;
                if (maxMag == Math.Abs(n.X))      //project to yz
                    proj = 0;
                else if (maxMag == Math.Abs(n.Y)) //project to xz
                    proj = 1;
                else                    //project to xy
                    proj = 2;

                //count how many edges crossed by arbitrary p vector (use x-axis)
                int crossTimes = 0;
                foreach (uint polyEdge in polyEdges)
                {
                    //project the edges and intersection point
                    uint startVertIndex = edges[polyEdge].startVertexIndex;
                    uint endVertIndex = edges[polyEdge].endVertexIndex;
                    Structures.FLOAT3 start3 = vertices[startVertIndex].position;
                    Structures.FLOAT3 end3 = vertices[endVertIndex].position;
                    Structures.FLOAT2 end = Structures.FLOAT3.ToFLOAT2(end3, proj);
                    Structures.FLOAT2 start = Structures.FLOAT3.ToFLOAT2(start3, proj);
                    start -= Structures.FLOAT3.ToFLOAT2(p, proj);
                    end -= Structures.FLOAT3.ToFLOAT2(p, proj);

                    if (Math.Sign(start.Y)!= Math.Sign(end.Y))
                    {
                        if (start.X > 0 && end.X > 0)
                            crossTimes++;
                        else if (start.X > 0 || end.X > 0)
                        {
                            float xIntercept = start.X - start.Y * (end.X - start.X) / (end.Y - start.Y);
                            if (xIntercept > 0)
                                crossTimes++;
                        }
                    }
                }

                //if crossTimes is even then the line missed the polygon
                if (crossTimes % 2 == 0)
                    return false;

                hitPoint = p;
                hitEdges = polyEdges;
                return true;
            }

            return BSP2D_IntersectionCheck(bsp2d_nodes[node].leftChildIndex_f, a, b) ||
                BSP2D_IntersectionCheck(bsp2d_nodes[node].rightChildIndex_f, a, b);
        }

        //responsible for getting the smallest BSP node that contains and does not divide
        //the two given positions
        private uint BSPGetContainingNode(uint parent, Structures.FLOAT3 a, Structures.FLOAT3 b)
        {
            //first see if parent is a leaf by checking the 32nd bit
            if ((parent & 0x80000000) != 0)
                return parent;

            //check which side of the plane each point is on
            float sideA = pointOnPlane(bsp3d_nodes[parent].planeIndex, a);
            float sideB = pointOnPlane(bsp3d_nodes[parent].planeIndex, b);

            if (sideA > 0 && sideB > 0)
            {
                //both on front side
                return BSPGetContainingNode(bsp3d_nodes[parent].frontNodeIndex_f, a, b);
            }
            else if (sideA < 0 && sideB < 0)
            {
                //both on back side
                return BSPGetContainingNode(bsp3d_nodes[parent].backNodeIndex_f, a, b);
            }
            else
            {
                //on different sides or both lie exactly on the plane
                return parent;
            }

        }

        // returns float proportional to the cosine of the angle between the plane's
        // normal and the given point. If the returned value is less than 0, then the
        // point is behind the plane. If greater, the point is in front. If equal, the
        // point lies on the plane
        private float pointOnPlane(uint plane, Structures.FLOAT3 pos)
        {
            // d * (i, j, k) gives a point on the plane
            Structures.FLOAT3 point = planes[plane].normal * planes[plane].d;
            //subtract point from pos to get vector to pos
            Structures.FLOAT3 vec = pos - point;
            //dot vec with normal
            return Structures.DotProduct(vec, planes[plane].normal);
        }

        public void BSPGet()
        {
            BSPloaded = false;
            byte[] chunkBuffer = new byte[4];

            //find the bsp tag
            int tagReferencePointer = (int)HALO_MEMORY.MAP_DATA + (int)HALO_MEMORY.TAG_REFERENCE_ARRAY;
            for (; ; tagReferencePointer += (int)HALO_MEMORY.TAG_REFERENCE_SIZE)
            {
                ReadMemoryNoBuf(tagReferencePointer + (int)HALO_MEMORY.PSBS_STRING, 4, ref chunkBuffer);
                if (chunkBuffer[0] == 112 && chunkBuffer[1] == 115 &&
                        chunkBuffer[2] == 98 && chunkBuffer[3] == 115)
                {
                    //Debug.WriteLine("found sbsp tag reference");
                    break;
                }
            }
            ReadMemoryNoBuf(tagReferencePointer + (int)HALO_MEMORY.TAG_POINTER, 4, ref chunkBuffer);
            int BSPTagPointer = (int)System.BitConverter.ToInt32(chunkBuffer, 0);

            //Debug.WriteLine("sbsp tag data pointer: " + BSPTagPointer.ToString("X"));

            //get the data arrays
            int blockPointers = ReadMemoryInt(BSPTagPointer + (int)HALO_MEMORY.BLOCK_POINTERS, ref chunkBuffer);

            //data arrays
            BSPGet_BSP3D_NODES(ReadMemoryInt(blockPointers + 0x00, ref chunkBuffer),
                               ReadMemoryInt(blockPointers + 0x04, ref chunkBuffer));
            
            BSPGet_PLANES(ReadMemoryInt(blockPointers + 0x0C, ref chunkBuffer),
                          ReadMemoryInt(blockPointers + 0x10, ref chunkBuffer));

            BSPGet_LEAVES(ReadMemoryInt(blockPointers + 0x18, ref chunkBuffer),
                          ReadMemoryInt(blockPointers + 0x1C, ref chunkBuffer));

            BSPGet_BSP2D_REFS(ReadMemoryInt(blockPointers + 0x24, ref chunkBuffer),
                              ReadMemoryInt(blockPointers + 0x28, ref chunkBuffer));

            BSPGet_BSP2D_NODES(ReadMemoryInt(blockPointers + 0x30, ref chunkBuffer),
                               ReadMemoryInt(blockPointers + 0x34, ref chunkBuffer));

            BSPGet_SURFACES(ReadMemoryInt(blockPointers + 0x3C, ref chunkBuffer),
                            ReadMemoryInt(blockPointers + 0x40, ref chunkBuffer));

            BSPGet_EDGES(ReadMemoryInt(blockPointers + 0x48, ref chunkBuffer),
                         ReadMemoryInt(blockPointers + 0x4C, ref chunkBuffer));

            BSPGet_VERTICES(ReadMemoryInt(blockPointers + 0x54, ref chunkBuffer),
                            ReadMemoryInt(blockPointers + 0x58, ref chunkBuffer));

            BSPloaded = true;
        }

        private void BSPGet_BSP3D_NODES(int count, int pointer)
        {
            //Debug.WriteLine("Reading " + count + " BSP3D NODES at " + pointer.ToString("X"));

            int size = 0x0C;
            bsp3d_nodes = new Structures.BSP3D_NODE[count];
            
            byte[] buf = new byte[size];
            for (int i = 0; i < count; i++)
            {
                int elementPointer = pointer + i * size;
                ReadMemoryNoBuf(elementPointer, size, ref buf);

                bsp3d_nodes[i].planeIndex = (uint)System.BitConverter.ToUInt32(buf, 0x00);
                bsp3d_nodes[i].backNodeIndex_f = (uint)System.BitConverter.ToUInt32(buf, 0x04);
                bsp3d_nodes[i].frontNodeIndex_f = (uint)System.BitConverter.ToUInt32(buf, 0x08);
            }
        }

        private void BSPGet_PLANES(int count, int pointer)
        {
            //Debug.WriteLine("Reading " + count + " PLANES at " + pointer.ToString("X"));

            int size = 0x10;
            planes = new Structures.PLANE[count];

            byte[] buf = new byte[size];
            for (int i = 0; i < count; i++)
            {
                int elementPointer = pointer + i * size;
                ReadMemoryNoBuf(elementPointer, size, ref buf);

                planes[i].normal.X = (float)System.BitConverter.ToSingle(buf, 0x00);
                planes[i].normal.Y = (float)System.BitConverter.ToSingle(buf, 0x04);
                planes[i].normal.Z = (float)System.BitConverter.ToSingle(buf, 0x08);
                planes[i].d = (float)System.BitConverter.ToSingle(buf, 0x0C);
            }
        }

        private void BSPGet_LEAVES(int count, int pointer)
        {
            //Debug.WriteLine("Reading " + count + " LEAVES at " + pointer.ToString("X"));
            
            int size = 0x08;
            leaves = new Structures.LEAF[count];

            byte[] buf = new byte[size];
            for (int i = 0; i < count; i++)
            {
                int elementPointer = pointer + i * size;
                ReadMemoryNoBuf(elementPointer, size, ref buf);

                leaves[i].containsDoubleSidedSurfaces = (ushort)System.BitConverter.ToUInt16(buf, 0x00);
                leaves[i].BSP2DReferenceCount = (ushort)System.BitConverter.ToUInt16(buf, 0x02);
                leaves[i].firstBSP2DReferenceIndex = (uint)System.BitConverter.ToUInt32(buf, 0x04);
            }
        }

        private void BSPGet_BSP2D_REFS(int count, int pointer)
        {
            //Debug.WriteLine("Reading " + count + " BSP2D_REFS at " + pointer.ToString("X"));

            int size = 0x08;
            bsp2d_refs = new Structures.BSP2D_REFERENCE[count];

            byte[] buf = new byte[size];
            for (int i = 0; i < count; i++)
            {
                int elementPointer = pointer + i * size;
                ReadMemoryNoBuf(elementPointer, size, ref buf);

                bsp2d_refs[i].PLANEIndex = System.BitConverter.ToUInt32(buf, 0x00);
                bsp2d_refs[i].BSP2DNodeIndex_f = System.BitConverter.ToUInt32(buf, 0x04);
            }
        }

        private void BSPGet_BSP2D_NODES(int count, int pointer)
        {
            //Debug.WriteLine("Reading " + count + " BSP2D_NODES at " + pointer.ToString("X"));

            int size = 0x14;
            bsp2d_nodes = new Structures.BSP2D_NODE[count];

            byte[] buf = new byte[size];
            for (int i = 0; i < count; i++)
            {
                int elementPointer = pointer + i * size;
                ReadMemoryNoBuf(elementPointer, size, ref buf);

                bsp2d_nodes[i].i = (float)System.BitConverter.ToSingle(buf, 0x00);
                bsp2d_nodes[i].j = (float)System.BitConverter.ToSingle(buf, 0x04);
                bsp2d_nodes[i].d = (float)System.BitConverter.ToSingle(buf, 0x08);
                bsp2d_nodes[i].leftChildIndex_f = (uint)System.BitConverter.ToUInt32(buf, 0x0C);
                bsp2d_nodes[i].rightChildIndex_f = (uint)System.BitConverter.ToUInt32(buf, 0x10);
            }
        }

        private void BSPGet_SURFACES(int count, int pointer)
        {
            //Debug.WriteLine("Reading " + count + " SURFACES at " + pointer.ToString("X"));

            int size = 0x0C;
            surfaces = new Structures.SURFACE[count];

            byte[] buf = new byte[size];
            for (int i = 0; i < count; i++)
            {
                int elementPointer = pointer + i * size;
                ReadMemoryNoBuf(elementPointer, size, ref buf);

                surfaces[i].planeIndex = (uint)System.BitConverter.ToUInt32(buf, 0x00);
                surfaces[i].firstEdgeIndex = (uint)System.BitConverter.ToUInt32(buf, 0x04);
                surfaces[i].flags = buf[8];
                surfaces[i].breakable = buf[9];
                surfaces[i].materialIndex = (ushort)System.BitConverter.ToUInt16(buf, 0x0A);
            }
        }

        private void BSPGet_EDGES(int count, int pointer)
        {
            //Debug.WriteLine("Reading " + count + " EDGES at " + pointer.ToString("X"));

            int size = 0x18;
            edges = new Structures.EDGE[count];

            byte[] buf = new byte[size];
            for (int i = 0; i < count; i++)
            {
                int elementPointer = pointer + i * size;
                ReadMemoryNoBuf(elementPointer, size, ref buf);

                edges[i].startVertexIndex = (uint)System.BitConverter.ToUInt32(buf, 0x00);
                edges[i].endVertexIndex = (uint)System.BitConverter.ToUInt32(buf, 0x04);
                edges[i].forwardEdgeIndex = (uint)System.BitConverter.ToUInt32(buf, 0x08);
                edges[i].reverseEdgeIndex = (uint)System.BitConverter.ToUInt32(buf, 0x0C);
                edges[i].leftSurfaceIndex = (uint)System.BitConverter.ToUInt32(buf, 0x10);
                edges[i].rightSurfaceIndex = (uint)System.BitConverter.ToUInt32(buf, 0x14);
            }
        }

        private void BSPGet_VERTICES(int count, int pointer)
        {
            //Debug.WriteLine("Reading " + count + " VERTICES at " + pointer.ToString("X"));

            int size = 0x10;
            vertices = new Structures.VERTEX[count];

            byte[] buf = new byte[size];
            for (int i = 0; i < count; i++)
            {
                int elementPointer = pointer + i * size;
                ReadMemoryNoBuf(elementPointer, size, ref buf);

                vertices[i].position.X = (float)System.BitConverter.ToSingle(buf, 0x00);
                vertices[i].position.Y = (float)System.BitConverter.ToSingle(buf, 0x04);
                vertices[i].position.Z = (float)System.BitConverter.ToSingle(buf, 0x08);
                vertices[i].firstEdgeIndex = (uint)BitConverter.ToUInt32(buf, 0x0C);
            }
        }

        #endregion BSP


        #region memory_methods

        //converts read in bytes to structs
        public static object RawDeserialize(byte[] rawData, int position, Type anyType)
        {
            int rawsize = Marshal.SizeOf(anyType);
            if (rawsize > rawData.Length)
                return null;
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.Copy(rawData, position, buffer, rawsize);
            object retobj = Marshal.PtrToStructure(buffer, anyType);
            Marshal.FreeHGlobal(buffer);
            return retobj;
        }

        //event sent to Form1 to stop any activities requiring memory reading
        public delegate void ProcessStoppedHandler(object sender, EventArgs e);
        public event ProcessStoppedHandler Stopped;

        //continually checks if the game is running
        private void MainLoop()
        {
            processRunning = false;
            bool newProcessRunning = false;
            while (true)
            {
                newProcessRunning = GetProcess();

                if (!newProcessRunning && processRunning)
                    Stopped(this, new EventArgs());

                processRunning = newProcessRunning;
                Thread.Sleep(100);
            }
        }

        [DllImport("Kernel32.dll")]
        static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        private bool GetProcess()
        {
            Process[] haloProcesses = Process.GetProcessesByName("haloce");

            if (haloProcesses.Length == 0)
            {
                return false;
            }

            hProcessRead = OpenProcess(ProcessAccessFlags.VMRead, true, haloProcesses[0].Id);
            hProcessWrite = OpenProcess(ProcessAccessFlags.VMOperation | ProcessAccessFlags.VMWrite,
                true, haloProcesses[0].Id);
            return true;
        }

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            byte[] buffer, UInt32 nSize, out IntPtr lpNumberOfBytesWritten);

        public bool WriteMemory(int address, byte[] buffer)
        {
            try
            {
                IntPtr numwrote;
                WriteProcessMemory(hProcessWrite, (IntPtr)address, buffer, (uint)buffer.Length, out numwrote);
                return true;
            }
            catch (Win32Exception)
            {
                return false;
            }
        }

        public bool WriteMemoryByPointer(int pointer, int offset, byte[] buffer)
        {
            int address = System.BitConverter.ToInt32(
                ReadMemory((int)pointer, 4), 0);
            return WriteMemory(address + offset, buffer);
        }

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            byte[] buffer, UInt32 dwSize, out IntPtr lpNumberOfBytesRead);

        public byte[] ReadMemory(int address, int bytes)
        {
            byte[] ret = new byte[bytes];
            try
            {
                IntPtr numread;
                ReadProcessMemory(hProcessRead, (IntPtr)address, ret, (uint)bytes, out numread);
            }
            catch (Win32Exception)
            {
                return ret;
            }
            return ret;
        }

        #region specific
        int ReadMemoryInt(int address, ref byte[] ret)
        {
            try
            {
                IntPtr numread;
                ReadProcessMemory(hProcessRead, (IntPtr)address, ret, 4, out numread);

            }
            catch (Win32Exception)
            {
                return 0;
            }
            return (int)System.BitConverter.ToInt32(ret, 0);
        }

        uint ReadMemoryUInt(int address, ref byte[] ret)
        {
            try
            {
                IntPtr numread;
                ReadProcessMemory(hProcessRead, (IntPtr)address, ret, 4, out numread);
            }
            catch (Win32Exception)
            {
                return 0;
            }
            return (uint)System.BitConverter.ToUInt32(ret, 0);
        }
        #endregion

        public int ReadMemoryNoBuf(int address, int bytes, ref byte[] ret)
        {
            IntPtr numread;
            try
            {
                ReadProcessMemory(hProcessRead, (IntPtr)address, ret, (uint)bytes, out numread);
            }
            catch (Win32Exception)
            {
                return 0;
            }
            return (int)numread;
        }

        public byte[] ReadMemoryByPointer(int pointer, int offset, int bytes)
        {
            int address = System.BitConverter.ToInt32(ReadMemory(pointer, 4), 0);
            return ReadMemory(address + offset, bytes);
        }

        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }
        public struct SECURITY_DESCRIPTOR
        {
            public byte Revision;
            public byte Sbz1;
            public ushort Control;
            public uint OffsetOwner;
            public uint OffsetGroup;
            public uint OffsetSacl;
            public uint OffsetDacl;
            public uint OwnerSid;
            public uint GroupSid;
            public uint Sacl;
            public uint Dacl;
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, SECURITY_ATTRIBUTES lpThreadAttributes,
            uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParamter, uint dwCreationFlags,
            out IntPtr lpThreadId);
        public void WriteConsoleOutput(String message)
        {
            //SECURITY_DESCRIPTOR secDes = new SECURITY_DESCRIPTOR;
            //secDes.
            //SECURITY_ATTRIBUTES secAtt = new SECURITY_ATTRIBUTES();
            //secAtt.nLength = sizeof(SECURITY_ATTRIBUTES);
            //secAtt.lpSecurityDescriptor = null;
            //secAtt.bInheritHandle = false;

            //IntPtr lpThreadId;
            //IntPtr hRemoteThread = CreateRemoteThread(hProcessWrite, null, 0, HALO_MEMORY.CONSOLE_OUTPUT, null, 0, out lpThreadId);

        }

        #endregion
    }
}