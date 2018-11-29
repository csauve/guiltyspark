using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HaloBot
{
    public class Structures
    {
        public struct FLOAT3
        {
            public float X;
            public float Y;
            public float Z;

            public FLOAT3(float x, float y, float z)
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
            }

            public static FLOAT3 operator +(FLOAT3 f1, FLOAT3 f2)
            {
                return new FLOAT3(f1.X + f2.X, f1.Y + f2.Y, f1.Z + f2.Z);
            }

            public static FLOAT3 operator -(FLOAT3 f1, FLOAT3 f2)
            {
                return new FLOAT3(f1.X - f2.X, f1.Y - f2.Y, f1.Z - f2.Z);
            }

            public static FLOAT3 operator *(FLOAT3 f1, float scalar)
            {
                return new FLOAT3(f1.X * scalar, f1.Y * scalar, f1.Z * scalar);
            }

            public static FLOAT2 ToFLOAT2(Structures.FLOAT3 p, int proj)
            {
                if (proj == 0)
                    return new FLOAT2(p.Y, p.Z);
                else if (proj == 1)
                    return new FLOAT2(p.X, p.Z);
                else
                    return new FLOAT2(p.X, p.Y);
            }

        }

        public struct FLOAT2
        {
            public float X;
            public float Y;

            public FLOAT2(float x, float y)
            {
                this.X = x;
                this.Y = y;
            }

            public static FLOAT2 operator +(FLOAT2 f1, FLOAT2 f2)
            {
                return new FLOAT2(f1.X + f2.X, f1.Y + f2.Y);
            }

            public static FLOAT2 operator -(FLOAT2 f1, FLOAT2 f2)
            {
                return new FLOAT2(f1.X - f2.X, f1.Y - f2.Y);
            }

            public static FLOAT2 operator *(FLOAT2 f1, float scalar)
            {
                return new FLOAT2(f1.X * scalar, f1.Y * scalar);
            }
        }

        public static float DotProduct(FLOAT3 a, FLOAT3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static float DotProduct(FLOAT2 a, FLOAT2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public struct BSP3D_NODE
        {
            public uint planeIndex;
            public uint backNodeIndex_f;
            public uint frontNodeIndex_f;
        }

        public struct PLANE
        {
            public FLOAT3 normal;
            public float d;
        }

        public struct LEAF
        {
            public ushort containsDoubleSidedSurfaces;
            public ushort BSP2DReferenceCount;
            public uint firstBSP2DReferenceIndex;
        }

        public struct BSP2D_REFERENCE
        {
            public uint PLANEIndex;
            public uint BSP2DNodeIndex_f;
        }

        public struct BSP2D_NODE
        {
            public float i, j, d;
            public uint leftChildIndex_f;
            public uint rightChildIndex_f;
        }

        public struct SURFACE
        {
            public uint planeIndex;
            public uint firstEdgeIndex;
            public byte flags;
            public byte breakable;
            public ushort materialIndex;
        }

        public struct EDGE
        {
            public uint startVertexIndex;
            public uint endVertexIndex;
            public uint forwardEdgeIndex;
            public uint reverseEdgeIndex;
            public uint leftSurfaceIndex;
            public uint rightSurfaceIndex;
        }

        public struct VERTEX
        {
            public FLOAT3 position;
            public uint firstEdgeIndex;
        }

        //public struct Object_Table_Header
        //{
        //   fixed char TName[32];          // 'object'
        //   ushort MaxObjects;        // Maximum number of objects - 0x800(2048 objects)
        //   ushort Size;                  // Size of each object array - 0x0C(12 bytes)
        //   ulong Unknown0;           // always 1?
        //   fixed char Data[4];              // '@t@d' - translates to 'data'?
        //   ushort Max;                  // Max number of objects the game has reached (slots maybe?)
        //   ushort Num;                  // Number of objects in the current game
        //   ushort NextObjectIndex; // Index number of the next object to spawn
        //   ushort NextObjectID;      // ID number of the next object to spawn
        //   ulong FirstObject;          // Pointer to the first object in the table   
        //}

        ////-------------------------------------------
        //public struct Object_Table_Element
        //{
        //   ushort ObjectID;           // Matches up to Object ID in static player table ( for players )
        //   ushort Unknown0;
        //   ushort Unknown1;
        //   ushort Size;                 // Structure size
        //   ulong Offset;       <--8       // Pointer to the object data structure
        //}

        //public struct AWeapon
        //{
            //byte unknown[10];
        //}

        ////-------------------------------------------
        //public struct AMasterchief
        //{
        //   ushort BipdMetaIndex;   // [Biped]characters\cyborg_mp\cyborg_mp
        //   ushort BipdMetaID;      // [Biped]characters\cyborg_mp\cyborg_mp
        //   fixed char Zeros_00[4];
        //   fixed char BitFlags_00[4];
        //   ulong Timer_00;
        //   fixed char BitFlags_01[4];
        //   ulong Timer_01;
        //   fixed char Zeros_01[68];
        //   fixed float m_World[3];       //<------92 bytes
        //   fixed float m_Velocity[3];
        //   fixed float m_LowerRot[3];
        //   fixed float m_Scale[3];
        //   fixed char Zeros_02[12];
        //   ulong LocationID;
        //   ulong Pointer_00;
        //   float xUnknown;
        //   float yUnknown;
        //   float zUnknown;
        //   fixed char Zeros_03[20];
        //   ushort PlayerIndex;            //<-------192 bytes
        //   ushort PlayerID;
        //   ulong Unknown00;
        //   fixed char Zeros_04[4];
        //   ushort AntrMetaIndex; // [Animation Trigger]characters\cyborg\cyborg
        //   ushort AntrMetaID;   // [Animation Trigger]characters\cyborg\cyborg
        //   char BitFlags_02[8];
        //   char Unknown01[8];
        //   float Health;                 //<----224 bytes
        //   float Shield_00;
        //   ulong Zeros_05;
        //   float Unknown02;
        //   ulong Unknown03;
        //   float Unknown04;
        //   float Unknown05;
        //   fixed char Unknown06[24];
        //   ushort VehicleWeaponIndex;                  //<------276 bytes
        //   ushort VehicleWeaponID;
        //   ushort WeaponIndex;        //<--280
        //   ushort WeaponID;
        //   ushort VehicleIndex;                  // Ex: Turret on Warthog
        //   ushort VehicleID;
        //   ushort SeatType;                          //<-------288
        //   fixed char BitFlags_03[2];
        //   ulong Zeros_06;
        //   float Shield_01;
        //   float Flashlight_00;
        //   float Zeros_07;
        //   float Flashlight_01;
        //   fixed char Zeros_08[20];
        //   ulong Unknown07;
        //   fixed char Zeros_09[28];
        //   char Unknown08;
        //   fixed char Unknown10[148];
        //   ulong IsInvisible; // normal = 0x41 invis = 0x51 (bitfield?)     <----516 for byte needed
        //   char IsCrouching;      // crouch = 1, jump = 2                 <----520
        //   fixed char Unknown11[3];                  //this is wrong
        //   fixed char Unknown09[884];
        //   fixed float LeftThigh[13];                //<----1411
        //   fixed float RightThigh[13];
        //   fixed float Pelvis[13];
        //   fixed float LeftCalf[13];
        //   fixed float RightCalf[13];
        //   fixed float Spine[13];
        //   fixed float LeftClavicle[13];
        //   fixed float LeftFoot[13];
        //   fixed float Neck[13];
        //   fixed float RightClavicle[13];
        //   fixed float RightFoot[13];
        //   fixed float Head[13];                     //<----1984
        //   fixed float LeftUpperArm[13];
        //   fixed float RightUpperArm[13];
        //   fixed float LeftLowerArm[13];
        //   fixed float RightLowerArm[13];
        //   fixed float LeftHand[13];
        //   fixed float RightHand[13];
        //}

        ////-------------------------------------------

        //public struct Static_Player_Header
        //{
        //   fixed char TName[32]; // 'players'
        //   ushort MaxSlots; // Max number of slots/players possible
        //   ushort SlotSize; // Size of each Static_Player struct
        //   ulong Unknown; // always 1?
        //   fixed char Data[4]; // '@t@d' - translated as 'data'?
        //   ushort IsInMainMenu; // 0 = in game 1 = in main menu / not in game
        //   ushort SlotsTaken; // or # of players
        //   ushort NextPlayerIndex; // Index # of the next player to join
        //   ushort NextPlayerID; // ID # of the next player to join
        //   ulong FirstPlayer; //<--52 Pointer to the first static player
        //}

        ////-------------------------------------------

        //public struct Static_Player
        //{
        //   ushort PlayerID;            // Starts at 0x70EC
        //   ushort PlayerID2;            // 0xFFFF means nonexistent player?
        //   fixed char PlayerName0[24];           // Unicode / Max - 11 Chars + EOS (12 total)
        //   ulong Unknown0;                     // Always -1 / 0xFFFFFFFF
        //   ulong Team;                // 0 = Red / 1 = Blue
        //   ulong SwapID;              // ObjectID
        //   ushort SwapType;           // 8 = Vehicle / 6 = Weapon
        //   short SwapSeat;                    // Warthog - Driver = 0 / Passenger = 1 / Gunner = 2 / Weapon = -1
        //   ulong RespawnTimer;        // ?????? Counts down when dead, Alive = 0
        //   ulong Unknown1;            // Always 0
        //   ushort ObjectIndex;      <---52
        //   ushort ObjectID;           // Matches against object table
        //   ulong Unknown3;            // Some sort of ID
        //   ulong LocationID;          // This is very, very interesting. BG is split into 25 location ID's. 1 -19
        //   long Unknown4;                     // Always -1 / 0xFFFFFFFF
        //   ulong BulletCount;         // Something to do with bullets increases - weird.
        //   fixed char PlayerName1[24];           // Unicode / Max - 11 Chars + EOS (12 total)
        //   ulong Unknown5;            // 02 00 FF FF
        //   ulong PlayerIndex;
        //   ulong Unknown6;
        //   float SpeedModifier;
        //   fixed char Unknown7[108];
        //   ushort Ping;
        //}

        ////-------------------------------------------

        //public struct ALocal
        //{
        //    ushort PlayerIndex;
        //    ushort PlayerID;
        //    fixed char Unknown00[160];
        //    ushort ObjectIndex;
        //    ushort ObjectID;
        //    char Unknown01;
        //    fixed float m_fRot[3];
        //}
    }
}
